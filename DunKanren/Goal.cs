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
    public class Goal : IPrintable
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

        protected List<Goal> SubGoals;

        private static Goal BaseGoal = null!;
        private static int BaseLineCount = 0;

        protected Goal()
        {
            PersonalID = IDGen++;
            this.Expression = "Unknown Goal";
            this.Description = "Application of Unknown Goal";
            this.Application = (State s) => Stream.Singleton(s);
            this.SubGoals = new List<Goal>();
        }

        protected static Goal FutureGoal()
        {
            return new Goal()
            {
                Expression = "{...}",
                Description = "{Unevaluated Lambda Expression}"
            };
        }

        protected Goal(Func<State, Stream> fun) : this()
        {
            this.Application = fun;
        }

        protected Goal(Func<IEnumerable<Goal>, Goal> agg) : this()
        {
            this.Application = (State s) => agg(this.SubGoals).Application(s);
        }

        protected Goal(Goal g) : this(g.Application)
        {
            this.Expression = g.Expression;
            this.Description = g.Description;
            this.SubGoals = g.SubGoals.Select(x => new Goal(x)).ToList();
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

        protected static void MarkEvaluated(Goal g)
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
        protected static Goal Primitive_Fresh(Func<Variable, Goal> fun, Goal shell)
        {
            return new Goal((State s) =>
            {
                Variable v = new Variable(s, ExtractVarNames(fun.Method)[0]);
                return fun(v).PursueIn(s.DeclareVars(v).Next());
            });
        }

        protected static Goal Primitive_Equality(Term t1, Term t2) => new Goal((State s) => Stream.Singleton(s.Unify(t1, t2)));
        protected static Goal Primitive_DisEquality(Term t1, Term t2) => new Goal((State s) => Stream.Singleton(s.DisUnify(t1, t2)));

        protected static Goal Primitive_Conjunction(Goal g1, Goal g2) => new Goal((State s) => Stream.New(g1.PursueIn(s).SelectMany(x => g2.PursueIn(x))));

        protected static Goal Primitive_Disjunction(Goal g1, Goal g2) => new Goal((State s) => Stream.Interleave(g1.PursueIn(s), g2.PursueIn(s)));

        #endregion


        #region Static Callables

        public static Goal CallFresh(Func<Variable, Goal> lambda) => new Goal()
        {
            Expression = "Lambda (" + (lambda.Method.GetParameters()[0].Name ?? "var") + ")",
            Description = "There exists a variable (" + lambda.Method.GetParameters()[0].Name ?? "var" + ") such that:",
            SubGoals = new List<Goal>() { FutureGoal() }
        };

        public static Goal Equality(Term t1, Term t2) => new Goal(Primitive_Equality(t1, t2))
        {
            Expression = "(" + t1.ToString() + " ≡ " + t2.ToString() + ")",
            Description = "'" + t1.ToString() + "' equals '" + t2.ToString() + "'"
        };

        public static Goal Disequality(Term t1, Term t2) => new Goal(Primitive_DisEquality(t1, t2))
        {
            Expression = "(" + t1.ToString() + " ╪ " + t2.ToString() + ")",
            Description = "'" + t1.ToString() + "' never equals '" + t2.ToString() + "'"
        };

        public static Goal Conjunction(Goal g1, Goal g2) => new Goal(Primitive_Conjunction(g1, g2))
        {
            Expression = "(" + g1.ToString() + " & " + g2.ToString() + ")",
            Description = "Both of the following statements are true:",
            SubGoals = new List<Goal>() { g1, g2 }
        };
        public static Goal Disjunction(Goal g1, Goal g2) => new Goal(Primitive_Disjunction(g1, g2))
        {
            Expression = "(" + g1.ToString() + " | " + g2.ToString() + ")",
            Description = "One or both of the following statements are true:",
            SubGoals = new List<Goal>() { g1, g2 }
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

                Goal sub = iter.Current;

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


    public abstract class Clause : Goal, IEnumerable<Goal>
    {
        protected Clause(Func<IEnumerable<Goal>, Goal> aggregateParams) : base(aggregateParams) { }

        public virtual void Add(Goal g)
        {
            this.SubGoals.Add(g);
        }

        public void Add(string ex, string desc)
        {
            this.Expression = ex;
            this.Description = desc;
        }

        public IEnumerator<Goal> GetEnumerator() => this.SubGoals.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.SubGoals.GetEnumerator();
    }


    
    public class CallFresh : Goal
    {
        private CallFresh(MethodInfo funInfo, Func<Variable[], Goal> fun) : base()
        {
            string[] varNames = ExtractVarNames(funInfo);
            string varString = String.Join(", ", varNames);

            this.Expression = "Lambda (" + varString + ")";
            this.Description = FormatDescription(varNames);
            this.SubGoals = new List<Goal>() { Goal.FutureGoal() };

            this.Application = (State s) =>
            {
                State dupe = s.Dupe();
                Variable[] vs = dupe.InstantiateVars(varNames);
                dupe = dupe.DeclareVars(vs);

                Goal mid = fun(vs);
                MarkEvaluated(mid);
                this.SubGoals = new List<Goal>() { mid };

                return mid.PursueIn(dupe.Next());
            };
        }

        private static string FormatDescription(string[] varNames)
        {
            string middle = varNames.Length > 1 ? " variables" : "s a variable";
            return "There exist" + middle + " (" + String.Join(", ", varNames) + ") such that:";
        }

        public CallFresh(Func<Variable, Goal> lambda) : this(
            lambda.Method, v => lambda(v[0]))
        { }
        public CallFresh(Func<Variable, Variable, Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1]))
        { }
        public CallFresh(Func<Variable, Variable, Variable, Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1], v[2]))
        { }
        public CallFresh(Func<Variable, Variable, Variable, Variable, Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1], v[2], v[3]))
        { }
        public CallFresh(Func<Variable, Variable, Variable, Variable, Variable, Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1], v[2], v[3], v[4]))
        { }
        public CallFresh(Func<Variable, Variable, Variable, Variable, Variable, Variable, Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1], v[2], v[3], v[4], v[5]))
        { }
        public CallFresh(Func<Variable, Variable, Variable, Variable, Variable, Variable, Variable, Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1], v[2], v[3], v[4], v[5], v[6]))
        { }
        public CallFresh(Func<Variable, Variable, Variable, Variable, Variable, Variable, Variable, Variable, Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1], v[2], v[3], v[4], v[5], v[6], v[7]))
        { }
    }


    public class Shell : Clause
    {
        public Shell(Goal g, string expression, string description) : base(g => g.First())
        {
            this.Expression = expression;
            this.Description = description;
            this.SubGoals = new List<Goal>() { g };
        }

        public override void Add(Goal g) => throw new InvalidOperationException();
    }


    /// <summary>
    /// Expresses a conjunction (logical-and) of goals
    /// </summary>
    public class Conj : Clause
    {
        public Conj(params Goal[] goals) : base(g => g.Aggregate<Goal>(Goal.Conjunction))
        {
            Description = "All of the following statements are simultaneously true:";
            SubGoals = goals.ToList<Goal>();
        }

        public override string Expression { get => "("+ String.Join(" & ", this.SubGoals) + ")"; }
    }

    /// <summary>
    /// Expresses a disjunction (logical-or) of goals
    /// </summary>
    public class Disj : Clause
    {
        public Disj(params Goal[] goals) : base(g => g.Aggregate(Goal.Disjunction))
        {
            Description = "One or more of the following statements must be true:";
            SubGoals = goals.ToList<Goal>();
        }

        public override string Expression { get => "(" + String.Join(" | ", this.SubGoals) + ")"; }
    }

    /// <summary>
    /// Expresses several goal clauses in conjunctive normal form (conjunction of disjunctions).
    /// eg (A OR B) AND (C OR D)
    /// </summary>
    public class CNF : Clause
    {
        public CNF(params Disj[] goals) : base(g => g.Aggregate(Goal.Conjunction))
        {
            Description = "All of the following clauses are simultaneously true:";
            SubGoals = goals.ToList<Goal>();
        }
        public override string Expression { get => "(" + String.Join(" && ", this.SubGoals) + ")"; }
        public void Add(params Goal[] goals)
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
        public DNF(params Conj[] goals) : base(g => g.Aggregate(Goal.Disjunction))
        {
            Description = "One or more of the following clauses must be true:";
            SubGoals = goals.ToList<Goal>();
        }
        public override string Expression { get => "(" + String.Join(" || ", this.SubGoals) + ")"; }
        public void Add(params Goal[] goals)
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
