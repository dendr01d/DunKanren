//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DunKanren
//{
//    public class Seq : Term
//    {
//        private IEnumerable<Term> InnerSeq;

//        protected Seq()
//        {
//            this.InnerSeq = new List<Term>() { };
//        }

//        public Seq(params Term[] items)
//        {
//            this.InnerSeq = items;
//        }

//        public static Seq Uence(Term t) => new(t);
//        public static Seq Uence(Seq s) => s;
//        public static Seq Uence(Nil _) => new();

//        public static Term PeekFront(Seq seq)
//        {
//            return seq.InnerSeq.Any()
//                ? seq.InnerSeq.First()
//                : Term.nil;
//        }

//        public static Term PeekBack(Seq seq)
//        {
//            return seq.InnerSeq.Any()
//                ? seq.InnerSeq.Last()
//                : Term.nil;
//        }

//        public static Seq PushFront(Term front, Seq seq)
//        {
//            return new Seq(front).Add(seq.InnerSeq.ToArray());
//        }

//        public static Seq PushBack(Seq seq, Term back)
//        {
//            return new Seq(seq.InnerSeq.ToArray()).Add(back);
//        }

//        public static Seq PopFront(Seq seq)
//        {
//            return seq.InnerSeq.Any()
//                ? new Seq(seq.InnerSeq.Skip(1).ToArray())
//                : new Seq();
//        }

//        public static Seq PopBack(Seq seq)
//        {
//            return seq.InnerSeq.Any()
//                ? new Seq(seq.InnerSeq.SkipLast(1).ToArray())
//                : new Seq();
//        }

//        private Seq Add(params Term[] items)
//        {
//            this.InnerSeq = this.InnerSeq.Concat(items);
//            return this;
//        }

//        public Value<bool> Empty()
//        {
//            return this.InnerSeq.Any() ? Term.False : Term.True;
//        }

//        public override Term Dereference(State s)
//        {
//            return new Seq(this.InnerSeq.Select(x => x.Dereference(s)).ToArray());
//        }

//        public override bool SameAs(State s, Term other) => other.SameAs(s, this);
//        public override bool SameAs(State s, Seq other)
//        {
//            return base.SameAs(s, other);
//        }

//        public override bool TryUnifyWith(State s, Term other, out State result) => other.TryUnifyWith(s, this, out result);
//        public override bool TryUnifyWith(State s, Seq other, out State result)
//        {
//            if (this.InnerSeq.SequenceEqual(other.InnerSeq))
//            {
//                return s.Affirm(other, this, out result);
//            }
//            else if (this.InnerSeq.Count() != other.InnerSeq.Count())
//            {
//                return s.Reject(other, this, out result);
//            }

//            State output = s;
//            foreach (var pair in this.InnerSeq.Zip(other.InnerSeq, (x, y) => new Tuple<Term, Term>(x, y)))
//            {
//                if (!pair.Item1.TryUnifyWith(output, pair.Item2, out output))
//                {
//                    return s.Reject(other, this, out result);
//                }
//            }

//            result = output;
//            return true;
//        }

//        public override string ToString()
//        {
//            return $"[{String.Join(", ", this.InnerSeq)}]";
//        }
//    }
//}
