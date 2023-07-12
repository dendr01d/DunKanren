using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren
{
    public abstract class Constraint : IEquatable<Constraint>
    {
        public Term Boundary { get; init; }
        protected Constraint(Term bound)
        {
            Boundary = bound;
        }

        public abstract bool Check(Term t);
        public abstract bool Equals(Constraint? other);
        public abstract override string ToString();

        public class Inequality : Constraint
        {
            public Inequality(Term t) : base(t) { }
            public override bool Check(Term t) => !t.Equals(Boundary);
            public override bool Equals(Constraint? other) => other is Inequality c && c.Boundary.Equals(Boundary);
            public override string ToString() => $"term != {Boundary}";
        }

    }
}
