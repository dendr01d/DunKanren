namespace AutomaticTesting.Goal_Tests
{
    using DunKanren;
    using DunKanren.Goals;

    [TestClass]
    public class Std_Tests
    {
        [TestInitialize]
        public void Boilerplate()
        {
            IO.DisablePrompting();
        }

        [TestMethod]
        public void Test_Conso()
        {
            Value<int> five = new(5);
            Value<int> six = new(6);
            Cons test = Cons.Truct(five, six);

            Goal g = new CallFresh((x, y) => StdGoals.Conso(x, y, test));

            Stream str = g.Pursue();
            Assert.AreEqual(true, str.Any());
            Assert.AreEqual(1, str.Count());

            State output = str.First();
            Assert.AreEqual(true, output.LookupBySymbol("x")?.TermEquals(output, five) ?? false);
            Assert.AreEqual(true, output.LookupBySymbol("y")?.TermEquals(output, six) ?? false);
        }

        [TestMethod]
        public void Test_Appendo()
        {
            string s1 = "hello ";
            string s2 = "world!";

            Cons string1 = Cons.Truct(s1);
            Cons string2 = Cons.Truct(s2);
            Cons answer = Cons.Truct(s1 + s2);

            Goal g = new CallFresh(x => StdGoals.Appendo(string1, string2, x));

            Stream str = g.Pursue();
            Assert.AreEqual(true, str.Any());
            Assert.AreEqual(1, str.Count());

            State output = str.First();
            Assert.AreEqual(true, output.LookupBySymbol("x")?.TermEquals(output, answer) ?? false);
        }


    }
}
