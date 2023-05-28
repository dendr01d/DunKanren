using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.Goals
{
    public abstract class Goal : IPrintable
    {
        public abstract string Description { get; }
        public abstract string Expression { get; }
        public abstract IEnumerable<IPrintable> Components { get; }

        public Stream PursueIn(State s)
        {
            IO.Debug_Print(this.ToString());
            return this.Pursuit(s);
        }
        protected abstract Stream Pursuit(State s);
        public Stream Pursue() => this.Pursuit(State.InitialState());
        public abstract Goal Negate();

        public override string ToString() => $"({this.Expression})";
        public string ToVerboseString() => this.Description;

        public virtual Goal Resolve() => this;
        public virtual bool? LogicalDeterminate => null;

        public IEnumerable<string> ToTree() => ToTree("", true, false);
        public IEnumerable<string> ToTree(string prefix, bool first, bool last)
        {
            string parentPrefix = first ? "" : prefix + (last ? IO.LEAVES : IO.BRANCH);
            string childPrefix = first ? "" : prefix + (last ? IO.SPACER : IO.JUMPER);

            yield return parentPrefix + (this.Components.Any() ? IO.HEADER : IO.ALONER) + this.Description;

            if (this.Components.Any())
            {
                foreach (var comp in this.Components.SkipLast(1))
                {
                    foreach (string line in comp.ToTree(childPrefix, false, false))
                    {
                        yield return line;
                    }
                }

                foreach (string line in this.Components.Last().ToTree(childPrefix, false, true))
                {
                    yield return line;
                }
            }

            //yield return parentPrefix + IO.HEADER + g.Description;
            //if (this.Components.Any())
            //{
            //    using (var iter = this.Components.GetEnumerator())
            //    {
            //        bool move1 = iter.MoveNext();

            //        //if this goal won't print any children then 
            //        yield return parentPrefix + (move1 ? IO.HEADER : IO.ALONER) + Description;

            //        IPrintable? sub = iter.Current;

            //        while (sub != null)
            //        {
            //            if (iter.MoveNext())
            //            {
            //                foreach (string line in sub.ToTree(childPrefix, false, false))
            //                {
            //                    yield return line;
            //                }

            //                sub = iter.Current;
            //            }
            //            else
            //            {
            //                foreach (string line in sub.ToTree(childPrefix, false, true))
            //                {
            //                    yield return line;
            //                }
            //                //yield return prefix + IO.JUMPER;

            //                sub = null;
            //            }

            //        }
            //    }
            //}
        }
    }

    /// <summary>
    /// No-Op Goal; always returns the same state it's passed. Similar to Logical Truth
    /// </summary>
    public class Top : Goal
    {
        public override string Expression => "TRUE";
        public override string Description => this.Expression;
        public override IEnumerable<IPrintable> Components => Array.Empty<IPrintable>();
        protected override Stream Pursuit(State s) => Stream.Singleton(s);
        public override Goal Negate() => new Bottom();
        public override bool? LogicalDeterminate => true;
    }

    /// <summary>
    /// No-Op Goal; always rejects the states it's passed. Similar to Logical Falsity
    /// </summary>
    public class Bottom : Goal
    {
        public override string Expression => "FALSE";
        public override string Description => this.Expression;
        public override IEnumerable<IPrintable> Components => Array.Empty<IPrintable>();
        protected override Stream Pursuit(State s) => Stream.Empty();
        public override Goal Negate() => new Top();
        public override bool? LogicalDeterminate => false;
    }

    public class Not : Goal
    {
        public override string Expression => $"!({this.Original})";
        public override string Description => "The following statement is NOT true";
        public override IEnumerable<IPrintable> Components => new IPrintable[] { this.Original };
        private readonly Goal Original;
        public Not(Goal g)
        {
            this.Original = g.Resolve();
        }
        protected override Stream Pursuit(State s) => this.Original.Negate().PursueIn(s);
        public override Goal Negate() => this.Original;

        public override Goal Resolve()
        {
            if (this.Original is Not)
            {
                return Original;
            }
            return base.Resolve();
        }

        public override bool? LogicalDeterminate => !this.Original.LogicalDeterminate;
    }

    public abstract class Goal<T1, T2> : Goal
    {
        public T1 Argument1 { get; protected set; }
        public T2 Argument2 { get; protected set; }

        public Goal(T1 arg1, T2 arg2)
        {
            this.Argument1 = arg1;
            this.Argument2 = arg2;
        }

        protected override Stream Pursuit(State s) => this.ApplyToState(s, this.Argument1, this.Argument2);

        protected abstract Stream ApplyToState(State s, T1 arg1, T2 arg2);
    }

    public static class GoalFactory
    {


    }
}
