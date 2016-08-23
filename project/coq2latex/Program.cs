using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace coq2latex
{
    public static class Program
    {
        static readonly string regexParenthesizedExpression =
            @"(\(((?<BR>\()|(?<-BR>\))|[^()]*)+\)(?(BR)(?!))";
        static readonly Regex regexCoqExpression = new Regex(
            @"\s*?" +
            regexParenthesizedExpression +
            @"|(\w|\'|\\)+)" +
            @"\s*?");

        public static string Bare(this string s)
        {
            return regexCoqExpression.Replace(s, match => new string(' ', match.Length));
        }
        public static IEnumerable<string> SplitBare(this string s, string at)
        {
            string bare = s.Bare();
            var parts = bare.Split(new string[] { at }, StringSplitOptions.None);
            s = at + s;
            foreach (var part in parts)
            {
                s = s.Substring(at.Length);
                yield return s.Substring(0, part.Length);
                s = s.Substring(part.Length);
            }
        }

        static string ReadInput()
        {
            var input = new StringBuilder();
            string line;
            while ((line = Console.ReadLine()) != null)
                input.AppendLine(line);
            return input.ToString();
        }

        static List<RewriteRule> ParseRewriteRules(string input)
        {
            var rules = new List<RewriteRule>();
            var matches = Regex.Matches(input, @"^\s*\(\*\s*coq2latex:\s*(?<lhs>.*?)\s*:=\s*(?<rhs>.*?)\s*\*\)\s*$",
                RegexOptions.Multiline);
            foreach(Match match in matches)
            {
                string lhs = match.Groups["lhs"].Value;
                string rhs = match.Groups["rhs"].Value;
                Debug.WriteLine("RW = " + lhs + " => " + rhs);
                var lhsExpr = Expression.Parse(lhs);
                Func<Expression, string> getArg = ex =>
                {
                    string str = ex.ToString();
                    return str.StartsWith("#") ? str : null;
                };
                rules.Add(new RewriteRule {
                    Head = lhsExpr.Head,
                    Match = lhsExpr.Tail.Select(argEx => getArg(argEx) == null ? argEx.ToString() : null).ToArray(),
                    Rewrite = strs =>
                    {
                        string res = rhs;
                        for (int i = 0; i < lhsExpr.Arity; ++i)
                        {
                            var arg = getArg(lhsExpr.Tail.ElementAt(i));
                            if (arg != null)
                                res = Regex.Replace(res, arg + @"\b", "{" + strs[i] + "}");
                        }
                        return res;
                    }
                });
            }
            return rules;
        }

        static List<string> ParseOriginalNames(string[] inputParts, string defName, string ctorName)
        {
            var regexRule = new Regex(@"Inductive\s+" + defName + @"(\s|\,|\:)");
            var part = inputParts
                .Select(x => x.Trim())
                .LastOrDefault(regexRule.IsMatch);
            if (part == null)
                return null;
            part = part.SplitBare(":=").Last();
            var regexCtor = new Regex(@"^" + ctorName + @"\s*:\s*forall\s*(?<part>.*)");
            part = part
                .SplitBare("|")
                .Select(x => x.Trim())
                .FirstOrDefault(regexCtor.IsMatch);
            if (part == null)
                return null;
            part = regexCtor.Match(part).Groups["part"].Value;
            part = part.SplitBare(",").First();
            var res = regexCoqExpression
                .Matches(Regex.Replace(part, @"\(\*(.*?)\*\)", ""))
                .OfType<Match>()
                .SelectMany(m =>
                {
                    var s = m.Value.Trim();
                    s = s.Split(':')[0].Trim().TrimStart('(');
                    return s.Split().Where(t => t != "");
                })
                .ToList();
            return res;
        }
        static string ParseAlternativeName(string[] inputParts, string defName, string ctorName, string varName)
        {
            var regexRule = new Regex(@"Inductive\s+" + defName + @"(\s|\,|\:)");
            var part = inputParts
                .Select(x => x.Trim())
                .LastOrDefault(regexRule.IsMatch);
            if (part == null)
                return null;
            part = part.SplitBare(":=").Last();
            var regexCtor = new Regex(@"^" + ctorName + @"\s*:");
            part = part
                .SplitBare("|")
                .Select(x => x.Trim())
                .FirstOrDefault(regexCtor.IsMatch);
            var match = Regex.Match(part, @"\b" + varName + @"\(\*(?<alt>.*?)\*\)");
            if (!match.Success)
                return null;
            var altName = match.Groups["alt"].Value;
            Debug.WriteLine("Rename: " + defName + "." + ctorName + "." + varName + " => " + altName);
            return altName;
        }

        static List<Definition> ParseDefinition(string input, string[] inputParts, string defName)
        {
            // get local name
            int localNameDot = defName.LastIndexOf('.') + 1;
            string defNamespace = defName.Substring(0, localNameDot);
            defName = defName.Substring(localNameDot);

            Match defMatch = Regex.Match(input, @"^Inductive " + defName + @" : (?<type>.*?)Prop :=\s*(?<body>.*)$",
                RegexOptions.Multiline);
            Debug.WriteLine(defMatch);
            if (!defMatch.Success)
                return null;
            
            var ctors = defMatch.Groups["body"].Value.Trim().SplitBare(" | ");
            var ctorDefs = new List<Definition>();
            foreach (var ctor in ctors)
            {
                var bound = new List<string>();
                var precond = new List<string>();

                Match ctorMatch = Regex.Match(ctor, @"^(?<name>\S+) : (?<body>.*)$");
                string name = ctorMatch.Groups["name"].Value;
                string body = ctorMatch.Groups["body"].Value;
                // parse variable bindings and premises
                var bodyParts = body.SplitBare(", ").ToList();
                if (bodyParts.Count == 2 && bodyParts[0].StartsWith("forall "))
                {
                    // normalize
                    if (!bodyParts[0].Contains("("))
                        bodyParts[0] = bodyParts[0].Replace("forall ", "forall (") + ")";

                    var matches = regexCoqExpression.Matches(bodyParts[0]).OfType<Match>();
                    foreach (var match in matches.Skip(1))
                    {
                        var sepMatch = Regex.Match(match.Value.Trim(), @"^\((?<vars>.*?) : (?<type>.*)\)$");
                        var vars = sepMatch.Groups["vars"].Value;
                        var type = sepMatch.Groups["type"].Value;
                        if (vars == "_")
                            precond.Add(type);
                        else
                            bound.AddRange(vars.Split());
                    }
                }

                var def = new Definition
                {
                    Name = name,
                    Bound = bound,
                    Premises = precond.Select(Expression.Parse),
                    Conclusion = Expression.Parse(bodyParts.Last())
                };
                // recover orig bound var names
                var boundOrig = ParseOriginalNames(inputParts, defName, name);
                if (boundOrig.Count != bound.Count)
                    throw new FormatException("failure parsing original bound variable names for " + defName + "/" + name + " (count mismatch: " + string.Join(",", bound) + " vs " + string.Join(",", boundOrig) + ")");
                for (int i = 0; i < bound.Count; ++i)
                {
                    var src = bound[i];
                    var dst = boundOrig[i];

                    // get user choices for bound var
                    var udef = ParseAlternativeName(inputParts, defName, name, boundOrig[i]);
                    if (udef != null)
                        dst = udef == @"\" ? @"\" + dst : udef;
                    def = def.Replace(src, dst);
                }
                def = def.EraseNamespaces();

                Debug.WriteLine(def);

                // parse
                ctorDefs.Add(def);
            }
            return ctorDefs;
        }

        static string CreateLatex(Expression expr, List<RewriteRule> rules)
        {
            var matchingRule = rules
                .Where(rule =>
                    rule.Head == expr.Head &&
                    rule.Arity <= expr.Arity &&
                    rule.Match.Select((arg, i) => arg == null || arg == expr.Tail.ElementAt(i).ToString()).All(x => x)
                    )
                .OrderByDescending(rule => rule.Arity)
                .FirstOrDefault();

            var args = expr.Tail.Select(arg => CreateLatex(arg, rules)).ToList();

            if (matchingRule != null)
                args = new string[] { matchingRule.Rewrite(args.Take(matchingRule.Arity).ToArray()) }.Concat(
                    args.Skip(matchingRule.Arity)).ToList();
            else
                args.Insert(0, expr.Head);

            if (args.Count == 1)
                return args[0];
            else
                return args[0] + "(" + string.Join(", ", args.Skip(1)) + ")";
        }

        static string CreateLatex(Definition def, List<RewriteRule> rules)
        {
            var latexPremises = def.Premises.Select(x => CreateLatex(x, rules)).ToList();
            var latexConclusion = CreateLatex(def.Conclusion, rules);

            string indent = "    ";

            // mathpartir
            var res = new StringBuilder();
            res.AppendLine(@"\begin{mathpar}");
            res.AppendLine(@"\inferrule* [Right=" + def.Name + "]");
            res.AppendLine("{");
            if (latexPremises.Count == 0)
                res.AppendLine(indent + "~");
            else
            {
                for (int i = 0; i < latexPremises.Count; ++i)
                    res.AppendLine(indent + latexPremises[i] + (i != latexPremises.Count - 1 ? @" \\" : ""));
            }
            res.AppendLine("}");
            res.AppendLine("{");
            res.AppendLine(indent + latexConclusion);
            res.AppendLine("}");
            res.AppendLine(@"\end{mathpar}");
            return res.ToString();
        }

        static void Main(string[] relations)
        {
            Debug.Listeners.Add(new ConsoleTraceListener(true));
            
            string input = ReadInput();

            string[] inputParts = input.SplitBare(".").ToArray();
            var rewriteRules = ParseRewriteRules(input);
            foreach (var relation in relations)
            {
                Console.WriteLine("% Inductive " + relation);
                var ctorDefs = ParseDefinition(input, inputParts, relation);
                ctorDefs.ForEach(d =>
                {
                    var latex = CreateLatex(d, rewriteRules);
                    Console.WriteLine(latex);
                });
            }
        }
    }
}
