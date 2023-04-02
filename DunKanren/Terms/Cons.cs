using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren
{
    public class Cons : Term
    {
        public Term Car { get; init; }
        public Term Cdr { get; init; }

        protected Cons()
        {
            this.Car = Term.nil;
            this.Cdr = Term.nil;
        }

        public Cons(Term car, Term cdr)
        {
            this.Car = car;
            this.Cdr = cdr;
        }

        public Cons(Term car, Term cdar, Term cddr, params Term[] more)
        {
            this.Car = car;
            this.Cdr = Cons.Truct(cdar, Cons.Truct(cddr, Cons.TructList(more)));
        }

        public static Cons Truct(Term car, Term cdr) => new Cons(car, cdr);
        public static Term TructList(params Term[] sequence)
        {
            if (sequence.Length == 0)
            {
                return Term.nil;
            }
            else
            {
                return new Cons(sequence[0], Cons.TructList(sequence.Skip(1).ToArray()));
            }
        }

        public static Term Truct(string s) => Cons.TructList(s.Select(x => ValueFactory.Box(x)).ToArray());


        public override Term Dereference(State s)
        {
            return Cons.Truct(this.Car.Dereference(s), this.Cdr.Dereference(s));
        }

        public override bool SameAs(State s, Term other) => other.SameAs(s, this);
        public override bool SameAs(State s, Cons other)
        {
            return this.Car.SameAs(s, other.Car) && this.Cdr.SameAs(s, other.Cdr);
        }

        public override State? UnifyWith(State s, Term other) => other.UnifyWith(s, this);
        public override State? UnifyWith(State s, Cons other)
        {
            State? half = this.Car.UnifyWith(s, other.Car);

            if (half is null)
            {
                return half;
            }
            else
            {
                return this.Cdr.UnifyWith(half, other.Cdr);
            }
        }

        public override string ToString()
        {
            if (this.IsStringy())
            {
                return "\"" + this.ToSubString() + "\"";
            }
            else
            {
                return "(" + this.Car.ToString() + " . " + this.Cdr + ")";
            }

        }

        private string ToSubString()
        {
            return this.Car.ToString() + (this.Cdr as Cons)?.ToSubString() ?? Term.nil.ToString();
        }

        private bool IsStringy()
        {
            return this.Car is Value<char> && (this.Cdr is Nil || ((this.Cdr as Cons)?.IsStringy() ?? false));
        }


        public override IEnumerable<string> ToTree(string prefix, bool first, bool last)
        {
            yield return prefix + IO.BRANCH + this.Car.ToTree(prefix, false, false);
            yield return prefix + IO.LEAVES + this.Cdr.ToTree(prefix, false, true);
        }
    }

    /*
    public class Cons<T1, T2> : Term
        where T1 : Term, new()
        where T2 : Term, new()
    {
        public T1 Car { get; init; }
        public T2 Cdr { get; init; }

        protected Cons()
        {
            this.Car = new T1();
            this.Cdr = new T2();
        }

        public Cons(T1 car, T2 cdr)
        {
            this.Car = car;
            this.Cdr = cdr;
        }


        public override Term Dereference(State s)
        {
            return ConsFactory.Truct(this.Car.Dereference(s), this.Cdr.Dereference(s));
        }

        public override bool SameAs(State s, Term other) => other.SameAs(s, this);
        public override bool SameAs<O1, O2>(State s, Cons<O1, O2> other)
        {
            return this.Car.SameAs(s, other.Car) && this.Cdr.SameAs(s, other.Cdr);
        }

        public override State? UnifyWith(State s, Term other) => other.UnifyWith(s, this);
        public override State? UnifyWith<O1, O2>(State s, Cons<O1, O2> other)
        {
            State? half = this.Car.UnifyWith(s, other.Car);

            if (half is null)
            {
                return half;
            }
            else
            {
                return this.Cdr.UnifyWith(half, other.Cdr);
            }
        }

        public override string ToString()
        {
            return "(" + this.Car.ToString() + " . " + this.Cdr + ")";
        }
    }

    public static class ConsFactory
    {
        public static Cons<T1, T2> Truct<T1, T2>(T1 car, T2 cdr)
            where T1 : Term, new()
            where T2 : Term, new()
        {
            return new Cons<T1, T2>(car, cdr);
        }

        public static Cons<T, Nil> Truct<T>(T t)
            where T : Term, new()
        {
            return new Cons<T, Nil>(t, Term.nil);
        }
    }

    */
    /*
    public abstract class ConsCell : Cons, IMaybe<Cons, Nil>
    {
        public Term Reflect() => this;

        public R Match<R>(Func<Cons, R> caseA, Func<Nil, R> caseB) => caseA(this);

        public void Switch(Action<Cons> caseA, Action<Nil> caseB) => caseA(this);
    }

    public class ConsTail : Nil, IMaybe<Cons, Nil>
    {
        public Term Reflect() => Term.nil;

        public R Match<R>(Func<Cons, R> caseA, Func<Nil, R> caseB) => caseB(Term.nil);

        public void Switch(Action<Cons> caseA, Action<Nil> caseB) => caseB(Term.nil);
    }

    public abstract class ConsList<T> : ConsCell
        where T : Term
    {
        public T Head { get; init; }
        public IMaybe<Cons, Nil> Tail { get; init; }

        public ConsList(T head, ConsList<T> tail) : this(head)
        {
            Head = head;
            Tail = tail;
        }

        public ConsList(T head)
        {
            Head = head;
            Tail = new ConsTail();
        }
    }

    public class ConsString : ConsList<Value<char>>
    {
        public ConsString(Value<char> head, ConsList<Value<char>> tail) : base(head, tail) { }

        public ConsString(Value<char> head) : base(head) { }
    }
    */

    /*
    public abstract class WList<T>
    {
        public abstract WList<T> Append(WList<T> that);
        public abstract WList<U> Flatten<Tu, U>() where Tu : List<U>;
    }

    public class Nil<A> : WList<A>
    {
        public override WList<A> Append(WList<A> that)
        {
            throw new NotImplementedException();
        }

        public override WList<U> Flatten<Tu, U>()
        {
            return new Nil<U>();
        }
    }

    public class Cons<A> : WList<A>
    {
        private A Head;
        WList<A> Tail;

        public override WList<A> Append(WList<A> that)
        {
            throw new NotImplementedException();
        }
        public override WList<U> Flatten<Tu, U>()
        {
            return this.Head.app
        }
    }
    */
}
