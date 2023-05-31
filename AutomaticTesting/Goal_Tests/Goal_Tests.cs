namespace AutomaticTesting.Goal_Tests
{
    using DunKanren;
    using DunKanren.Goals;

    [TestClass]
    public class Goal_Tests
    {
        [TestInitialize]
        public void Boilerplate()
        {
            IO.DisablePrompting();
        }

        [TestMethod]
        public void Test_Top()
        {
            State s = State.InitialState();
            Goal g = new Top();

            Stream str = g.PursueIn(s);

            Assert.AreEqual(true, str.Any());
            Assert.AreEqual(1, str.Count());
            Assert.AreSame(s, str.First());
        }

        [TestMethod]
        public void Test_Bottom()
        {
            State s = State.InitialState();
            Goal g = new Bottom();

            Stream str = g.PursueIn(s);

            Assert.AreEqual(false, str.Any());
            Assert.AreEqual(0, str.Count());
        }

        [TestMethod]
        public void Test_Not()
        {
            State s = State.InitialState();
            Goal g1 = new Bottom();

            Stream str1 = g1.PursueIn(s);

            Assert.AreEqual(false, str1.Any());
            Assert.AreEqual(0, str1.Count());


            Goal g2 = new Not(g1);

            Stream str2 = g2.PursueIn(s);

            Assert.AreEqual(true, str2.Any());
            Assert.AreEqual(1, str2.Count());
            Assert.AreSame(s, str2.First());
        }
    }
}
