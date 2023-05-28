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
            Assert.AreEqual(s.Negs.Any(), false);
            Assert.AreEqual(s.RecursionLevel, 0);
            Assert.AreEqual(s.VariableCounter, 0);
        }

        [TestMethod]
        public void Test_Dupe()
        {
            State s = State.InitialState();
            Variable v = new Variable(ref s, "test");
            State s2 = s.Dupe();

            Assert.AreNotSame(s, s2);

            Assert.AreEqual(s.RecursionLevel, s2.RecursionLevel);
            Assert.AreEqual(s.VariableCounter, s2.VariableCounter);

            Assert.AreNotSame(s.Subs, s2.Subs);
            CollectionAssert.AreEqual(s.Subs, s2.Subs);

            Assert.AreNotSame(s.Negs, s2.Negs);
            CollectionAssert.AreEqual(s.Negs, s2.Negs);
        }

        [TestMethod]
        public void Test_Recurse()
        {
            State s = State.InitialState();
            State r = s.Next();

            Assert.AreEqual(r.RecursionLevel, 1);
        }

        [TestMethod]
        public void Test_Declaration()
        {
            State s = State.InitialState();

            Variable[] newVars = s.DeclareVars(out s, "test1", "test2", "test3");

            CollectionAssert.AreEquivalent(newVars, s.Subs.Keys);

            foreach(Variable var in newVars)
            {
                Assert.AreEqual(var, s.Subs[var]);
            }

            Assert.AreEqual(3, s.VariableCounter);
        }

        [TestMethod]
        public void Test_Walk()
        {
            State s = State.InitialState();

            Variable[] newVars = s.DeclareVars(out s, "test1", "test2", "test3", "test4");
            Variable extra = s.DeclareVars(out s, "test5").First();

            for(int i = 0; i < newVars.Length - 1; ++i)
            {
                s.TryExtend(newVars[i], newVars[i + 1], out s);
            }

            Assert.AreEqual(newVars[3], s.Walk(newVars[0]));
            Assert.AreEqual(newVars[3], s.Walk(newVars[1]));
            Assert.AreEqual(newVars[3], s.Walk(newVars[2]));

            Assert.AreNotEqual(newVars[3], s.Walk(extra));
            Assert.AreNotEqual(extra, s.Walk(newVars[0]));

            Assert.AreEqual(extra, s.Walk(extra));
        }

        [TestMethod]
        public void Test_Extension()
        {
            State s = State.InitialState();

            Variable[] altVars = State.InitialState().DeclareVars(out State _, "test1", "test2", "test3");

            bool succ1 = s.TryExtend(altVars[0], altVars[1], out s);
            bool succ2 = s.TryExtend(altVars[1], altVars[2], out s);

            Assert.AreEqual(true, succ1);
            Assert.AreEqual(true, succ2);

            Assert.AreEqual(s.Subs[altVars[0]], altVars[1]);
            Assert.AreEqual(s.Subs[altVars[1]], altVars[2]);
            Assert.AreEqual(2, s.VariableCounter);


            bool succ3 = s.TryExtend(altVars[0], altVars[2], out s);
            Assert.AreEqual(false, succ3);
            Assert.AreNotEqual(altVars[2], s.Subs[altVars[0]]);


            bool succ4 = s.TryExtend(altVars[0], altVars[1], out State s2);
            Assert.AreEqual(true, succ4);
            CollectionAssert.AreEquivalent(s.Subs, s2.Subs);

            s2.Negs.Add(altVars[2], new HashSet<Term>() { altVars[0], altVars[1] });

            bool succ5 = s2.TryExtend(altVars[2], altVars[0], out State s3);
            Assert.AreEqual(false, succ5);
            CollectionAssert.AreEquivalent(s2.Subs, s3.Subs);

            bool succ6 = s2.TryExtend(altVars[2], altVars[1], out State s4);
            Assert.AreEqual(false, succ6);
            CollectionAssert.AreEquivalent(s2.Subs, s4.Subs);
        }

        [TestMethod]
        public void Test_Unification()
        {
            State s = State.InitialState();

            Variable x = new(ref s, "x");
            Variable y = new(ref s, "y");

            Value<int> five = new Value<int>(5);
            Value<int> six = new Value<int>(6);

            bool succ1 = s.TryUnify(x, five, out s);
            Assert.AreEqual(true, succ1);
            Assert.AreEqual(five, s.Subs[x]);

            bool succ2 = s.TryUnify(y, six, out s);
            Assert.AreEqual(true, succ2);
            Assert.AreEqual(six, s.Subs[y]);

            bool succ3 = s.TryUnify(x, y, out s);
            Assert.AreEqual(false, succ3);
            Assert.AreEqual(five, s.Subs[x]);
            Assert.AreEqual(six, s.Subs[y]);

            bool succ4 = s.TryUnify(five, six, out s);
            Assert.AreEqual(false, succ4);
            Assert.AreEqual(five, s.Subs[x]);
            Assert.AreEqual(six, s.Subs[y]);
        }

        [TestMethod]
        public void Test_DisUnification()
        {
            State s = State.InitialState();

            Variable x = new(ref s, "x");
            Variable y = new(ref s, "y");

            Value<int> five = new Value<int>(5);
            Value<int> six = new Value<int>(6);

            //bool succ1 = s.TryUnify(x, five, out s);
            //Assert.AreEqual(true, succ1);

            bool succ2 = s.TryDisUnify(x, six, out s);
            Assert.AreEqual(true, succ2);
            Assert.AreEqual(true, s.Negs.ContainsKey(x));
            Assert.AreEqual(true, s.Negs[x].Contains(six));

            bool succ3 = s.TryUnify(x, six, out s);
            Assert.AreEqual(false, succ3);
            Assert.AreEqual(x, s.Subs[x]);

            bool succ4 = s.TryUnify(y, six, out s);
            Assert.AreEqual(true, succ4);
            Assert.AreEqual(six, s.Subs[y]);

            bool succ5 = s.TryUnify(x, y, out s);
            Assert.AreEqual(false, succ5);
            Assert.AreEqual(x, s.Subs[x]);
            Assert.AreEqual(six, s.Subs[y]);
        }
    }
}


