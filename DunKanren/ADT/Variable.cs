using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.ADT
{
    public abstract partial class Term
    {
        public abstract partial class Variable : Term
        {
            public static Free GetFree(State owner, string symbol) => new Free(owner, symbol);
            public static Typed<D> GetTyped<D>(State owner, string symbol)
                where D : Term => new Typed<D>(GetFree(owner, symbol));
            public static Bound<D> GetBound<D>(State owner, string symbol, D definition)
                where D : Term => new Bound<D>(GetFree(owner, symbol), definition);

            protected Variable(State s, string symbol)
            {
                _owner = s;
                Symbol = symbol;
                _variableID = s.GenerateVariableID();
                _restrictedDefinitions = new();
            }

            protected Variable(Variable original)
            {
                _owner = original._owner;
                Symbol = original.Symbol;
                _variableID = original._variableID;
                _restrictedDefinitions = new(original._restrictedDefinitions);
            }

            protected State _owner;
            protected int _variableID;
            protected HashSet<Term> _restrictedDefinitions;
            public string Symbol { get; private set; }
            public abstract Term Binding { get; }
            public bool IsBound => Binding == this;
            public IEnumerable<Term> Restrictions { get => _restrictedDefinitions; }
            public abstract Variable DeepCopy();

            public sealed class Bound<D> : Variable where D : Term
            {
                private D _definition;
                public override Term Binding => _definition;
                public override Variable DeepCopy() => new Bound<D>(this);

                private Bound(Bound<D> original) : base(original)
                {
                    _definition = original._definition;
                }
                public Bound(Typed<D> original, D definition) : base(original)
                {
                    _definition = definition;
                }
                public Bound(Free original, D definition) : base(original)
                {
                    _definition = definition;
                }
            }
            public sealed class Typed<D> : Variable where D : Term
            {
                public override Term Binding => NilValue;
                public override Variable DeepCopy() => new Typed<D>(this);

                private Typed(Typed<D> original) : base(original) { }
                public Typed(Free original) : base(original) { }
            }
            public sealed class Free : Variable
            {
                public override Term Binding { get => this; }
                public override Variable DeepCopy() => new Free(_owner, Symbol);

                public Free(State owner, string symbol) : base(owner, symbol) { }
            }
        }
    }
}
