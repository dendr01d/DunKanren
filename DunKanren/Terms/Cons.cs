using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DunKanren
{
    public abstract class Cons : Term, IEnumerable<Term>
    {
        public abstract Term Car { get; }
        public abstract Term Cdr { get; }
        public override uint Ungroundedness => Car.Ungroundedness + Cdr.Ungroundedness;
        public override bool Equals(Term? other) => other is Cons t && t.Car.Equals(Car) && t.Cdr.Equals(Cdr);
        public override Term Reify(State s) => Truct(s.Walk(Car), s.Walk(Cdr));

        private static bool IsList<T>(Cons c)
            where T : Term
        {
            return c.Car is T && (c.Cdr is Nil || (c.Cdr is Cons cc && IsList<T>(cc)));
        }

        public override string ToString()
        {
            if (IsList<Value<char>>(this))
            {
                return $"{Car}{Cdr}";
            }
            else if (IsList<Nil>(this))
            {
                return Seq<Nil>.Peano.ToInt(this).ToString();
            }
            else if (Cdr is not Cons and not Nil)
            {
                return $"{Car} . {Cdr}";
            }
            else
            {
                return $"{Car}, {Cdr}";
            }
        }

        protected virtual IEnumerable<Term> Enumerate()
        {
            yield return Car;
            yield return Cdr;
        }

        public IEnumerator<Term> GetEnumerator() => Enumerate().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region Static Constructors

        public static Cons Truct<T1, T2>(T1 car, T2 cdr)
            where T1 : Term
            where T2 : Term
        {

            return new Cell<T1, T2>.Pair(car, cdr);
        }

        public static Seq<T>.List Truct<T>(T car, params T[] tail)
            where T : Term
        {
            return tail.Any()
                ? new Seq<T>.List(car, Truct(tail[0], tail[1..]))
                : new Seq<T>.List(car);
        }

        public static Seq<Nil>.Peano Truct(uint value)
        {
            return value > 0
                ? new Seq<Nil>.Peano(NIL, Truct(value - 1))
                : new Seq<Nil>.Peano(NIL);
        }

        public static Seq<Value<char>>.ConsString Truct(string s)
        {
            if (!s.Any()) throw new System.ArgumentException("Can't construct a ConsString out of an empty string");

            return s.Length > 1
                ? new Seq<Value<char>>.ConsString(ValueFactory.Box(s[0]), Truct(s[1..]))
                : new Seq<Value<char>>.ConsString(ValueFactory.Box(s[0]));
        }

        #endregion

        public abstract class Cell<T1, T2> : Cons
            where T1 : Term
            where T2 : Term
        {
            protected T1 _car;
            protected T2 _cdr;

            protected Cell(T1 car, T2 cdr)
            {
                _car = car;
                _cdr = cdr;
            }

            public sealed class Pair : Cell<T1, T2>
            {
                public override T1 Car => _car;
                public override T2 Cdr => _cdr;
                public Pair(T1 car, T2 cdr) : base(car, cdr) { }
            }
        }

        public abstract class Seq<L> : Cell<L, MaybeNil<Seq<L>>>
            where L : Term
        {
            public override L Car => _car;
            public override Term Cdr => _cdr.GetValue();
            protected Seq(L car, Seq<L> cdr) : base(car, new MaybeNil<Seq<L>>(cdr)) { }
            protected Seq(L car) : base(car, new MaybeNil<Seq<L>>()) { }

            protected override IEnumerable<L> Enumerate()
            {
                yield return Car;
                if (!_cdr.IsNil && Cdr is Seq<L> seq)
                {
                    seq.Enumerate();
                }
            }

            // ---------------

            public sealed class List : Seq<L>
            {
                public List(L car, Seq<L> cdr) : base(car, cdr) { }
                public List(L car) : base(car) { }
            }

            public sealed class ConsString : Seq<Value<char>>
            {
                public ConsString(Value<char> car, Seq<Value<char>> cdr) : base(car, cdr) { }
                public ConsString(Value<char> car) : base(car) { }
            }

            public sealed class Peano : Seq<Nil>
            {
                public Peano(Nil car, Peano cdr) : base(car, cdr) { }
                public Peano(Nil car) : base(car) { }

                public static uint ToInt(Cons p)
                {
                    return p.Cdr is Cons p2
                        ? 1 + ToInt(p2)
                        : 0;
                }
            }
        }
    }

    //public class ConsString : ConsList<Value<char>>
    //{
    //    public ConsString(Value<char> car) : base(car) { }
    //    public ConsString(Value<char> car, ConsString cdr) : base(car, cdr) { }

    //    public ConsString(char car) : this(ValueFactory.Box(car)) { }
    //    public ConsString(char car, ConsString cdr) : this(ValueFactory.Box(car), cdr) { }

    //    protected override bool IsString => true;

    //    //public override Term Dereference(State s) => this;

    //    //public override string ToString()
    //    //{
    //    //    StringBuilder sb = new($"\"{this.Car}");

    //    //    return this.Cdr.Deconstruct(
    //    //        r => r.ToSubString(sb),
    //    //        n => $"[{this.Car}]");
    //    //}

    //    //internal override string ToSubString(StringBuilder prior)
    //    //{
    //    //    prior.Append($"{this.Car}");

    //    //    return this.Cdr.Deconstruct(
    //    //        r => r.ToSubString(prior),
    //    //        n => prior.Append('\"').ToString());
    //    //}

    //    public static implicit operator ConsString(Cons<Value<char>, Nil> cell) => new ConsString(cell.Car);
    //}

    //public class ConsList : ConsList<Term>
    //{
    //    public ConsList(Term car) : base(car) { }
    //    public ConsList(Term car, ConsList cdr) : base(car, cdr) { }
    //}

    //public class ConsEmpty : ConsList
    //{
    //    public ConsEmpty() : base(Term.NIL) { }

    //    public override Nil Car { get => Term.NIL; }

    //    public override uint Ungroundedness => Term.NIL.Ungroundedness;
    //    public override bool TermEquals(State s, Term other) => other.TermEquals(s, Term.NIL);

    //    public override string ToString() => Term.NIL.ToString();
    //    public override string ToVerboseString() => Term.NIL.ToVerboseString();
    //}

    //public class LCons : Term
    //{
    //    public Term Car { get; init; }
    //    public Term Cdr { get; init; }

    //    private bool IsList;
    //    private bool IsString;

    //    private LCons()
    //    {
    //        this.Car = Term.NIL;
    //        this.Cdr = Term.NIL;
    //        this.IsList = false;
    //    }

    //    private LCons(Term car) : this()
    //    {
    //        this.Car = car;
    //        this.IsList = true;
    //        this.IsString = car is Value<char>;
    //    }

    //    private LCons(Term car, Term cdr) : this()
    //    {
    //        this.Car = car;
    //        this.Cdr = cdr;
    //        this.IsList = cdr is Nil;
    //        this.IsString = this.IsList && car is Value<char>;
    //    }

    //    //public Cons(Term car, Term cdar, Term cddr, params Term[] more)
    //    //{
    //    //    this.Car = car;
    //    //    this.Cdr = Cons.Truct(cdar, Cons.Truct(cddr, Cons.TructList(more)));
    //    //}

    //    public static LCons Truct(Term car, Term cdr) => new LCons(car, cdr);

    //    public static Term TructList(params Term[] sequence) => sequence.Any() ? GuaranteedCons(sequence) : Term.NIL;

    //    private static LCons GuaranteedCons(params Term[] sequence)
    //    {
    //        if (sequence.Length == 1)
    //        {
    //            return LCons.Truct(sequence[0], Term.NIL);
    //        }
    //        else
    //        {
    //            return LCons.Truct(sequence[0], GuaranteedCons(sequence[1..]));
    //        }
    //    }

    //    public static Term Truct(string s) => GuaranteedCons(s.Select(ValueFactory.Box).ToArray());

    //    public override uint Ungroundedness => this.Car.Ungroundedness + this.Cdr.Ungroundedness;

    //    public override bool TermEquals(State s, Term other) => other.TermEquals(s, this);
    //    public override bool TermEquals(State s, LCons other)
    //    {
    //        return this.Car.TermEquals(s, other.Car) && this.Cdr.TermEquals(s, other.Cdr);
    //    }

    //    public override string ToString()
    //    {
    //        if (this.IsStringy())
    //        {
    //            return $"\"{this.ToSubString()}\"";
    //        }
    //        else if (this.IsListy())
    //        {
    //            return $"[{this.ToSubList()}]";
    //        }
    //        else
    //        {
    //            return "(" + this.Car.ToString() + " . " + this.Cdr + ")";
    //        }

    //    }

    //    private string ToSubString()
    //    {
    //        return this.Car.ToString() + (this.Cdr as LCons)?.ToSubString() ?? Term.NIL.ToString();
    //    }

    //    private bool IsStringy()
    //    {
    //        return this.Car is Value<char> && (this.Cdr is Nil || ((this.Cdr as LCons)?.IsStringy() ?? false));
    //    }

    //    private string ToSubList()
    //    {
    //        return $"{this.Car}, {((this.Cdr as LCons)?.ToSubList() ?? Term.NIL.ToString())}";
    //    }

    //    private bool IsListy()
    //    {
    //        return this.Cdr is Nil || ((this.Cdr as LCons)?.IsListy() ?? false);
    //    }

    //    //public override IEnumerable<string> ToTree(string prefix, bool first, bool last)
    //    //{
    //    //    string parentPrefix = first ? "" : prefix + (last ? IO.LEAVES : IO.HEADER);
    //    //    string childPrefix = first ? "" : prefix + (last ? IO.SPACER : IO.JUMPER);

    //    //    foreach (string line in this.Car.ToTree(parentPrefix, false, false))
    //    //    {
    //    //        yield return line;
    //    //    }

    //    //    foreach (string line in this.Cdr.ToTree(childPrefix, false, true))
    //    //    {
    //    //        yield return line;
    //    //    }

    //    //    //yield return prefix + IO.HEADER + IO.ALONER + this.Car.ToTree(prefix, false, false);
    //    //    //yield return prefix + IO.LEAVES + this.Cdr.ToTree(prefix, false, true);
    //    //}
    //}

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
