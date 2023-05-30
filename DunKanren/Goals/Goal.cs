using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.Goals
{
    /// <summary>
    /// AKA "Hierarchical" Goal
    /// </summary>
    public abstract partial class Goal : IPrintable
    {
        private int PersonalID;
        private static int GoalCounter = 0;

        public virtual string Expression { get; protected set; }
        public virtual string Description { get; protected set; }

        protected Func<State, Stream> Application;

        protected List<IPrintable> Children;

        protected Goal()
        {
            this.PersonalID = ++GoalCounter;
            this.Expression = "Unknown Goal";
            this.Description = "Application of Unknown Goal";
            this.Application = (State s) => throw new NotImplementedException("Goal was assigned no application function");
            this.Children = new List<IPrintable>();
        }

        protected Goal(Func<State, Stream> fun) : this()
        {
            this.Application = fun;
        }

        public Stream PursueIn(State s)
        {
            return this.Application(s);
        }

        public Stream Pursue()
        {
            return this.PursueIn(State.InitialState());
        }

        protected static string[] ExtractVarNames(System.Reflection.MethodInfo funInfo)
        {
            char defaultSymbol = 'A';
            return funInfo.GetParameters().Select(x => x.Name ?? "v" + (defaultSymbol++).ToString()).ToArray();
        }

        public string GetName() => "Goal #" + this.PersonalID + ": " + this.ToString();

        public override string ToString() => this.Expression;
        public virtual string ToVerboseString() => this.Description;

        public IEnumerable<string> ToTree() => this.ToTree("", true, false);
        public IEnumerable<string> ToTree(string prefix, bool first, bool last)
        {
            string parentPrefix = first ? "" : prefix + (last ? IO.LEAVES : IO.BRANCH);
            string childPrefix = first ? "" : prefix + (last ? IO.SPACER : IO.JUMPER);


            //yield return parentPrefix + IO.HEADER + g.Description;

            using (var iter = this.Children.GetEnumerator())
            {
                bool move1 = iter.MoveNext();

                //if this goal won't print any children then 
                yield return parentPrefix + (move1 ? IO.HEADER : IO.ALONER) + this.Description;

                IPrintable sub = iter.Current;

                while (sub != null)
                {
                    if (iter.MoveNext())
                    {
                        foreach (string line in sub.ToTree(childPrefix, false, false))
                        {
                            yield return line;
                        }
                    }
                    else
                    {
                        foreach (string line in sub.ToTree(childPrefix, false, true))
                        {
                            yield return line;
                        }
                        //yield return prefix + IO.JUMPER;
                    }

                    sub = iter.Current;
                }
            }
        }
    }

    public class FutureGoal : Goal
    {
        public FutureGoal() : base()
        {
            this.Expression = "{Unevaluated Goal}";
            this.Description = "{Application of Unevaluated Goal}";
        }
    }
}
