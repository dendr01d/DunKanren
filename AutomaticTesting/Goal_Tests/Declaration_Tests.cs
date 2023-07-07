namespace AutomaticTesting.Goal_Tests
{
    using DunKanren;
    using DunKanren.Goals;

    [TestClass]
    public class Declaration_Tests
    {
        [TestInitialize]
        public void Boilerplate()
        {
            IO.DisablePrompting();
        }

        [TestMethod]
        public void Test_Fresh()
        {
            Goal g1 = new Fresh("a", "b", "c");
            Stream str1 = g1.Pursue();
            Assert.AreEqual(true, str1.Any());
            Assert.AreEqual(1, str1.Count());

            State s1 = str1.First();
            Assert.AreEqual(3, s1.Subs.Count);
            Assert.AreEqual(true, s1.Subs.Keys.Any(x => x.Symbol == "a"));
            Assert.AreEqual(true, s1.Subs.Keys.Any(x => x.Symbol == "b"));
            Assert.AreEqual(true, s1.Subs.Keys.Any(x => x.Symbol == "c"));

            Assert.AreEqual(1, s1.RecursionLevel);


            Func<Variable, Variable, Variable, Goal> f = (x, y, z) => new Top();

            Goal g2 = new Fresh(f.Method.GetParameters());
            Stream str2 = g2.Pursue();
            Assert.AreEqual(true, str2.Any());
            Assert.AreEqual(1, str2.Count());

            State s2 = str2.First();
            Assert.AreEqual(3, s2.Subs.Count);
            Assert.AreEqual(true, s2.Subs.Keys.Any(x => x.Symbol == "x"));
            Assert.AreEqual(true, s2.Subs.Keys.Any(x => x.Symbol == "y"));
            Assert.AreEqual(true, s2.Subs.Keys.Any(x => x.Symbol == "z"));

            Assert.AreEqual(1, s2.RecursionLevel);
        }

        //[TestMethod]
        //public void Test_Call()
        //{
        //    Goal g1 = new Top();
        //    Goal g2 = new Call(g1);
        //    Goal g3 = new Call(g1.PursueIn);

        //    State s = State.InitialState();
        //    State s1 = g1.PursueIn(s).First();
        //    State s2 = g2.PursueIn(s).First();
        //    State s3 = g3.PursueIn(s).First();

        //    Assert.AreSame(s, s1);
        //    Assert.AreEqual(0, s1.RecursionLevel);
        //    Assert.AreEqual(1, s2.RecursionLevel);
        //    Assert.AreEqual(1, s3.RecursionLevel);
        //}

        [TestMethod]
        public void Test_CallFresh()
        {
            Goal g1 = new CallFresh((a, b, c) => new Top());
            Stream str1 = g1.Pursue();
            Assert.AreEqual(true, str1.Any());
            Assert.AreEqual(1, str1.Count());

            State s1 = str1.First();
            Assert.AreEqual(true, s1.Subs.Keys.Any(x => x.Symbol == "a"));
            Assert.AreEqual(true, s1.Subs.Keys.Any(x => x.Symbol == "b"));
            Assert.AreEqual(true, s1.Subs.Keys.Any(x => x.Symbol == "c"));

            Assert.AreEqual(1, s1.RecursionLevel);


            Goal g2 = new CallFresh((x, y, z) => new Bottom());
            Stream str2 = g2.Pursue();
            Assert.AreEqual(false, str2.Any());
            Assert.AreEqual(0, str2.Count());
        }

        [TestMethod]
        public void Test_CallFresh_IntoGoal()
        {
            Goal g1 = new CallFresh((x, y) => new Conj()
            {
                x == 5,
                y == 6
            });

            Stream str1 = g1.Pursue();
            Assert.AreEqual(true, str1.Any());
            Assert.AreEqual(1, str1.Count());

            State s1 = str1.First();
            Assert.AreEqual(true, s1.Subs.Keys.Any(x => x.Symbol == "x"));
            Assert.AreEqual(true, s1.Subs.Keys.Any(x => x.Symbol == "y"));

            Assert.AreEqual(true, s1.LookupBySymbol("x")?.ToString() == "5");
            Assert.AreEqual(true, s1.LookupBySymbol("y")?.ToString() == "6");
        }
    }
}
