using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.Terms
{
    /*
    public abstract class Cont<T> : Term, IEnumerable<Term>
        where T : Term
    {
        protected abstract IEnumerable<T> GetCollection();

        public class AnonymousCont<Ti> : Cont<Ti>
            where Ti : Term
        {
            private Ti[] Coll;
            public AnonymousCont(params Ti[] ts)
            {
                this.Coll = ts;
            }
            protected override IEnumerable<Ti> GetCollection() => this.Coll;
            public override string ToString() => "{ " + String.Join(" ", this.GetCollection()) + " }";
        }

        public override bool SameAs(State s, Term other) => other.SameAs(s, this);
        public override bool SameAs<O>(State s, Cont<O> other)
        {
            return this.Zip(other, (x, y) => x.SameAs(s, y)).All(x => x);
        }

        public override State? UnifyWith(State s, Term other) => other.UnifyWith(s, this);
        public override State? UnifyWith<O>(State s, Cont<O> other)
        {
            return this.Zip(other, (x, y) => new { x, y }).Aggregate(s, (sub, pair) => sub?.Unify(pair.x, pair.y) ?? null!);
        }

        public IEnumerator<Term> GetEnumerator() => this.GetCollection().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetCollection().GetEnumerator();
    }

    public class ContArray<T> : Cont<T>
        where T : Term
    {
        private T[] CArr;
        protected override IEnumerable<T> GetCollection() => this.CArr;

        public ContArray(params T[] ts)
        {
            this.CArr = ts;
        }

        public override string ToString()
        {
            return "[" + String.Join(", ", this.GetCollection()) + "]";
        }
    }

    public class ConsList<T, TSelf> : Cont<T> where TSelf : ConsList<T, TSelf>
        where T : Term
    {
        public T Car { get; private set; }
        private TSelf? Tail { get; init; }

        public Term Cdr { get => this.Tail is null ? Term.nil : this.Tail; }

        protected ConsList(T head)
        {
            this.Car = head;
            this.Tail = null;
        }
        protected ConsList(T head, TSelf tail) : this(head)
        {
            this.Tail = tail;
        }

        protected override IEnumerable<T> GetCollection()
        {
            yield return this.Car;

            if (this.Tail is null) yield break;

            foreach(T more in this.Tail)
            {
                yield return more;
            }
        }

        public override string ToString()
        {
            return "(" + this.Car.ToString() + " . " + this.Cdr.ToString() + ")";
        }

        protected string ToSubString(Func<T, TSelf, string> caseMore, Func<T, Nil, string> caseEnd)
        {
            if (this.Tail is null)
            {
                return caseEnd(this.Car, Term.nil);
            }
            else
            {
                return caseMore(this.Car, this.Tail);
            }
        }
    }

    public class ConsString : ConsList<Value<char>, ConsString>
    {
        private ConsString(char first, ConsString rest) : base(ValueFactory.Box(first), rest) { }
        private ConsString(char first) : base(ValueFactory.Box(first)) { }

        public static Term New(string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                return Term.nil;
            }
            else
            {
                return RecurseNew(s.ToArray());
            }
        }

        private static ConsString RecurseNew(params char[] cs)
        {
            if (cs.Length == 1)
            {
                return new ConsString(cs[0]);
            }
            else
            {
                return new ConsString(cs[0], RecurseNew(cs.Skip(1).ToArray()));
            }
        }

        public override string ToString()
        {
            return "\"" + this.ToSubString(this.CaseMore, this.CaseEnd) + "\"";
        }

        private string CaseMore(Value<char> head, ConsString tail) => head.ToString() + tail.ToSubString(CaseMore, CaseEnd);
        private string CaseEnd(Value<char> head, Nil tail) => head.ToString() + Term.nil.ToString();
    }
    */
}
