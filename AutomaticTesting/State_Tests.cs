namespace AutomaticTesting
{
    [TestClass]
    public class State_Tests
    {
        [TestInitialize]
        public void Boilerplate()
        {
            IO.DisablePrompting();
        }

        [TestMethod]
        public void Test_Default()
        {
            State s = State.InitialState();

            Assert.AreEqual(s.Subs.Any(), false);
            Assert.AreEqual(s.RecursionLevel, 0);
            Assert.AreEqual(s.VariableCounter, 0);
        }

        [TestMethod, Timeout(2000)]
        public void Test_Dupe()
        {
            State s = State.InitialState();
            (s, _) = s.DeclareVar("test");
            State s2 = s.Dupe();

            Assert.AreNotSame(s, s2);

            Assert.AreEqual(s.RecursionLevel, s2.RecursionLevel);
            Assert.AreEqual(1, s.VariableCounter);
            Assert.AreEqual(s.VariableCounter, s2.VariableCounter);

            //apparently a quirk of immutabledictionaries is that depending on how you construct them
            // -- if they have the same contents -- then they end up sharing a reference
            //even though they're not supposed to...?
            Assert.AreNotSame(s.Subs, s2.Subs);
            CollectionAssert.AreEqual(s.Subs, s2.Subs);
        }

        [TestMethod, Timeout(2000)]
        public void Test_Recurse()
        {
            State s = State.InitialState();
            State r = s.Next();

            Assert.AreEqual(r.RecursionLevel, 1);
        }

        [TestMethod, Timeout(2000)]
        public void Test_Declaration()
        {
            State s = State.InitialState();

            Variable[] newVars = s.DeclareVars(out s, "test1", "test2", "test3");

            CollectionAssert.AreEquivalent(newVars, s.Subs.Keys.ToArray());

            foreach(Variable var in newVars)
            {
                Assert.AreEqual(true, s.Subs[var] is Instance.Indefinite);
            }

            Assert.AreEqual(3, s.VariableCounter);
        }

        [TestMethod, Timeout(2000)]
        public void Test_Walk()
        {
            State s = State.InitialState();

            Variable[] newVars = s.DeclareVars(out s, "test1", "test2", "test3", "test4");
            Variable extra = s.DeclareVars(out s, "test5").First();

            for(int i = 0; i < newVars.Length - 1; ++i)
            {
                bool succ = s.TryUnify(newVars[i], newVars[i + 1], out s);
                Assert.AreEqual(true, succ);
            }

            Assert.AreEqual(newVars[3], s.Walk(newVars[0]));
            Assert.AreEqual(newVars[3], s.Walk(newVars[1]));
            Assert.AreEqual(newVars[3], s.Walk(newVars[2]));

            Assert.AreNotEqual(newVars[3], s.Walk(extra));
            Assert.AreNotEqual(extra, s.Walk(newVars[0]));

            Assert.AreEqual(extra, s.Walk(extra));
        }

        [TestMethod, Timeout(2000)]
        public void Test_Unification()
        {
            State s = State.InitialState();

            (s, Variable x) = s.DeclareVar("x");
            (s, Variable y) = s.DeclareVar("y");

            Value<int> five = new Value<int>(5);
            Value<int> six = new Value<int>(6);

            bool succ1 = s.TryUnify(x, five, out s);
            Assert.AreEqual(true, succ1);
            Assert.AreEqual(true, s.Subs[x] is Instance.Definite def1 && def1.Definition.Equals(five));

            bool succ2 = s.TryUnify(y, six, out s);
            Assert.AreEqual(true, succ2);
            Assert.AreEqual(true, s.Subs[y] is Instance.Definite def2 && def2.Definition.Equals(six));

            bool succ3 = s.TryUnify(x, y, out s);
            Assert.AreEqual(false, succ3);
            Assert.AreEqual(true, s.Subs[x] is Instance.Definite def3_1 && def3_1.Definition.Equals(five));
            Assert.AreEqual(true, s.Subs[y] is Instance.Definite def3_2 && def3_2.Definition.Equals(six));

            bool succ4 = s.TryUnify(five, six, out s);
            Assert.AreEqual(false, succ4);
            Assert.AreEqual(true, s.Subs[x] is Instance.Definite def4_1 && def4_1.Definition.Equals(five));
            Assert.AreEqual(true, s.Subs[y] is Instance.Definite def4_2 && def4_2.Definition.Equals(six));
        }

        [TestMethod, Timeout(2000)]
        public void Test_DisUnification()
        {
            State s = State.InitialState();

            (s, Variable x) = s.DeclareVar("x");
            (s, Variable y) = s.DeclareVar("y");

            Value<int> five = new(5);
            Value<int> six = new(6);

            //concrete != different concrete
            //different concrete values can never unify -- ie they always disunify
            bool succ1 = s.TryDisUnify(five, six, out s);
            Assert.AreEqual(true, succ1);

            //concrete != itself
            //similarly, a single value will always unify with itself
            bool succ8 = s.TryDisUnify(five, five, out s);
            Assert.AreEqual(false, succ8);

            //variable != concrete
            //constrain x to not be 6
            bool succ2 = s.TryDisUnify(x, six, out s);
            Assert.AreEqual(true, succ2);
            Assert.AreEqual(true, s.Subs[x] is Instance.Indefinite x2 && x2.RuleCount == 1);

            //enforce variable != concrete
            //x cannot be 6 per the constraint
            bool succ3 = s.TryUnify(x, six, out s);
            Assert.AreEqual(false, succ3);
            Assert.AreEqual(true, s.Subs[x] is Instance.Indefinite x3 && x3.RuleCount == 1);

            //define y to be 6
            bool succ4 = s.TryUnify(y, six, out s);
            Assert.AreEqual(true, succ4);
            Assert.AreEqual(true, s.Subs[x] is Instance.Indefinite x4 && x4.RuleCount == 1);
            Assert.AreEqual(true, s.Subs[y] is Instance.Definite y4 && y4.Definition.Equals(six));

            //variable != concrete by transitivity
            //x cannot be y, because x cannot be 6, and y is 6
            bool succ5 = s.TryUnify(x, y, out s);
            Assert.AreEqual(false, succ5);
            Assert.AreEqual(true, s.Subs[x] is Instance.Indefinite x5 && x5.RuleCount == 1);
            Assert.AreEqual(true, s.Subs[y] is Instance.Definite y5 && y5.Definition.Equals(six));

            //define x to be 5
            bool succ7 = s.TryUnify(x, five, out s);
            Assert.AreEqual(true, succ7);
            Assert.AreEqual(true, s.Subs[x] is Instance.Definite x1 && x1.Definition.Equals(five));
            Assert.AreEqual(true, s.Subs[y] is Instance.Definite y1 && y1.Definition.Equals(six));

            //variable == concrete => !(variable != concrete)
            //x is 5, so we can no longer constrain its value to be anything but 5
            bool succ9 = s.TryDisUnify(x, five, out s);
            Assert.AreEqual(false, succ9);
            Assert.AreEqual(true, s.Subs[x] is Instance.Definite x9 && x9.Definition.Equals(five));
            Assert.AreEqual(true, s.Subs[y] is Instance.Definite y9 && y9.Definition.Equals(six));

            Value<int> eight = new(8);

            //variable == concrete is congruent with variable != other concrete
            //we can no longer constrain x's value, but we can still test its definition via constraint
            bool succ10 = s.TryDisUnify(x, eight, out s);
            Assert.AreEqual(true, succ10);
            Assert.AreEqual(true, s.Subs[x] is Instance.Definite x10 && x10.Definition.Equals(five));
            Assert.AreEqual(true, s.Subs[y] is Instance.Definite y10 && y10.Definition.Equals(six));

            //concrete != concrete (by double transitivity)
            //x and y are different values, so they can never unify
            bool succ11 = s.TryDisUnify(x, y, out s);
            Assert.AreEqual(true, succ11);
            Assert.AreEqual(true, s.Subs[x] is Instance.Definite x11 && x11.Definition.Equals(five));
            Assert.AreEqual(true, s.Subs[y] is Instance.Definite y11 && y11.Definition.Equals(six));
        }


        [TestMethod, Timeout(2000)]
        public void Test_Differentiation()
        {
            State s = State.InitialState();
            List<Variable> vars = s.DeclareVars(out s, "a", "b", "c", "d", "e").ToList();

            //      this minus this
            //0: definite   vs indefinite
            //1: indefinite vs definite
            //2: definite   vs definite
            //3: indefinite vs indefinite (different restrictions)
            //4: indefinite vs indefinite (same restrictions)
            //5: definite   vs none
            //6: indefinite vs none

            vars.AddRange(s.DeclareVars(out State s1, "f", "g").ToList());
            s1 = s1
                   .Unify(vars[0], 5)!
                .DisUnify(vars[1], 5)!
                   .Unify(vars[2], 5)!
                .DisUnify(vars[3], 5)!
                .DisUnify(vars[4], 5)!
                   .Unify(vars[5], 5)!
                .DisUnify(vars[6], 5)!;

            State s2 = s
                .DisUnify(vars[0], 6)!
                   .Unify(vars[1], 6)!
                   .Unify(vars[2], 6)!
                .DisUnify(vars[3], 6)!
                .DisUnify(vars[4], 5)!;

            State diff = s2.SubtractFrom(s1); //s1 - s2
            Assert.AreEqual(7, diff.Subs.Count());

            Assert.AreEqual(true, diff.Subs[vars[0]].Equals(new Instance.Definite(5)));
            Assert.AreEqual(true, diff.Subs[vars[1]].Equals(new Instance.Definite(6)));
            Assert.AreEqual(true, diff.Subs[vars[2]].Equals(new Instance.Definite(5)));
            Assert.AreEqual(true, diff.Subs[vars[3]].Equals(new Instance.Indefinite(new Constraint.Inequality(6))));
            Assert.AreEqual(true, diff.Subs[vars[4]].Equals(new Instance.Definite(5)));
            Assert.AreEqual(true, diff.Subs[vars[5]].Equals(new Instance.Definite(5)));
            Assert.AreEqual(true, diff.Subs[vars[6]].Equals(new Instance.Indefinite(new Constraint.Inequality(5))));

        }
    }
}


