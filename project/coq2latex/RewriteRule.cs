using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coq2latex
{
    class RewriteRule
    {
        public string Head { get; set; }
        public string[] Match { get; set; }
        public int Arity { get { return Match.Length; } }
        public Func<string[], string> Rewrite { get; set; }

        public override string ToString()
        {
            var args = Enumerable.Range(0, Arity).Select(x => Match[x] ?? ("#" + x)).ToArray();
            return "Head " + string.Join(" ", args) + " => " + Rewrite(args);
        }
    }
}
