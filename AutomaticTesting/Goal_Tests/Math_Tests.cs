
namespace AutomaticTesting.Goal_Tests
{
    using DunKanren;
    using DunKanren.Goals;

    [TestClass]
    public  class Math_Tests
    {

        [TestMethod, Timeout(2000)]
        public void Test_Zero()
        {
            Term zero = 0;
            Term notZero = 5;

            Goal g = new CallFresh(x => new Conj()
            {
                Goal.OR(x == zero, x == notZero),
                MathGoals.Unito(x)
            });

            DunKanren.Stream str = g.Pursue();
            Std_Tests.TestGoal(g, zero);
        }

        [TestMethod, Timeout(2000)]
        public void Test_Succo()
        {
            Term zero = 0;
            Term one = 1;

            Goal g1 = new CallFresh(x => MathGoals.Succo(0, x));
            Std_Tests.TestGoal(g1, one);

            Goal g2 = new CallFresh(x => MathGoals.Succo(x, 1));
            Std_Tests.TestGoal(g2, zero);
        }


        [TestMethod, Timeout(2000)]
        public void Test_Addo()
        {
            Std_Tests.TestGoal(new CallFresh(x => MathGoals.Addo(1, 2, x)), 3);
            Std_Tests.TestGoal(new CallFresh(x => MathGoals.Addo(1, x, 3)), 2);
            Std_Tests.TestGoal(new CallFresh(x => MathGoals.Addo(x, 2, 3)), 1);
        }

        [TestMethod, Timeout(2000)]
        public void Test_Multiplyo()
        {
            Std_Tests.TestGoal(new CallFresh(x => MathGoals.Multiplyo(2, 3, x)), 6);
            Std_Tests.TestGoal(new CallFresh(x => MathGoals.Multiplyo(2, x, 6)), 3);
            Std_Tests.TestGoal(new CallFresh(x => MathGoals.Multiplyo(x, 3, 6)), 2);
        }
    }
}
