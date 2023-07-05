using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static DunKanren.ADT.Term.Variable;

namespace DunKanren.ADT
{
    public abstract partial class Term
    {
        public abstract partial class Variable : Term
        {
            protected Variable(string symbol)
            {
                Symbol = symbol;
                VariableID = State.GenerateVariableID();
            }

            protected Variable(Variable original)
            {
                Symbol = original.Symbol;
                VariableID = original.VariableID;
            }
            public int VariableID { get; private set; }
            public string Symbol { get; private set; }

            public abstract Term? DereferenceVia(State s);

            public sealed class Bound<D> : Variable
                where D : Term
            {
                public D Assignment { get; private set; }
                public Bound(string symbol, D definition) : base(symbol)
                {
                    Assignment = definition;
                }
                public Bound(Free<D> original, D definition) : base(original)
                {
                    Assignment = definition;
                }
                public Term? Dereference(State s) => s.Subs.Contains(this) ? Assignment : null;
            }
            public sealed class Free<D> : Variable
                where D : Term
            {
                private ImmutableHashSet<Predicate<ADT.Term>> _restrictions;
                public Free(string symbol) : base(symbol)
                {
                    _restrictions = ImmutableHashSet.Create<Predicate<Term>>();
                }
                public Free(Free<D> original, Predicate<ADT.Term> pred) : this(original.Symbol)
                {
                    _restrictions = _restrictions.Add(pred);
                }

                public Bound<D>? TryBind(D value)
                {
                    if (_restrictions.All(x => x(value)))
                    {
                        return new Bound<D>(Symbol, value);
                    }
                    return null;
                }
                public Term? Dereference(State s) => s.Subs.Contains(this) ? this : null;
            }
        }
    }
}
