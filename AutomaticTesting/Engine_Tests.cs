﻿namespace AutomaticTesting
{
    using DunKanren;
    using DunKanren.Goals;

    [TestClass]
    public class Engine_Tests
    {
        [TestInitialize]
        public void Boilerplate()
        {
            IO.DisablePrompting();
        }

        public static List<State> Solve(Goal test)
        {
            List<State> output = new();
            int counter = 0;

            foreach (State s in test.Pursue())
            {
                Console.WriteLine("---------------------------");
                Console.WriteLine($"         ANSWER #{counter}");
                Console.WriteLine("---------------------------");
                Console.WriteLine(s);
                output.Add(s);
                Console.WriteLine();
            }

            return output;
        }

        [TestMethod, Timeout(2000)]
        public void Test_Basic()
        {
            Solve(new CallFresh((x, y) => new Conj(x == 5, y == 6)));
        }

        [TestMethod, Timeout(2000)]
        public void Test_Intermediate()
        {
            Goal g = new CallFresh((x, y) => new Conj()
            {
                x == 5,
                new Disj()
                {
                    y == x,
                    y == 6
                }
            });

            Solve(g);
        }

        [TestMethod, Timeout(2000)]
        public void Test_Advanced()
        {
            Goal g = new CallFresh((x, y) => new Conj()
            {
                new Disj()
                {
                    x == 5,
                    x == 6
                },
                y == x,
                y != 5
            });

            Solve(g);
        }

        [TestMethod, Timeout(2000)]
        public void Test_DoubleAppendo()
        {
            Goal g = new CallFresh((x, y, z) => new Conj() {
                new CallFresh(w => new Conj()
                {
                    StdGoals.Appendo(x, y, w),
                    StdGoals.Appendo(w, z, "Duncan")
                })
            });

            Solve(g);
        }

        [TestMethod, Timeout(2000)]
        public void Test_Inference()
        {
            Goal g = new CallFresh(x => new Conj()
            {
                new Disj()
                {
                    x == 2,
                    x == 3,
                    x == 4
                },
                !MathGoals.Addo(x, x, 6),
                !MathGoals.Addo(2, 5, 7)
            });

            Solve(g);
        }

        //[TestMethod]
        //[Timeout(2000)]  // Milliseconds
        //public void Test_Appendo_Simple()
        //{
        //    static Goal Appendo(Term a, Term b, Term c)
        //    {
        //        return new Disj()
        //        {
        //            new Conj()
        //            {
        //                a == Term.NIL,
        //                b == c
        //            },
        //            new Conj()
        //            {
        //                //c != Term.NIL,
        //                new CallFresh((first, aRest, cRest) => new Conj()
        //                {
        //                    a == Cons.Truct(first, aRest),
        //                    c == Cons.Truct(first, cRest),
        //                    Appendo(aRest, b, cRest)
        //                })
        //            }
        //        };
        //    }

        //    Goal g = new CallFresh((x, y) => Appendo(x, y, "abc"));

        //    Solve(g);
        //}
    }
}
