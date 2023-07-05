using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.ADT
{
    public abstract partial class Term
    {
        private Term() { }
        public abstract partial class Variable : Term { }
        public abstract partial class Value : Term { }
        public sealed partial class Nil : Term { }
        public abstract partial class Cons : Term { }

        protected static readonly Term NilValue = new Nil();

        protected T Match<T>(
            Func<Variable, T> fVar,
            Func<Value, T> fVal,
            Func<Cons, T> fCon,
            Func<Term, T> fNil)
        {
            return this switch
            {
                Variable tVar => fVar(tVar),
                Value tVal => fVal(tVal),
                Cons tCon => fCon(tCon),
                _ => fNil(this)
            };
        }


        //public bool TryUnifyWith(State s, Term other, out State result)
        //{
        //    return (this, other) switch
        //    {
        //        (Nil, Nil) => s.tr
        //        (Variable, _) => s.TryUnify(this, other, out result),
        //        (_, Variable) => s.TryUnify(other, this, out result),
        //    };
        //}
    }
}
