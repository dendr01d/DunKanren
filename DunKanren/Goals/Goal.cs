using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.Goals
{
    public abstract class Goal : IPrintable, IGrounded
    {
        public abstract string Description { get; }
        public abstract string Expression { get; }
        public abstract IEnumerable<IPrintable> SubExpressions { get; }

        public Stream Pursue() => this.GetApp().Value(State.InitialState());
        public virtual Stream PursueIn(State s) => this.GetApp().Value(s);
        public virtual Stream NegateIn(State s) => this.GetNeg().Value(s);

        public Goal Negate() => !this;

        internal abstract Lazy<Func<State, Stream>> GetApp();
        internal abstract Lazy<Func<State, Stream>> GetNeg();

        public override string ToString() => $"({this.Expression})";
        public string ToVerboseString() => this.Description;

        public IEnumerable<string> ToTree() => ToTree("", true, false);
        public virtual IEnumerable<string> ToTree(string prefix, bool first, bool last)
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

        public virtual uint Ungroundedness { get => (Priority.IndexOf(this.GetType()) is int p1 ? (uint)p1 : 0); }
        public int CompareTo(IGrounded? other) => this.Ungroundedness.CompareTo(other?.Ungroundedness ?? 0);

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

        public static Goal operator &(Goal lhs, Goal rhs) => AND(lhs, rhs);
        public static Goal operator |(Goal lhs, Goal rhs) => OR(lhs, rhs);
        public static Goal operator !(Goal g) => NOT(g);
    }

    /// <summary>
    /// No-Op Goal; always returns the same state it's passed. Similar to Logical Truth
    /// </summary>
    public class Top : Goal
    {
        public override string Expression => "TRUE";
        public override string Description => this.Expression;
        public override IEnumerable<IPrintable> SubExpressions => Array.Empty<IPrintable>();

        internal override Lazy<Func<State, Stream>> GetApp() => new(() => (State s) => Stream.Singleton(s));
        internal override Lazy<Func<State, Stream>> GetNeg() => new(() => (State s) => Stream.Empty());
    }

    /// <summary>
    /// No-Op Goal; always rejects the states it's passed. Similar to Logical Falsity
    /// </summary>
    public class Bottom : Goal
    {
        public override string Expression => "FALSE";
        public override string Description => this.Expression;
        public override IEnumerable<IPrintable> SubExpressions => Array.Empty<IPrintable>();

        internal override Lazy<Func<State, Stream>> GetApp() => new(() => (State s) => Stream.Empty());
        internal override Lazy<Func<State, Stream>> GetNeg() => new(() => (State s) => Stream.Singleton(s));
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

        internal override Lazy<Func<State, Stream>> GetApp() => this.Original.GetNeg();
        internal override Lazy<Func<State, Stream>> GetNeg() => this.Original.GetApp();

        public override uint Ungroundedness => this.Original.Ungroundedness;
    }

    /// <summary>
    /// Represents a goal constructed using 2 or more arguments of type T,
    /// and the final goal is assembled via some combination of these arguments
    /// </summary>
    public abstract class Goal<T> : Goal, IEnumerable<T>
        where T : class, IPrintable
    {
        protected List<T> Subs;
        public override IEnumerable<IPrintable> SubExpressions => this.Subs.Select(x => x);

        public override Stream PursueIn(State s) => this.Subs.Any() ? this.GetApp().Value(s) : Stream.Empty();
        public override Stream NegateIn(State s) => this.Subs.Any() ? this.GetNeg().Value(s) : Stream.Empty();

        protected Goal()
        {
            this.Subs = new();
        }

        protected Goal(params T[] subs)
        {
            this.Subs = subs.ToList();
        }

        public virtual IEnumerator<T> GetEnumerator() => this.Subs.Select(x => x).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public interface ICollectionInitialized<T> : IEnumerable<T>
    {
        public void Add(T goal);
    }
}
