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
        public sealed partial class Variable : Term
        {
            public string Symbol { get; private set; }
            private int _variableID;
            private State _owner;
            public int RecursionLevel => _owner.RecursionLevel;

            public Variable(string symbol, State owner)
            {
                Symbol = symbol;
                _owner = owner;
                _variableID = owner.GenerateVariableID();
            }

            public Term Reify(State context)
            {
                if (context.Walk(this) is Term t
                    && t is Variable v)
                {
                    return v.Reify(context);
                }

                return this;
            }

            public bool Equals(Variable? other)
            {
                return other is Variable
                    && _variableID == other._variableID;
            }
        }
    }
}
