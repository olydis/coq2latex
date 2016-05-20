using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coq2latex
{
    class Definition
    {
        public string Name { get; set; }
        public IEnumerable<string> Bound { get; set; }
        public IEnumerable<Expression> Premises { get; set; }
        public Expression Conclusion { get; set; }

        public Definition Replace(string from, string to)
        {
            return new Definition
            {
                Name = Name,
                Bound = Bound,
                Premises = Premises.Select(x => x.Replace(from, to)),
                Conclusion = Conclusion.Replace(from, to)
            };
        }

        public override string ToString()
        {
            return string.Format("({0} := ({1}) ({2}) {3})",
                Name, 
                string.Join(",", Bound),
                string.Join(",", Premises), 
                Conclusion);
        }
    }
}
