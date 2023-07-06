using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.ADT
{
    public abstract partial class Term : IPrintable
    {
        private Term() { }
        public partial class Variable : Term { }
        public abstract partial class Value : Term { }
        public abstract partial class Cons : Term { }
        public sealed partial class Nil : Term { }

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

        public bool ContextuallyEquals(Term other, State context)
        {
            return (this, other) switch
            {
                (Variable v1, Variable v2) => v1.Reify(context).Equals(v2.Reify(context)),
                (Variable v, Term o) => v.Reify(context).ContextuallyEquals(o, context),
                (Term o, Variable v) => v.Reify(context).ContextuallyEquals(o, context),
                (Value v1, Value v2) => v1.Equals(v2),
                (Cons c1, Cons c2) => c1.Car.Equals(c2.Car) && c1.Cdr.Equals(c2.Cdr),
                (Nil, Nil) => true,
                (_, _) => false
            };
        }

        public Stream Unify(Term other, State context)
        {
            //three possible results:
            //they are equal -- already unified
            //they're variable values that aren't equal -- unification possible
            //they're concrete values that are not equal -- unification impossible

            if (ContextuallyEquals(other, context))
            {
                return Stream.Singleton(context);
            }
            else if (this is Variable v1 && context.Extend(v1, other) is State s1)
            {
                return Stream.Singleton(s1);
            }
            else if (other is Variable v2 && context.Extend(v2, other) is State s2)
            {
                return Stream.Singleton(s2);
            }

            //else

            return Stream.Empty();
        }

        public Stream DisUnify(Term other, State context)
        {
            if (ContextuallyEquals(other, context))
            {
                return Stream.Empty();
            }
            else if (this is Variable v1 && context.Restrict(v1, x => !x.Equals(other)) is State s1)
            {
                return Stream.Singleton(s1);
            }
            else if (other is Variable v2 && context.Restrict(v2, x => !x.Equals(this)) is State s2)
            {
                return Stream.Singleton(s2);
            }

            return Stream.Empty();

        }

        public string ToVerboseString() => this.ToString() ?? "NullString";
        public IEnumerable<string> ToTree() => this.ToTree("", true, false);
        public virtual IEnumerable<string> ToTree(string prefix, bool first, bool last) => new string[]
        {
            last
            ? prefix + IO.LEAVES + this.ToString()
            : prefix + IO.BRANCH + this.ToString()
        };

        #region Implicit Conversions

        public static implicit operator Term(int i) => new Value.TypedValue<int>(i);
        public static implicit operator Term(double d) => new Number(d);
        public static implicit operator Term((int, int) ii) => new Number(ii.Item1, ii.Item2);

        public static implicit operator Term(char c) => ValueFactory.Box(c);
        public static implicit operator Term(bool b) => b ? Term.True : Term.False;
        public static implicit operator Term(string s) => LCons.Truct(s);
        //public static implicit operator Term(string s) => new Seq(s.Select(x => ValueFactory.Box(x)).ToArray());

        #endregion

        #region Operator Overloads

        public static Goals.Goal operator ==(Term left, Term right) => new Goals.Equality(left, right);
        public static Goals.Goal operator !=(Term left, Term right) => new Goals.Disequality(left, right);

        #endregion
    }
}
