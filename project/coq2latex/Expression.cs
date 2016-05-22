using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace coq2latex
{
    class Expression
    {
        public static Expression Parse(string s)
        {
            s += ")";
            return ParseExpression(ref s);
        }
        static Expression ParseExpression(ref string s)
        {
            var expVar = ParseExpressionStr(ref s);
            if (expVar == null)
                throw new FormatException("expected variable as head of expression");

            List<Expression> tail = new List<Expression>();
            while (s[0] != ')')
            {
                s = s.Substring(1);

                var expArg = ParseExpressionStr(ref s);
                if (expArg != null)
                {
                    tail.Add(new Expression(expArg));
                    continue;
                }

                s = s.Substring(1);
                tail.Add(ParseExpression(ref s));
                s = s.Substring(1);
            }

            return new Expression(expVar, tail);
        }

        static string ParseExpressionStr(ref string s)
        {
            if (s.StartsWith("(") || s.StartsWith(")")) return null;
            string name = new string(s.TakeWhile(c => !char.IsWhiteSpace(c) && c != ')').ToArray());
            s = s.Substring(name.Length);
            return name;
        }

        public Expression(string head) : this(head, Enumerable.Empty<Expression>()) { }
        public Expression(string head, IEnumerable<Expression> tail)
        {
            this.Head = head;
            this.Tail = tail.ToList();
        }

        public string Head { get; private set; }
        public List<Expression> Tail { get; private set; }
        public int Arity
        {
            get { return Tail.Count(); }
        }

        public Expression Replace(string from, string to)
        {
            return new Expression(
                Head == from ? to : Head,
                Tail.Select(x => x.Replace(from, to))
                );
        }

        public Expression EraseNamespaces()
        {
            return new Expression(
                Head.Split('.').Last(),
                Tail.Select(x => x.EraseNamespaces())
                );
        }

        public override string ToString()
        {
            if (Arity == 0)
                return this.Head;
            return "(" + this.Head + " " + string.Join(" ", this.Tail) + ")";
        }
    }
}
