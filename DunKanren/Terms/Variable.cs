using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren
{
    public class Variable : Term, IComparable<Variable>
    {
        public readonly string Symbol;

        private readonly int ID;
        public readonly int RecursionLevel;

        public Variable(ref State s, string symbol)
        {
            this.Symbol = symbol;
            this.ID = s.GenerateVariableID();
            this.RecursionLevel = s.RecursionLevel;

            //s.Subs = s.Subs.Add(this, new Instance.Indefinite());
        }

        protected Variable(Variable original)
        {
            this.Symbol = original.Symbol;
            this.ID = original.ID;
            this.RecursionLevel = original.RecursionLevel;
        }

        public override uint Ungroundedness => 1;

        public override bool Equals(Term? other) => other is Variable t && t.ID == ID;
        public override Term Reify(State s) => s.Walk(this);

        public override string ToString() => this.Symbol + "#" + this.RecursionLevel;

        public override int GetHashCode() => this.ID;

        public int CompareTo(Variable? other)
        {
            if (other is null)
            {
                return 1; //apparently everything is greater-than Null
            }

            if (this.RecursionLevel != other.RecursionLevel)
            {
                return this.RecursionLevel.CompareTo(other.RecursionLevel);
            }
            else
            {
                return this.ID.CompareTo(other.ID);
            }
        }
    }
}
