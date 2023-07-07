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

        public sealed class Definite : Instance
        {
            public Term Definition { get; private set; }

            public Definite(Term definition)
            {
                Definition = definition;
            }

            public override bool TermEquals(State s, Term other) => Definition.TermEquals(s, other);

            public override string ToString() => Definition.ToString();

            public override bool CongruentWith(Term t) => Definition.Equals(t);
            public override Instance BindTo(Term t) => throw new InvalidOperationException("Can't bind term to definite term instance");
        }

        public sealed class Indefinite : Instance
        {
            private readonly ImmutableHashSet<Predicate<Term>> _restrictions;

            public Indefinite(params Predicate<Term>[] preds)
            {
                _restrictions = preds.ToImmutableHashSet();
            }

            public override bool TermEquals(State s, Term other) => ReferenceEquals(this, other);

            public override string ToString() => $"Indefinite ({_restrictions.Count}r)";

            public override bool CongruentWith(Term t) => _restrictions.All(x => x(t));
            public override Instance BindTo(Term t) => new Definite(t);
            public Indefinite AddRestriction(Predicate<Term> pred)
            {
                return new Indefinite(_restrictions.Append(pred).ToArray());
            }
        }
    }
}
