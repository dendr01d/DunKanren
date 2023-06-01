using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.Goals
{
    public abstract class Goal : IPrintable, IComparable<Goal>
    {
        public abstract string Description { get; }
        public abstract string Expression { get; }
        public abstract IEnumerable<IPrintable> SubExpressions { get; }

        public Stream Pursue() => this.GetApp()(State.InitialState());
        public virtual Stream PursueIn(State s) => this.GetApp()(s);
        public virtual Stream NegateIn(State s) => this.GetNeg()(s);

        internal abstract Func<State, Stream> GetApp();
        internal abstract Func<State, Stream> GetNeg();

        public override string ToString() => $"({this.Expression})";
        public string ToVerboseString() => this.Description;

        public IEnumerable<string> ToTree() => ToTree("", true, false);
        public IEnumerable<string> ToTree(string prefix, bool first, bool last)
        {
            string parentPrefix = first ? "" : prefix + (last ? IO.LEAVES : IO.BRANCH);
            string childPrefix = first ? "" : prefix + (last ? IO.SPACER : IO.JUMPER);

            yield return parentPrefix + (this.SubExpressions.Any() ? IO.HEADER : IO.ALONER) + this.Description;

            if (this.SubExpressions.Any())
            {
                foreach (var comp in this.SubExpressions.SkipLast(1))
                {
                    foreach (string line in comp.ToTree(childPrefix, false, false))
                    {
                        yield return line;
                    }
                }

                foreach (string line in this.SubExpressions.Last().ToTree(childPrefix, false, true))
                {
                    yield return line;
                }
            }
        }

        public int CompareTo(Goal? other)
        {
            if (other is null)
            {
                return 1;
            }
            else
            {
                return Compare(this, other);
            }
        }

        public abstract int Ungroundedness { get; }

        /// <summary>
        /// Goal sorting order for evaluating conjunctions.
        /// Basically try to eliminate branches as fast as possible.
        /// Instantiating new variables and recursing is dead last.
        /// </summary>
        private static List<Type> Priority = new()
        {
            typeof(Bottom),
            typeof(Top),
            typeof(Not),
            typeof(Disequality),
            typeof(Equality),
            typeof(Disj),
            typeof(Impl),
            typeof(Conj),
            typeof(Fresh),
            typeof(CallFresh)
        };

        public static int Compare(Goal g1, Goal g2)
        {
            //if (Priority.IndexOf(g1.GetType()) is int p1
            //    && Priority.IndexOf(g2.GetType()) is int p2
            //    && p1 != p2)
            //{
            //    return p1.CompareTo(p2);
            //}

            return g1.Ungroundedness.CompareTo(g2.Ungroundedness);
        }

        public static Goal NOT(Goal g)
        {
            return new Not(g);
        }

        public static Goal AND(Goal g1, Goal g2)
        {
            return new Conj(g1, g2);
        }

        public static Goal OR(Goal g1, Goal g2)
        {
            return new Disj(g1, g2);
        }

        public static Goal IMPL(Goal g1, Goal g2)
        {
            return new Disj(NOT(g1), g2);
        }

        public static Goal BIMP(Goal g1, Goal g2)
        {
            return new Conj(IMPL(g1, g2), IMPL(g2, g1));
        }

        public static Goal XOR(Goal g1, Goal g2)
        {
            return AND(OR(g1, g2), NOT(AND(g1, g2)));
        }
    }

    /// <summary>
    /// No-Op Goal; always returns the same state it's passed. Similar to Logical Truth
    /// </summary>
    public class Top : Goal
    {
        public override string Expression => "TRUE";
        public override string Description => this.Expression;
        public override IEnumerable<IPrintable> SubExpressions => Array.Empty<IPrintable>();

        internal override Func<State, Stream> GetApp() => (State s) => Stream.Singleton(s);
        internal override Func<State, Stream> GetNeg() => (State s) => Stream.Empty();

        public override int Ungroundedness => 0;
    }

    /// <summary>
    /// No-Op Goal; always rejects the states it's passed. Similar to Logical Falsity
    /// </summary>
    public class Bottom : Goal
    {
        public override string Expression => "FALSE";
        public override string Description => this.Expression;
        public override IEnumerable<IPrintable> SubExpressions => Array.Empty<IPrintable>();

        internal override Func<State, Stream> GetApp() => (State s) => Stream.Empty();
        internal override Func<State, Stream> GetNeg() => (State s) => Stream.Singleton(s);

        public override int Ungroundedness => 0;
    }

    public class Not : Goal
    {
        public override string Expression => $"!({this.Original})";
        public override string Description => "The following statement is NOT true";
        public override IEnumerable<IPrintable> SubExpressions => new IPrintable[] { this.Original };

        private Goal Original;

        public Not(Goal g)
        {
            this.Original = g;
        }

        internal override Func<State, Stream> GetApp() => this.Original.GetNeg();
        internal override Func<State, Stream> GetNeg() => this.Original.GetApp();

        public override int Ungroundedness => this.Original.Ungroundedness;
    }

    /// <summary>
    /// Represents a goal constructed using 2 or more arguments of type T,
    /// and the final goal is assembled via some combination of these arguments
    /// </summary>
    public abstract class Goal<T> : Goal, IEnumerable<T>
        where T : class, IPrintable
    {
        protected List<Lazy<T>> Subs;
        public override IEnumerable<IPrintable> SubExpressions => this.Subs.Select(x => x.Value);

        protected Goal()
        {
            this.Subs = new();
        }

        protected Goal(params T[] subs)
        {
            this.Subs = subs.Select(x => new Lazy<T>(() => x)).ToList();
        }

        public virtual IEnumerator<T> GetEnumerator() => this.Subs.Select(x => x.Value).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public interface ICollectionInitialized<T> : IEnumerable<T>
    {
        public void Add(T goal);
    }
}
