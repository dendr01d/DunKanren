namespace AutomaticTesting.Goal_Tests
{
    using DunKanren;
    using DunKanren.Goals;

    [TestClass]
    public class Std_Tests
    {
        /// <summary>
        /// Given a single-variable <paramref name="query"/> over the term "x", tests to make sure that
        /// the query produces exactly one state where x is bound to the <paramref name="expectedOutput"/>
        /// </summary>
        public static void TestGoal(Goal query, Term expectedOutput)
        {
            Stream str = query.Pursue();
            Assert.AreEqual(true, str.Any());
            Assert.AreEqual(1, str.Count());

            State output = str.First();
            Assert.AreEqual(true, output.LookupBySymbol("x")?.Equals(expectedOutput) ?? false);
        }

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
            Assert.AreEqual(true, output.LookupBySymbol("x")?.Equals(five) ?? false);
            Assert.AreEqual(true, output.LookupBySymbol("y")?.Equals(six) ?? false);
        }

        [TestMethod, Timeout(2000)]
        public void Test_Appendo()
        {
            string s1 = "hello ";
            string s2 = "world!";

            Cons string1 = Cons.Truct(s1);
            Cons string2 = Cons.Truct(s2);
            Cons answer = Cons.Truct(s1 + s2);

            Goal g = new CallFresh(x => StdGoals.Appendo(string1, string2, x));
            Std_Tests.TestGoal(g, answer);
        }


        [TestMethod, Timeout(2000)]
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
                Assert.AreEqual(true, str.Any(x => x.LookupBySymbol("x")?.Equals(t) ?? false));
            }
        }


        [TestMethod, Timeout(2000)]
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

            TestGoal(g2, d);
        }

        [TestMethod, Timeout(2000)]
        public void Test_Removeo()
        {
            string with = "D6u66nca666n";
            string without = with.Replace("6", String.Empty);

            Cons stringWith = Cons.Truct(with);
            Cons stringWithout = Cons.Truct(without);
            Term removeTerm = '6';

            Goal g1 = new CallFresh(x => StdGoals.Removeo(removeTerm, stringWith, x));
            TestGoal(g1, stringWithout);

            Goal g2 = new CallFresh(x => StdGoals.Removeo(x, stringWith, stringWithout));
            TestGoal(g2, removeTerm);
        }

        [TestMethod, Timeout(2000)]
        public void Test_Distincto()
        {
            Term one = '1';
            Term two = '2';
            Term three = '3';
            Term four = '4';

            Goal g = new CallFresh(x => new Conj()
            {
                new Disj()
                {
                    x == two,
                    x == three
                },
                StdGoals.Distincto(Cons.Truct(one, two, x, four))
            });

            TestGoal(g, three);
        }
    }
}
