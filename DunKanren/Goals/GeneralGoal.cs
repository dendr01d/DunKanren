//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Drawing;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Runtime.CompilerServices;
//using System.Security.Cryptography;
//using System.Security.Cryptography.X509Certificates;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml;

//namespace DunKanren.Goals.General;

///// <summary>
///// A statement defining a relationship between terms
///// </summary>
//public class GeneralGoal : IPrintable
//{
//    private static int IDGen = 0;

//    private int PersonalID;

//    /// <summary>
//    /// Simple/Mathematical expression of what the Goal represents
//    /// </summary>
//    public virtual string Expression { get; protected set; }
//    /// <summary>
//    /// Longer-form description of what statement the goal represents
//    /// </summary>
//    public virtual string Description { get; protected set; }


//    protected Func<State, Stream> Application;

//    protected List<GeneralGoal> SubGoals;

//    private static GeneralGoal BaseGoal = null!;
//    private static int BaseLineCount = 0;

//    protected GeneralGoal()
//    {
//        PersonalID = IDGen++;
//        Expression = "Unknown Goal";
//        Description = "Application of Unknown Goal";
//        Application = (s) => Stream.Singleton(s);
//        SubGoals = new List<GeneralGoal>();
//    }

//    protected static GeneralGoal FutureGoal()
//    {
//        return new GeneralGoal()
//        {
//            Expression = "{...}",
//            Description = "{Unevaluated Lambda Expression}"
//        };
//    }

//    protected GeneralGoal(Func<State, Stream> fun) : this()
//    {
//        Application = fun;
//    }

//    protected GeneralGoal(Func<IEnumerable<GeneralGoal>, GeneralGoal> agg) : this()
//    {
//        Application = (s) => agg(SubGoals).Application(s);
//    }

//    protected GeneralGoal(GeneralGoal g) : this(g.Application)
//    {
//        Expression = g.Expression;
//        Description = g.Description;
//        SubGoals = g.SubGoals.Select(x => new GeneralGoal(x)).ToList();
//    }

//    //see if the base goal has grown since the last time it was printed, then maybe print it again
//    private static void ConditionalPrint(State s)
//    {
//        IEnumerable<string> lines = BaseGoal.ToTree();

//        if (lines.Count() > BaseLineCount)
//        {
//            BaseLineCount = lines.Count();
//            Console.WriteLine(BaseGoal.GetName() + " @ Depth " + s.RecursionLevel);
//            Console.WriteLine(IO.Graft(lines));
//        }
//    }

//    public Stream PursueIn(State s)
//    {

//        if (SubGoals.Any())
//        {
//            IO.Debug_Print(IO.Graft(ToTree()));
//        }
//        else
//        {
//            IO.Debug_Print(ToString());
//        }


//        return Application(s);
//    }

//    public Stream Pursue()
//    {
//        BaseGoal = this;
//        return PursueIn(State.InitialState());
//    }

//    protected static void MarkEvaluated(GeneralGoal g)
//    {
//        g.Description = "*" + g.Description;
//    }

//    protected static string[] ExtractVarNames(MethodInfo funInfo)
//    {
//        char defaultSymbol = 'A';
//        return funInfo.GetParameters().Select(x => x.Name ?? "v" + defaultSymbol++.ToString()).ToArray();
//    }

//    #region Primitive Goals

//    //protected delegate Stream PrimitiveGoal(State s);

//    //There's really no good primitive way to do this, since it's self-referential in terms of the goal that contains the primitive
//    protected static GeneralGoal Primitive_Fresh(Func<Variable, GeneralGoal> fun, GeneralGoal shell)
//    {
//        return new GeneralGoal((s) =>
//        {
//            Variable v = new Variable(s, ExtractVarNames(fun.Method)[0]);
//            return fun(v).PursueIn(s.DeclareVars(v).Next());
//        });
//    }

//    protected static GeneralGoal Primitive_Equality(Term t1, Term t2) => new GeneralGoal((s) => Stream.Singleton(s.Unify(t1, t2)));
//    protected static GeneralGoal Primitive_DisEquality(Term t1, Term t2) => new GeneralGoal((s) => Stream.Singleton(s.DisUnify(t1, t2)));

//    protected static GeneralGoal Primitive_Conjunction(GeneralGoal g1, GeneralGoal g2) => new GeneralGoal((s) => Stream.New(g1.PursueIn(s).SelectMany(x => g2.PursueIn(x))));

//    protected static GeneralGoal Primitive_Disjunction(GeneralGoal g1, GeneralGoal g2) => new GeneralGoal((s) => Stream.Interleave(g1.PursueIn(s), g2.PursueIn(s)));

//    #endregion


//    #region Static Callables

//    public static GeneralGoal CallFresh(Func<Variable, GeneralGoal> lambda) => new GeneralGoal()
//    {
//        Expression = "Lambda (" + (lambda.Method.GetParameters()[0].Name ?? "var") + ")",
//        Description = "There exists a variable (" + lambda.Method.GetParameters()[0].Name ?? "var" + ") such that:",
//        SubGoals = new List<GeneralGoal>() { FutureGoal() }
//    };

//    public static GeneralGoal Equality(Term t1, Term t2) => new GeneralGoal(Primitive_Equality(t1, t2))
//    {
//        Expression = "(" + t1.ToString() + " ≡ " + t2.ToString() + ")",
//        Description = "'" + t1.ToString() + "' equals '" + t2.ToString() + "'"
//    };

//    public static GeneralGoal Disequality(Term t1, Term t2) => new GeneralGoal(Primitive_DisEquality(t1, t2))
//    {
//        Expression = "(" + t1.ToString() + " ╪ " + t2.ToString() + ")",
//        Description = "'" + t1.ToString() + "' never equals '" + t2.ToString() + "'"
//    };

//    public static GeneralGoal Conjunction(GeneralGoal g1, GeneralGoal g2) => new GeneralGoal(Primitive_Conjunction(g1, g2))
//    {
//        Expression = "(" + g1.ToString() + " & " + g2.ToString() + ")",
//        Description = "Both of the following statements are true:",
//        SubGoals = new List<GeneralGoal>() { g1, g2 }
//    };
//    public static GeneralGoal Disjunction(GeneralGoal g1, GeneralGoal g2) => new GeneralGoal(Primitive_Disjunction(g1, g2))
//    {
//        Expression = "(" + g1.ToString() + " | " + g2.ToString() + ")",
//        Description = "One or both of the following statements are true:",
//        SubGoals = new List<GeneralGoal>() { g1, g2 }
//    };

//    #endregion

//    public string GetName() => "Goal #" + PersonalID + ": " + ToString();

//    public override string ToString() => Expression;
//    public virtual string ToVerboseString() => Description;

//    public IEnumerable<string> ToTree() => ToTree("", true, false);
//    public IEnumerable<string> ToTree(string prefix, bool first, bool last)
//    {
//        string parentPrefix = first ? "" : prefix + (last ? IO.LEAVES : IO.BRANCH);
//        string childPrefix = first ? "" : prefix + (last ? IO.SPACER : IO.JUMPER);


//        //yield return parentPrefix + IO.HEADER + g.Description;

//        using (var iter = SubGoals.GetEnumerator())
//        {
//            bool move1 = iter.MoveNext();

//            //if this goal won't print any children then 
//            yield return parentPrefix + (move1 ? IO.HEADER : IO.ALONER) + Description;

//            GeneralGoal sub = iter.Current;

//            while (sub != null)
//            {
//                if (iter.MoveNext())
//                {
//                    foreach (string line in sub.ToTree(childPrefix, false, false))
//                    {
//                        yield return line;
//                    }
//                }
//                else
//                {
//                    foreach (string line in sub.ToTree(childPrefix, false, true))
//                    {
//                        yield return line;
//                    }
//                    //yield return prefix + IO.JUMPER;
//                }

//                sub = iter.Current;
//            }
//        }
//    }
//}


//public abstract class Clause : GeneralGoal, IEnumerable<GeneralGoal>
//{
//    protected Clause(Func<IEnumerable<GeneralGoal>, GeneralGoal> aggregateParams) : base(aggregateParams) { }

//    public virtual void Add(GeneralGoal g)
//    {
//        SubGoals.Add(g);
//    }

//    public void Add(string ex, string desc)
//    {
//        Expression = ex;
//        Description = desc;
//    }

//    public IEnumerator<GeneralGoal> GetEnumerator() => SubGoals.GetEnumerator();

//    IEnumerator IEnumerable.GetEnumerator() => SubGoals.GetEnumerator();
//}



//public class CallFresh : GeneralGoal
//{
//    private CallFresh(MethodInfo funInfo, Func<Variable[], GeneralGoal> fun) : base()
//    {
//        string[] varNames = ExtractVarNames(funInfo);
//        string varString = string.Join(", ", varNames);

//        Expression = "Lambda (" + varString + ")";
//        Description = FormatDescription(varNames);
//        SubGoals = new List<GeneralGoal>() { FutureGoal() };

//        Application = (s) =>
//        {
//            State dupe = s.Dupe();
//            Variable[] vs = dupe.InstantiateVars(varNames);
//            dupe = dupe.DeclareVars(vs);

//            GeneralGoal mid = fun(vs);
//            MarkEvaluated(mid);
//            SubGoals = new List<GeneralGoal>() { mid };

//            return mid.PursueIn(dupe.Next());
//        };
//    }

//    private static string FormatDescription(string[] varNames)
//    {
//        string middle = varNames.Length > 1 ? " variables" : "s a variable";
//        return "There exist" + middle + " (" + string.Join(", ", varNames) + ") such that:";
//    }

//    public CallFresh(Func<Variable, GeneralGoal> lambda) : this(
//        lambda.Method, v => lambda(v[0]))
//    { }
//    public CallFresh(Func<Variable, Variable, GeneralGoal> lambda) : this(
//        lambda.Method, v => lambda(v[0], v[1]))
//    { }
//    public CallFresh(Func<Variable, Variable, Variable, GeneralGoal> lambda) : this(
//        lambda.Method, v => lambda(v[0], v[1], v[2]))
//    { }
//    public CallFresh(Func<Variable, Variable, Variable, Variable, GeneralGoal> lambda) : this(
//        lambda.Method, v => lambda(v[0], v[1], v[2], v[3]))
//    { }
//    public CallFresh(Func<Variable, Variable, Variable, Variable, Variable, GeneralGoal> lambda) : this(
//        lambda.Method, v => lambda(v[0], v[1], v[2], v[3], v[4]))
//    { }
//    public CallFresh(Func<Variable, Variable, Variable, Variable, Variable, Variable, GeneralGoal> lambda) : this(
//        lambda.Method, v => lambda(v[0], v[1], v[2], v[3], v[4], v[5]))
//    { }
//    public CallFresh(Func<Variable, Variable, Variable, Variable, Variable, Variable, Variable, GeneralGoal> lambda) : this(
//        lambda.Method, v => lambda(v[0], v[1], v[2], v[3], v[4], v[5], v[6]))
//    { }
//    public CallFresh(Func<Variable, Variable, Variable, Variable, Variable, Variable, Variable, Variable, GeneralGoal> lambda) : this(
//        lambda.Method, v => lambda(v[0], v[1], v[2], v[3], v[4], v[5], v[6], v[7]))
//    { }
//}


//public class Shell : Clause
//{
//    public Shell(GeneralGoal g, string expression, string description) : base(g => g.First())
//    {
//        Expression = expression;
//        Description = description;
//        SubGoals = new List<GeneralGoal>() { g };
//    }

//    public override void Add(GeneralGoal g) => throw new InvalidOperationException();
//}


///// <summary>
///// Expresses a conjunction (logical-and) of goals
///// </summary>
//public class Conj : Clause
//{
//    public Conj(params GeneralGoal[] goals) : base(g => g.Aggregate(Conjunction))
//    {
//        Description = "All of the following statements are simultaneously true:";
//        SubGoals = goals.ToList();
//    }

//    public override string Expression { get => "(" + string.Join(" & ", SubGoals) + ")"; }
//}

///// <summary>
///// Expresses a disjunction (logical-or) of goals
///// </summary>
//public class Disj : Clause
//{
//    public Disj(params GeneralGoal[] goals) : base(g => g.Aggregate(Disjunction))
//    {
//        Description = "One or more of the following statements must be true:";
//        SubGoals = goals.ToList();
//    }

//    public override string Expression { get => "(" + string.Join(" | ", SubGoals) + ")"; }
//}

///// <summary>
///// Expresses several goal clauses in conjunctive normal form (conjunction of disjunctions).
///// eg (A OR B) AND (C OR D)
///// </summary>
//public class CNF : Clause
//{
//    public CNF(params Disj[] goals) : base(g => g.Aggregate(Conjunction))
//    {
//        Description = "All of the following clauses are simultaneously true:";
//        SubGoals = goals.ToList<GeneralGoal>();
//    }
//    public override string Expression { get => "(" + string.Join(" && ", SubGoals) + ")"; }
//    public void Add(params GeneralGoal[] goals)
//    {
//        if (goals.Length == 1)
//        {
//            SubGoals.Add(goals[0]);
//        }
//        else if (goals.Length > 1)
//        {
//            SubGoals.Add(new Disj(goals));
//        }
//    }
//}

///// <summary>
///// Expresses several goal clauses in disjunctive normal form (disjunction of conjunctions).
///// eg (A AND B) OR (C AND D)
///// </summary>
//public class DNF : Clause
//{
//    public DNF(params Conj[] goals) : base(g => g.Aggregate(Disjunction))
//    {
//        Description = "One or more of the following clauses must be true:";
//        SubGoals = goals.ToList<GeneralGoal>();
//    }
//    public override string Expression { get => "(" + string.Join(" || ", SubGoals) + ")"; }
//    public void Add(params GeneralGoal[] goals)
//    {
//        if (goals.Length == 1)
//        {
//            SubGoals.Add(goals[0]);
//        }
//        else if (goals.Length > 1)
//        {
//            SubGoals.Add(new Conj(goals));
//        }
//    }
//}
