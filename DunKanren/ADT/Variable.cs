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
            private State _owner;
            private int _variableID;
            public string Symbol { get; private set; }

            public bool Equals(Variable? other)
            {
                return other is Variable v
                    && Equals(v._variableID, _variableID);
            }

            public Variable(State s, string symbol)
            {
                _owner = s;
                Symbol = symbol;
                _variableID = s.GenerateVariableID();
            }
        }
    }
}
