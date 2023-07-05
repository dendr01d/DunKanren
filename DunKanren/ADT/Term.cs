using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.ADT
{
    public abstract partial class Term : IEquatable<Term>
    {
        private Term() { }
        public partial class Variable : Term, IEquatable<Variable> { }
        public abstract partial class Value : Term, IEquatable<Value> { }
        public abstract partial class Cons : Term, IEquatable<Cons> { }
        private sealed partial class Nil : Term { }

        private T Match<T>(
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

        public bool Equals(Term? other, State context)
        {
            return (this, other) switch
            {
                (Variable v1, Variable v2) => v1 == v2,
                (Variable v, Term o) => v.Reify(context).Equals(o, context),
                (Term o, Variable v) => v.Reify(context).Equals(o, context),
                (Value v1, Value v2) => v1 == v2,
                (Cons c1, Cons c2) => c1.Car == c2.Car && c1.Cdr == c2.Cdr,
            }
        }

        public Term Reify(State s)
        {
            return this switch
            {
                Variable v => s.Walk(v) ?? this,
                Value.TypedValue<ADT.Term> t => t.
                _ => this
            };
        }
    }
}
