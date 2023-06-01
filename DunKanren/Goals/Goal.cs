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
        public abstract IEnumerable<IPrintable> ChildGoals { get; }

        public virtual Stream PursueIn(State s)
        {
            return this.Application(s);
        }
        protected abstract Stream Application(State s);
        public Stream Pursue() => this.Application(State.InitialState());
        public abstract Goal Negate();

        public override string ToString() => $"({this.Expression})";
        public string ToVerboseString() => this.Description;

        //public virtual Goal Resolve() => this;
        //public virtual bool? LogicalDeterminate => null;

        public IEnumerable<string> ToTree() => ToTree("", true, false);
        public IEnumerable<string> ToTree(string prefix, bool first, bool last)
        {
            string parentPrefix = first ? "" : prefix + (last ? IO.LEAVES : IO.BRANCH);
            string childPrefix = first ? "" : prefix + (last ? IO.SPACER : IO.JUMPER);

            yield return parentPrefix + (this.ChildGoals.Any() ? IO.HEADER : IO.ALONER) + this.Description;

            if (this.ChildGoals.Any())
            {
                foreach (var comp in this.ChildGoals.SkipLast(1))
                {
                    foreach (string line in comp.ToTree(childPrefix, false, false))
                    {
                        yield return line;
                    }
                }

                foreach (string line in this.ChildGoals.Last().ToTree(childPrefix, false, true))
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
            if (Priority.IndexOf(g1.GetType()) is int p1
                && Priority.IndexOf(g2.GetType()) is int p2
                && p1 != p2)
            {
                return p1.CompareTo(p2);
            }

            return g1.Ungroundedness.CompareTo(g2.Ungroundedness);
        }

        public static Goal NOT(Goal g)
        {
            return g.Negate();
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
            return new Disj(g1.Negate(), g2);
        }

        public static Goal BIMP(Goal g1, Goal g2)
        {
            return new Conj(IMPL(g1, g2), IMPL(g2, g1));
        }

        public static Goal XOR(Goal g1, Goal g2)
        {
            return AND(OR(g1, g2), AND(g1, g2).Negate());
        }
    }

    /// <summary>
    /// No-Op Goal; always returns the same state it's passed. Similar to Logical Truth
    /// </summary>
    public class Top : Goal
    {
        public override string Expression => "TRUE";
        public override string Description => this.Expression;
        public override IEnumerable<IPrintable> ChildGoals => Array.Empty<IPrintable>();
        protected override Stream Application(State s) => Stream.Singleton(s);
        public override Goal Negate() => new Bottom();
        //public override bool? LogicalDeterminate => true;

        public override int Ungroundedness => 0;
    }

    /// <summary>
    /// No-Op Goal; always rejects the states it's passed. Similar to Logical Falsity
    /// </summary>
    public class Bottom : Goal
    {
        public override string Expression => "FALSE";
        public override string Description => this.Expression;
        public override IEnumerable<IPrintable> ChildGoals => Array.Empty<IPrintable>();
        protected override Stream Application(State s) => Stream.Empty();
        public override Goal Negate() => new Top();
        //public override bool? LogicalDeterminate => false;

        public override int Ungroundedness => 0;
    }

    public class Not : Goal
    {
        public override string Expression => $"!({this.Original})";
        public override string Description => "The following statement is NOT true";
        public override IEnumerable<IPrintable> ChildGoals => new IPrintable[] { this.Original };
        private readonly Goal Original;
        public Not(Goal g)
        {
            this.Original = g;//.Resolve();
        }
        protected override Stream Application(State s) => this.Original.Negate().PursueIn(s);
        public override Goal Negate() => this.Original;

        public override int Ungroundedness => this.Original.Ungroundedness;
    }

    /// <summary>
    /// Represents a goal constructed using 2 or more arguments of type T,
    /// and the final goal is assembled via some combination of these arguments
    /// </summary>
    public abstract class Goal<T> : Goal, IEnumerable<T>
        where T : IPrintable
    {
        public abstract IEnumerator<T> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    public interface ICollectionInitialized<T> : IEnumerable<T>
    {
        public void Add(T goal);
    }
}
