using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace DunKanren
{
    /// <summary>
    /// A statement defining a relationship between terms
    /// </summary>
    public class Old_Goal : IPrintable
    {
        private static int IDGen = 0;

        private int PersonalID;

        /// <summary>
        /// Simple/Mathematical expression of what the Goal represents
        /// </summary>
        public virtual string Expression { get; protected set; }
        /// <summary>
        /// Longer-form description of what statement the goal represents
        /// </summary>
        public virtual string Description { get; protected set; }


        protected Func<State, Stream> Application;

        protected List<Old_Goal> SubGoals;

        private static Old_Goal BaseGoal = null!;
        private static int BaseLineCount = 0;

        protected Old_Goal()
        {
            PersonalID = IDGen++;
            this.Expression = "Unknown Goal";
            this.Description = "Application of Unknown Goal";
            this.Application = (State s) => Stream.Singleton(s);
            this.SubGoals = new List<Old_Goal>();
        }

        protected static Old_Goal FutureGoal()
        {
            return new Old_Goal()
            {
                Expression = "{...}",
                Description = "{Unevaluated Lambda Expression}"
            };
        }

        protected Old_Goal(Func<State, Stream> fun) : this()
        {
            this.Application = fun;
        }

        protected Old_Goal(Func<IEnumerable<Old_Goal>, Old_Goal> agg) : this()
        {
            this.Application = (State s) => agg(this.SubGoals).Application(s);
        }

        protected Old_Goal(Old_Goal g) : this(g.Application)
        {
            this.Expression = g.Expression;
            this.Description = g.Description;
            this.SubGoals = g.SubGoals.Select(x => new Old_Goal(x)).ToList();
        }

        //see if the base goal has grown since the last time it was printed, then maybe print it again
        private static void ConditionalPrint(State s)
        {
            IEnumerable<string> lines = BaseGoal.ToTree();

            if (lines.Count() > BaseLineCount)
            {
                BaseLineCount = lines.Count();
                Console.WriteLine(BaseGoal.GetName() + " @ Depth " + s.RecursionLevel);
                Console.WriteLine(IO.Graft(lines));
            }
        }

        public Stream PursueIn(State s)
        {
            
            if (this.SubGoals.Any())
            {
                IO.Debug_Print(IO.Graft(this.ToTree()));
            }
            else
            {
                IO.Debug_Print(this.ToString());
            }
            

            return this.Application(s);
        }

        public Stream Pursue()
        {
            BaseGoal = this;
            return this.PursueIn(State.InitialState());
        }

        protected static void MarkEvaluated(Old_Goal g)
        {
            g.Description = "*" + g.Description;
        }

        protected static string[] ExtractVarNames(System.Reflection.MethodInfo funInfo)
        {
            char defaultSymbol = 'A';
            return funInfo.GetParameters().Select(x => x.Name ?? "v" + (defaultSymbol++).ToString()).ToArray();
        }

        #region Primitive Goals

        //protected delegate Stream PrimitiveGoal(State s);

        //There's really no good primitive way to do this, since it's self-referential in terms of the goal that contains the primitive
        protected static Old_Goal Primitive_Fresh(Func<Variable, Old_Goal> fun, Old_Goal shell)
        {
            return new Old_Goal((State s) =>
            {
                Variable v = new Variable(s, ExtractVarNames(fun.Method)[0]);
                return fun(v).PursueIn(s.DeclareVars(v).Next());
            });
        }

        protected static Old_Goal Primitive_Equality(Term t1, Term t2) => new Old_Goal((State s) => Stream.Singleton(s.Unify(t1, t2)));
        protected static Old_Goal Primitive_DisEquality(Term t1, Term t2) => new Old_Goal((State s) => Stream.Singleton(s.DisUnify(t1, t2)));

        protected static Old_Goal Primitive_Conjunction(Old_Goal g1, Old_Goal g2) => new Old_Goal((State s) => Stream.New(g1.PursueIn(s).SelectMany(x => g2.PursueIn(x))));

        protected static Old_Goal Primitive_Disjunction(Old_Goal g1, Old_Goal g2) => new Old_Goal((State s) => Stream.Interleave(g1.PursueIn(s), g2.PursueIn(s)));

        #endregion


        #region Static Callables

        public static Old_Goal CallFresh(Func<Variable, Old_Goal> lambda) => new Old_Goal()
        {
            Expression = "Lambda (" + (lambda.Method.GetParameters()[0].Name ?? "var") + ")",
            Description = "There exists a variable (" + lambda.Method.GetParameters()[0].Name ?? "var" + ") such that:",
            SubGoals = new List<Old_Goal>() { FutureGoal() }
        };

        public static Old_Goal Equality(Term t1, Term t2) => new Old_Goal(Primitive_Equality(t1, t2))
        {
            Expression = "(" + t1.ToString() + " ≡ " + t2.ToString() + ")",
            Description = "'" + t1.ToString() + "' equals '" + t2.ToString() + "'"
        };

        public static Old_Goal Disequality(Term t1, Term t2) => new Old_Goal(Primitive_DisEquality(t1, t2))
        {
            Expression = "(" + t1.ToString() + " ╪ " + t2.ToString() + ")",
            Description = "'" + t1.ToString() + "' never equals '" + t2.ToString() + "'"
        };

        public static Old_Goal Conjunction(Old_Goal g1, Old_Goal g2) => new Old_Goal(Primitive_Conjunction(g1, g2))
        {
            Expression = "(" + g1.ToString() + " & " + g2.ToString() + ")",
            Description = "Both of the following statements are true:",
            SubGoals = new List<Old_Goal>() { g1, g2 }
        };
        public static Old_Goal Disjunction(Old_Goal g1, Old_Goal g2) => new Old_Goal(Primitive_Disjunction(g1, g2))
        {
            Expression = "(" + g1.ToString() + " | " + g2.ToString() + ")",
            Description = "One or both of the following statements are true:",
            SubGoals = new List<Old_Goal>() { g1, g2 }
        };

        #endregion

        public string GetName() => "Goal #" + this.PersonalID + ": " + this.ToString();

        public override string ToString() => this.Expression;
        public virtual string ToVerboseString() => this.Description;

        public IEnumerable<string> ToTree() => this.ToTree("", true, false);
        public IEnumerable<string> ToTree(string prefix, bool first, bool last)
        {
            string parentPrefix = first ? "" : prefix + (last ? IO.LEAVES : IO.BRANCH);
            string childPrefix = first ? "" : prefix + (last ? IO.SPACER : IO.JUMPER);


            //yield return parentPrefix + IO.HEADER + g.Description;

            using (var iter = this.SubGoals.GetEnumerator())
            {
                bool move1 = iter.MoveNext();

                //if this goal won't print any children then 
                yield return parentPrefix + (move1 ? IO.HEADER : IO.ALONER) + this.Description;

                Old_Goal sub = iter.Current;

                while (sub != null)
                {
                    if (iter.MoveNext())
                    {
                        foreach(string line in sub.ToTree(childPrefix, false, false))
                        {
                            yield return line;
                        }
                    }
                    else
                    {
                        foreach(string line in sub.ToTree(childPrefix, false, true))
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


    public abstract class Clause : Old_Goal, IEnumerable<Old_Goal>
    {
        protected Clause(Func<IEnumerable<Old_Goal>, Old_Goal> aggregateParams) : base(aggregateParams) { }

        public virtual void Add(Old_Goal g)
        {
            this.SubGoals.Add(g);
        }

        public void Add(string ex, string desc)
        {
            this.Expression = ex;
            this.Description = desc;
        }

        public IEnumerator<Old_Goal> GetEnumerator() => this.SubGoals.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.SubGoals.GetEnumerator();
    }


    
    public class CallFresh : Old_Goal
    {
        private CallFresh(MethodInfo funInfo, Func<Variable[], Old_Goal> fun) : base()
        {
            string[] varNames = ExtractVarNames(funInfo);
            string varString = String.Join(", ", varNames);

            this.Expression = "Lambda (" + varString + ")";
            this.Description = FormatDescription(varNames);
            this.SubGoals = new List<Old_Goal>() { Old_Goal.FutureGoal() };

            this.Application = (State s) =>
            {
                State dupe = s.Dupe();
                Variable[] vs = dupe.InstantiateVars(varNames);
                dupe = dupe.DeclareVars(vs);

                Old_Goal mid = fun(vs);
                MarkEvaluated(mid);
                this.SubGoals = new List<Old_Goal>() { mid };

                return mid.PursueIn(dupe.Next());
            };
        }

        private static string FormatDescription(string[] varNames)
        {
            string middle = varNames.Length > 1 ? " variables" : "s a variable";
            return "There exist" + middle + " (" + String.Join(", ", varNames) + ") such that:";
        }

        public CallFresh(Func<Variable, Old_Goal> lambda) : this(
            lambda.Method, v => lambda(v[0]))
        { }
        public CallFresh(Func<Variable, Variable, Old_Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1]))
        { }
        public CallFresh(Func<Variable, Variable, Variable, Old_Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1], v[2]))
        { }
        public CallFresh(Func<Variable, Variable, Variable, Variable, Old_Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1], v[2], v[3]))
        { }
        public CallFresh(Func<Variable, Variable, Variable, Variable, Variable, Old_Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1], v[2], v[3], v[4]))
        { }
        public CallFresh(Func<Variable, Variable, Variable, Variable, Variable, Variable, Old_Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1], v[2], v[3], v[4], v[5]))
        { }
        public CallFresh(Func<Variable, Variable, Variable, Variable, Variable, Variable, Variable, Old_Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1], v[2], v[3], v[4], v[5], v[6]))
        { }
        public CallFresh(Func<Variable, Variable, Variable, Variable, Variable, Variable, Variable, Variable, Old_Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1], v[2], v[3], v[4], v[5], v[6], v[7]))
        { }
    }


    public class Shell : Clause
    {
        public Shell(Old_Goal g, string expression, string description) : base(g => g.First())
        {
            this.Expression = expression;
            this.Description = description;
            this.SubGoals = new List<Old_Goal>() { g };
        }

        public override void Add(Old_Goal g) => throw new InvalidOperationException();
    }


    /// <summary>
    /// Expresses a conjunction (logical-and) of goals
    /// </summary>
    public class Conj : Clause
    {
        public Conj(params Old_Goal[] goals) : base(g => g.Aggregate<Old_Goal>(Old_Goal.Conjunction))
        {
            Description = "All of the following statements are simultaneously true:";
            SubGoals = goals.ToList<Old_Goal>();
        }

        public override string Expression { get => "("+ String.Join(" & ", this.SubGoals) + ")"; }
    }

    /// <summary>
    /// Expresses a disjunction (logical-or) of goals
    /// </summary>
    public class Disj : Clause
    {
        public Disj(params Old_Goal[] goals) : base(g => g.Aggregate(Old_Goal.Disjunction))
        {
            Description = "One or more of the following statements must be true:";
            SubGoals = goals.ToList<Old_Goal>();
        }

        public override string Expression { get => "(" + String.Join(" | ", this.SubGoals) + ")"; }
    }

    /// <summary>
    /// Expresses several goal clauses in conjunctive normal form (conjunction of disjunctions).
    /// eg (A OR B) AND (C OR D)
    /// </summary>
    public class CNF : Clause
    {
        public CNF(params Disj[] goals) : base(g => g.Aggregate(Old_Goal.Conjunction))
        {
            Description = "All of the following clauses are simultaneously true:";
            SubGoals = goals.ToList<Old_Goal>();
        }
        public override string Expression { get => "(" + String.Join(" && ", this.SubGoals) + ")"; }
        public void Add(params Old_Goal[] goals)
        {
            if (goals.Length == 1)
            {
                this.SubGoals.Add(goals[0]);
            }
            else if (goals.Length > 1)
            {
                this.SubGoals.Add(new Disj(goals));
            }
        }
    }

    /// <summary>
    /// Expresses several goal clauses in disjunctive normal form (disjunction of conjunctions).
    /// eg (A AND B) OR (C AND D)
    /// </summary>
    public class DNF : Clause
    {
        public DNF(params Conj[] goals) : base(g => g.Aggregate(Old_Goal.Disjunction))
        {
            Description = "One or more of the following clauses must be true:";
            SubGoals = goals.ToList<Old_Goal>();
        }
        public override string Expression { get => "(" + String.Join(" || ", this.SubGoals) + ")"; }
        public void Add(params Old_Goal[] goals)
        {
            if (goals.Length == 1)
            {
                this.SubGoals.Add(goals[0]);
            }
            else if (goals.Length > 1)
            {
                this.SubGoals.Add(new Conj(goals));
            }
        }
    }
}
