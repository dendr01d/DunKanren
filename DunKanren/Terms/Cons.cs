using System.Linq;
using System.Text;

namespace DunKanren
{
    public class Cons<T1, T2> : Term
        where T1 : Term
        where T2 : Term
    {
        public virtual T1 Car { get; init; }
        public virtual T2 Cdr { get; init; }

        public Cons(T1 car, T2 cdr)
        {
            this.Car = car;
            this.Cdr = cdr;
        }

        protected virtual bool IsList => false;
        protected virtual bool IsString => false;

        public override uint Ungroundedness => this.Car.Ungroundedness + this.Cdr.Ungroundedness;

        public override bool TermEquals(State s, Term other) => other.TermEquals(s, this);
        public override bool TermEquals<D1, D2>(State s, Cons<D1, D2> other)
        {
            return this.Car.TermEquals(s, other.Car) && this.Cdr.TermEquals(s, other.Cdr);
        }

        public override string ToString()
        {
            if (this.IsString)
            {
                return $"{this.Car}{this.Cdr}";
            }
            else if (this.IsList)
            {
                return $"{this.Car}, {this.Cdr}";
            }
            else
            {
                return $"( {this.Car} . {this.Cdr} )";
            }
        }
    }

    public class ConsCell : Cons<Term, Term>
    {
        public ConsCell(Term car, Term cdr) : base(car, cdr) { }
    }

    public class ConsList<T> : Cons<T, MaybeNil<ConsList<T>>>
        where T : Term
    {
        public ConsList(T car) : base(car, new()) { }
        public ConsList(T car, ConsList<T> cdr) : base(car, new(cdr)) { }

        protected override bool IsList => true;

        //public override Term Dereference(State s)
        //{
        //    if (this.UngroundedNess > 0)
        //    {
        //        return base.Dereference(s);
        //    }

        //    return this;
        //}

        //public override string ToString()
        //{
        //    StringBuilder sb = new($"[{this.Car}");

        //    return this.Cdr.Deconstruct(
        //        r => r.ToSubString(sb),
        //        n => $"[{this.Car}]");
        //}

        //internal virtual string ToSubString(StringBuilder prior)
        //{
        //    prior.Append($", {this.Car}");

        //    return this.Cdr.Deconstruct(
        //        r => r.ToSubString(prior),
        //        n => prior.Append(']').ToString());
        //}

        public static implicit operator ConsList<T>(Cons<T, Nil> cell) => new ConsList<T>(cell.Car);
    }

    public class ConsString : ConsList<Value<char>>
    {
        public ConsString(Value<char> car) : base(car) { }
        public ConsString(Value<char> car, ConsString cdr) : base(car, cdr) { }

        public ConsString(char car) : this(ValueFactory.Box(car)) { }
        public ConsString(char car, ConsString cdr) : this(ValueFactory.Box(car), cdr) { }

        protected override bool IsString => true;

        //public override Term Dereference(State s) => this;

        //public override string ToString()
        //{
        //    StringBuilder sb = new($"\"{this.Car}");

        //    return this.Cdr.Deconstruct(
        //        r => r.ToSubString(sb),
        //        n => $"[{this.Car}]");
        //}

        //internal override string ToSubString(StringBuilder prior)
        //{
        //    prior.Append($"{this.Car}");

        //    return this.Cdr.Deconstruct(
        //        r => r.ToSubString(prior),
        //        n => prior.Append('\"').ToString());
        //}

        public static implicit operator ConsString(Cons<Value<char>, Nil> cell) => new ConsString(cell.Car);
    }

    public class ConsList : ConsList<Term>
    {
        public ConsList(Term car) : base(car) { }
        public ConsList(Term car, ConsList cdr) : base(car, cdr) { }
    }

    public class ConsEmpty : ConsList
    {
        public ConsEmpty() : base(Term.NIL) { }

        public override Nil Car { get => Term.NIL; }

        public override uint Ungroundedness => Term.NIL.Ungroundedness;
        public override bool TermEquals(State s, Term other) => other.TermEquals(s, Term.NIL);

        public override string ToString() => Term.NIL.ToString();
        public override string ToVerboseString() => Term.NIL.ToVerboseString();
    }

    public static class Cons
    {
        public static Cons<T1, T2> Truct<T1, T2>(T1 car, T2 cdr)
            where T1 : Term
            where T2 : Term
        {
            return new Cons<T1, T2>(car, cdr);
        }

        public static ConsList<T> Truct<T>(T car, ConsList<T> cdr)
            where T : Term
        {
            return new ConsList<T>(car, cdr);
        } 

        public static ConsString Truct(Value<char> car, ConsString cdr)
        {
            return new ConsString(car, cdr);
        }

        public static ConsList<T> Truct<T>(T car, Nil _)
            where T : Term
            => new ConsList<T>(car);

        public static ConsString Truct(Value<char> car, Nil _)
            => new ConsString(car);

        public static ConsString Truct(string s)
        {
            if (s.Length == 1)
            {
                return new ConsString(s[0]);
            }
            else
            {
                return new ConsString(s[0], Truct(s[1..]));
            }
        }
    }

    public class LCons : Term
    {
        public Term Car { get; init; }
        public Term Cdr { get; init; }

        private bool IsList;
        private bool IsString;

        private LCons()
        {
            this.Car = Term.NIL;
            this.Cdr = Term.NIL;
            this.IsList = false;
        }

        private LCons(Term car) : this()
        {
            this.Car = car;
            this.IsList = true;
            this.IsString = car is Value<char>;
        }

        private LCons(Term car, Term cdr) : this()
        {
            this.Car = car;
            this.Cdr = cdr;
            this.IsList = cdr is Nil;
            this.IsString = this.IsList && car is Value<char>;
        }

        //public Cons(Term car, Term cdar, Term cddr, params Term[] more)
        //{
        //    this.Car = car;
        //    this.Cdr = Cons.Truct(cdar, Cons.Truct(cddr, Cons.TructList(more)));
        //}

        public static LCons Truct(Term car, Term cdr) => new LCons(car, cdr);

        public static Term TructList(params Term[] sequence) => sequence.Any() ? GuaranteedCons(sequence) : Term.NIL;

        private static LCons GuaranteedCons(params Term[] sequence)
        {
            if (sequence.Length == 1)
            {
                return LCons.Truct(sequence[0], Term.NIL);
            }
            else
            {
                return LCons.Truct(sequence[0], GuaranteedCons(sequence[1..]));
            }
        }

        public static Term Truct(string s) => GuaranteedCons(s.Select(ValueFactory.Box).ToArray());

        public override uint Ungroundedness => this.Car.Ungroundedness + this.Cdr.Ungroundedness;

        public override bool TermEquals(State s, Term other) => other.TermEquals(s, this);
        public override bool TermEquals(State s, LCons other)
        {
            return this.Car.TermEquals(s, other.Car) && this.Cdr.TermEquals(s, other.Cdr);
        }

        public override string ToString()
        {
            if (this.IsStringy())
            {
                return $"\"{this.ToSubString()}\"";
            }
            else if (this.IsListy())
            {
                return $"[{this.ToSubList()}]";
            }
            else
            {
                return "(" + this.Car.ToString() + " . " + this.Cdr + ")";
            }

        }

        private string ToSubString()
        {
            return this.Car.ToString() + (this.Cdr as LCons)?.ToSubString() ?? Term.NIL.ToString();
        }

        private bool IsStringy()
        {
            return this.Car is Value<char> && (this.Cdr is Nil || ((this.Cdr as LCons)?.IsStringy() ?? false));
        }

        private string ToSubList()
        {
            return $"{this.Car}, {((this.Cdr as LCons)?.ToSubList() ?? Term.NIL.ToString())}";
        }

        private bool IsListy()
        {
            return this.Cdr is Nil || ((this.Cdr as LCons)?.IsListy() ?? false);
        }

        //public override IEnumerable<string> ToTree(string prefix, bool first, bool last)
        //{
        //    string parentPrefix = first ? "" : prefix + (last ? IO.LEAVES : IO.HEADER);
        //    string childPrefix = first ? "" : prefix + (last ? IO.SPACER : IO.JUMPER);

        //    foreach (string line in this.Car.ToTree(parentPrefix, false, false))
        //    {
        //        yield return line;
        //    }

        //    foreach (string line in this.Cdr.ToTree(childPrefix, false, true))
        //    {
        //        yield return line;
        //    }

        //    //yield return prefix + IO.HEADER + IO.ALONER + this.Car.ToTree(prefix, false, false);
        //    //yield return prefix + IO.LEAVES + this.Cdr.ToTree(prefix, false, true);
        //}
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
