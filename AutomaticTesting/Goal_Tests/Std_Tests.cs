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


        [TestMethod]
        public void Test_Membero()
        {
            Term a = 'a';
            Term b = 'b';
            Term c = 'c';

            Cons list = Cons.Truct(a, b, c);

            Goal g = new CallFresh(x => StdGoals.Membero(x, list));

            Stream str = g.Pursue();
            Assert.AreEqual(true, str.Any());
            Assert.AreEqual(3, str.Count());

            //assert that each of a, b, and c could be answers to the query posed
            foreach (Term t in list)
            {
                Assert.AreEqual(true, str.Any(x => x.LookupBySymbol("x")?.TermEquals(x, t) ?? false));
            }
        }


        [TestMethod]
        public void Test_NotMembero()
        {
            Term a = 'a';
            Term b = 'b';
            Term c = 'c';

            Cons trio = Cons.Truct(a, b, c);

            Goal g = new CallFresh(x => new Conj() {
                new Disj()
                {
                    x == a,
                    x == b,
                    x == c
                },
                StdGoals.NotMembero(x, trio)
            });

            Stream str = g.Pursue();
            Assert.AreEqual(false, str.Any());

            Term d = 'd';

            Goal g2 = new CallFresh(x => new Conj()
            {
                new Disj()
                {
                    x == a,
                    x == b,
                    x == c,
                    x == d
                },
                StdGoals.NotMembero(x, trio)
            });

            Stream str2 = g2.Pursue();
            Assert.AreEqual(true, str2.Any());
            Assert.AreEqual(1, str2.Count());
            State solution = str2.First();
            Assert.AreEqual(true, solution.LookupBySymbol("x").TermEquals(solution, d));
        }
    }
}
