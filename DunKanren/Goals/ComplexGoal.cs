using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren.Goals
{
    /// <summary>
    /// A complex goal encapsulates an operation that is non-trivial to negate.
    /// For now that's just lambda closures (ie existential quantification)
    /// </summary>
    public class ComplexGoal : Goal
    {
        protected ComplexGoal() : base() { }
        protected ComplexGoal(Func<State, Stream> app) : base(app) { }
    }
    public class Exists : ComplexGoal
    {
        private Goal SubGoal;

        private Exists(MethodInfo funInfo, Func<Variable[], Goal> fun) : base()
        {
            string[] varNames = ExtractVarNames(funInfo);
            string varString = String.Join(", ", varNames);

            this.Expression = "Lambda (" + varString + ")";
            this.Description = FormatDescription(varNames);
            this.SubGoal = new FutureGoal();

            this.Application = (State s) =>
            {
                State dupe = s.Dupe();
                Variable[] vs = dupe.InstantiateVars(varNames);
                dupe = dupe.DeclareVars(vs);

                Goal mid = fun(vs);
                this.SubGoal = mid;

                return mid.PursueIn(dupe.Next());
            };
        }

        private static string FormatDescription(string[] varNames)
        {
            string middle = varNames.Length > 1 ? " variables" : "s a variable";
            return "There exist" + middle + " (" + String.Join(", ", varNames) + ") such that:";
        }

        public Exists(Func<Variable, Goal> lambda) : this(
            lambda.Method, v => lambda(v[0]))
        { }
        public Exists(Func<Variable, Variable, Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1]))
        { }
        public Exists(Func<Variable, Variable, Variable, Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1], v[2]))
        { }
        public Exists(Func<Variable, Variable, Variable, Variable, Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1], v[2], v[3]))
        { }
        public Exists(Func<Variable, Variable, Variable, Variable, Variable, Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1], v[2], v[3], v[4]))
        { }
        public Exists(Func<Variable, Variable, Variable, Variable, Variable, Variable, Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1], v[2], v[3], v[4], v[5]))
        { }
        public Exists(Func<Variable, Variable, Variable, Variable, Variable, Variable, Variable, Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1], v[2], v[3], v[4], v[5], v[6]))
        { }
        public Exists(Func<Variable, Variable, Variable, Variable, Variable, Variable, Variable, Variable, Goal> lambda) : this(
            lambda.Method, v => lambda(v[0], v[1], v[2], v[3], v[4], v[5], v[6], v[7]))
        { }
    }
}
