using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections.Immutable;

namespace DunKanren
{
    public abstract class Instance : Term
    {
        private Instance() { }

        public abstract bool CongruentWith(Term t);
        public abstract Instance BindTo(Term t);

        public abstract IEnumerable<Instance> Negate();
        public abstract Instance SubtractInfo(Instance other);

        public static Instance Empty = new Indefinite();

        public sealed class Definite : Instance
        {
            public Term Definition { get; private set; }

            public Definite(Term definition)
            {
                Definition = definition;
            }

            public override uint Ungroundedness => 0;

            public override bool Equals(Term? other) => other is Definite t && t.Definition.Equals(Definition);

            public override string ToString() => Definition.ToString();

            public override bool CongruentWith(Term t) => Definition.Equals(t);
            public override Instance BindTo(Term t) => throw new InvalidOperationException("Can't bind term to definite term instance");
            public override IEnumerable<Instance> Negate()
            {
                yield return new Indefinite(new Constraint.Inequality(Definition));
            }
            public override Instance SubtractInfo(Instance other)
            {
                return (other) switch
                {
                    Definite def => def.Definition.Equals(this.Definition) ? Empty : this,
                    _ => this
                };
            }
        }

        public sealed class Indefinite : Instance
        {
            private readonly ImmutableHashSet<Constraint.Inequality> _restrictions;
            public int RuleCount => _restrictions.Count;

            public Indefinite(params Constraint.Inequality[] preds)
            {
                _restrictions = preds.ToImmutableHashSet();
            }
            public override uint Ungroundedness => 0; //no good way to quantify according to number of restrictions?

            public override bool Equals(Term? other) => other is Indefinite t && t._restrictions.SetEquals(_restrictions);

            public override string ToString() => $"Indefinite ({_restrictions.Count}r)";

            public override bool CongruentWith(Term t) => _restrictions.All(x => x.Check(t));
            public override Instance BindTo(Term t) => new Definite(t);
            public Indefinite AddRestriction(Constraint.Inequality pred)
            {
                return new Indefinite(_restrictions.Append(pred).ToArray());
            }
            public override IEnumerable<Instance> Negate()
            {
                foreach(Constraint.Inequality pred in _restrictions)
                {
                    yield return new Definite(pred.Boundary);
                }
            }
            public override Instance SubtractInfo(Instance other)
            {
                return (other) switch
                {
                    Indefinite indef => new Indefinite(this._restrictions.Except(indef._restrictions).ToArray()),
                    _ => other
                };
            }
        }
    }
}
