using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static DunKanren.ADT.Term.Value;

namespace DunKanren.ADT
{
    public abstract partial class Term
    {
        public sealed partial class Variable : Term
        {
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
