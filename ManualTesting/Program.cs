using DunKanren.Goals;

namespace DunKanren
{
    internal class Program
    {
        //static Goal Puzzle()
        //{
        //    return new CallFresh((Aramis, Athos, Pathos, Constance) =>
        //        new CallFresh((Aramis_P, Athos_P, Pathos_P, Constance_P, hotel, jardin, estate) =>
        //            new CallFresh((Aramis_A, Athos_A, Pathos_A, Constance_A, musket, duel, rendM, rendF) => new Conj()
        //            {
        //                hotel == ValueFactory.Sym("Hotel Treville"),
        //                jardin == ValueFactory.Sym("Jardin du Luxembourg"),
        //                estate == ValueFactory.Sym("Geroge Villier's Estate"),

        //                musket == ValueFactory.Sym("Cleaning his musket"),
        //                duel == ValueFactory.Sym("Dueling Cardinal Richelieu"),
        //                rendF == ValueFactory.Sym("Meeting with a man"),
        //                rendM == ValueFactory.Sym("Meeting with a woman"),

        //                Aramis == Cons.Truct(Aramis_P, Aramis_A),
        //                Athos == Cons.Truct(Athos_P, Athos_A),
        //                Pathos == Cons.Truct(Pathos_P, Pathos_A),
        //                Constance == Cons.Truct(Constance_P, Constance_A),

        //                new Conj()
        //                {
        //                    StdGoals.Membero(Aramis_P, Cons.Truct(hotel, jardin, estate)),
        //                    StdGoals.Membero(Athos_P, Cons.Truct(hotel, jardin, estate)),
        //                    StdGoals.Membero(Pathos_P, Cons.Truct(hotel, jardin, estate)),
        //                    StdGoals.Membero(Constance_P, Cons.Truct(hotel, jardin, estate))
        //                },

        //                new Conj()
        //                {
        //                    StdGoals.Membero(Aramis_A, Cons.Truct(musket, duel, rendM)),
        //                    StdGoals.Membero(Athos_A, Cons.Truct(musket, duel, rendM)),
        //                    StdGoals.Membero(Pathos_A, Cons.Truct(musket, duel, rendM)),
        //                    Constance_A == rendF
        //                },

        //                new Conj()
        //                {
        //                    Aramis_P != Athos_P,
        //                    Athos_P != Pathos_P,
        //                    Pathos_P != Aramis_P
        //                },

        //                new Conj()
        //                {
        //                    Aramis_A != Athos_A,
        //                    Athos_A != Pathos_A,
        //                    Pathos_A != Aramis_A
        //                },

        //                //setup complete

        //                new Disj()
        //                {
        //                    new Conj()
        //                    {
        //                        Aramis_P != jardin,
        //                        Athos != jardin
        //                    },
        //                    new Disj()
        //                    {
        //                        Aramis == Cons.Truct(hotel, musket),
        //                        Athos == Cons.Truct(hotel, musket),
        //                        Pathos == Cons.Truct(hotel, musket)
        //                    }
        //                },

        //                new Conj()
        //                {
        //                    Pathos_A != duel,
        //                    new Conj()
        //                    {
        //                        Aramis != Cons.Truct(jardin, duel),
        //                        Athos != Cons.Truct(jardin, duel),
        //                        Pathos != Cons.Truct(jardin, duel)
        //                    }
        //                },

        //                new Conj()
        //                {
        //                    Aramis_A != musket,
        //                    Aramis_P != hotel
        //                },

        //                new Disj()
        //                {
        //                    Pathos_A != musket,
        //                    Constance_P != jardin
        //                },

        //                new Conj()
        //                {
        //                    new Disj()
        //                    {
        //                        Pathos_P != estate,
        //                        new Disj()
        //                        {
        //                            Aramis == Cons.Truct(estate, duel),
        //                            Athos == Cons.Truct(estate, duel),
        //                            Pathos == Cons.Truct(estate, duel)
        //                        }
        //                    },
        //                    new Disj()
        //                    {
        //                        new Conj()
        //                        {
        //                            Aramis != Cons.Truct(estate, duel),
        //                            Athos != Cons.Truct(estate, duel),
        //                            Pathos != Cons.Truct(estate, duel)
        //                        },
        //                        Pathos_P == estate
        //                    }
        //                },

        //                Constance_P != hotel
        //            })));
        //}

        //static Goal PuzzleNew()
        //{
        //    return new CallFresh((Aramis, Athos, Pathos, Constance) =>
        //        new CallFresh((Aramis_Doing, Athos_Doing, Pathos_Doing, Constance_Doing, musket, rendezvous, duel) =>
        //            new CallFresh((Aramis_Location, Athos_Location, Pathos_Location, Constance_Location, hotel, jardin, estate) => new Conj()
        //            {
        //                hotel == "Hotel Treville",
        //                jardin == "Jardin du Luxembourg",
        //                estate == "George Villier's estate",

        //                musket == "Cleaning his musket",
        //                rendezvous == "Having a rendezvous",
        //                duel == "Dueling Cardinal Richelieu",

        //                Constance_Doing == rendezvous,

        //                Aramis == Cons.Truct(Aramis_Doing, Aramis_Location),
        //                Athos == Cons.Truct(Athos_Doing, Athos_Location),
        //                Pathos == Cons.Truct(Pathos_Doing, Pathos_Location),
        //                Constance == Cons.Truct(Constance_Doing, Constance_Location),

        //                Bijecto(Cons.TructList(Aramis_Doing, Athos_Doing, Pathos_Doing), Cons.TructList(musket, rendezvous, duel)),
        //                Bijecto(Cons.TructList(Aramis_Location, Athos_Location, Pathos_Location), Cons.TructList(hotel, jardin, estate)),

        //                new Impl
        //                (
        //                    new Disj()
        //                    {
        //                        new Conj(Aramis_Doing == musket, Aramis_Location == hotel),
        //                        new Conj(Athos_Doing == musket, Athos_Location == hotel ),
        //                        new Conj(Pathos_Doing == musket, Pathos_Location == hotel)
        //                    },
        //                    new Disj()
        //                    {
        //                        Aramis_Location == jardin,
        //                        Athos_Location == jardin
        //                    }
        //                ),

        //                Pathos_Doing != duel,

        //                new DNF()
        //                {
        //                    { Aramis_Doing == duel, Aramis_Location == jardin },
        //                    { Athos_Doing == duel, Athos_Location == jardin },
        //                    { Pathos_Doing == duel, Pathos_Location == jardin }
        //                }.Negate(),

        //                Aramis_Doing != musket,
        //                Aramis_Location != hotel,

        //                new Impl(Pathos_Doing == musket, Constance_Location != jardin),
        //                new BImp(
        //                    new DNF()
        //                    {
        //                        { Aramis_Doing == duel, Aramis_Location == estate },
        //                        { Athos_Doing == duel, Athos_Location == estate },
        //                        { Pathos_Doing == duel, Pathos_Location == estate }
        //                    },
        //                    Pathos_Location == estate
        //                ),

        //                Constance_Location != hotel
        //            })));
        //}


        //https://leanprover.github.io/logic_and_proof/propositional_logic.html
        //public static Goal MurderPuzzle()
        //{

        //    return
        //        new CallFresh((Alice, Husband, Brother, Son, Daughter) =>
        //        new CallFresh((BarMan, BarWoman, Killer, Victim, Loner) =>
        //        new Conj()
        //    {
        //            BarMan == ValueFactory.Sym("Man at the bar"),
        //            BarWoman == ValueFactory.Sym("Woman at the bar"),
        //            Killer == ValueFactory.Sym("Killer at the beach"),
        //            Victim == ValueFactory.Sym("Victim at the beach"),
        //            Loner == ValueFactory.Sym("Bystander elsewhere"),

        //            Membero(Alice, Cons.Truct(BarMan, BarWoman, Killer, Victim, Loner)),
        //            Membero(Husband, Cons.Truct(BarMan, BarWoman, Killer, Victim, Loner)),
        //            Membero(Brother, Cons.Truct(BarMan, BarWoman, Killer, Victim, Loner)),
        //            Membero(Son, Cons.Truct(BarMan, BarWoman, Killer, Victim, Loner)),
        //            Membero(Daughter, Cons.Truct(BarMan, BarWoman, Killer, Victim, Loner)),

        //            new Conj()
        //            {
        //                Alice != Husband, Alice != Brother, Alice != Son, Alice != Daughter,
        //                Husband != Brother, Husband != Son, Husband != Daughter,
        //                Brother != Son, Brother != Daughter,
        //                Son != Daughter,
        //            },

        //            //setup complete?

        //            Membero(BarMan, Cons.Truct(Husband, Brother, Son)),
        //            Membero(BarWoman, Cons.Truct(Alice, Daughter)),

        //            new XOR()
        //            {
        //                Loner == Son,
        //                Loner == Daughter
        //            },

        //            new BImp(Alice == BarWoman, new Disj(Husband == Victim, Husband == Killer)),
        //            new BImp(Husband == BarMan, new Disj(Alice == Victim, Alice == Killer)),

        //            Husband != Victim, //the victim's twin must be amongst the 5 people, but the husband can't have one
        //            new Disj(Son != Killer, Daughter != Victim),
        //            new Disj(Son != Victim, Daughter != Killer),
        //            new Disj(Alice != Killer, Brother != Victim),
        //            new Disj(Alice != Victim, Brother != Killer),

        //            //new Disj()
        //            //{
        //            //    new Impl(Alice == Victim, Brother != Killer),
        //            //    new Impl(Brother == Victim, Alice != Killer),
        //            //    new Impl(Son == Victim, Daughter != Killer),
        //            //    new Impl(Daughter == Victim, Son != Killer),
        //            //},

        //            new Impl(Son == Victim, new Conj(Alice != Killer, Husband != Killer)),
        //            new Impl(Daughter == Victim, new Conj(Alice != Killer, Husband != Killer)),
        //            //new Impl(Alice == Victim, new Disj(Son == Killer, Daughter == Killer)),
        //            //new Impl(Husband == Victim, new Disj(Son == Killer, Daughter == Killer)),

        //            //new XOR(Alice == BarMan, Alice == BarWoman, Alice == Killer, Alice == Victim, Alice == Loner),
        //            //new XOR(Husband == BarMan, Husband == BarWoman, Husband == Killer, Husband == Victim, Husband == Loner),
        //            //new XOR(Brother == BarMan, Brother == BarWoman, Brother == Killer, Brother == Victim, Brother == Loner),
        //            //new XOR(Son == BarMan, Son == BarWoman, Son == Killer, Son == Victim, Son == Loner),
        //            //new XOR(Daughter == BarMan, Daughter == BarWoman, Daughter == Killer, Daughter == Victim, Daughter == Loner),


        //    }));
        //}

        static void Main()
        {
            //Goal g = Puzzle();

            Term a = 'a';
            Term b = 'b';
            Term c = 'c';
            Term d = 'd';

            Cons trio = Cons.Truct(a, b, c);

            Goal g = new CallFresh(x => new Conj()
            {
                x == d,
                StdGoals.NotMembero(x, trio)
            });

            //Goal g = new CallFresh((x, y) => new Conj(x == 5, y == 6));

            //Goal g = new CallFresh((x, y) => Appendo(x, y, "abc"));

            //Goal g = new CallFresh((x, y, z, w) => new Conj() {
            //    Appendo(x, y, z),
            //    Appendo(z, w, "abcd")
            //});

            //Goal g = new CallFresh(x => Bijecto("abc", x));

            //Goal g = new CallFresh(x => Sharedo("abc", x));

            //Goal g = new CallFresh(x => Membero(x, Cons.TructList('a', 'b', 'c')));

            //Goal g = new CallFresh(x => NotMembero('b', Cons.TructList('a', x, 'c')));

            //Goal g = new CallFresh(x => Removeo('a', "abcd", x));

            //Goal g = new CallFresh(x => Reverseo(x, "abcd"));

            Console.WriteLine("Sphinx of black quartz! Judge my vow:");
            Console.WriteLine();
            Console.WriteLine(IO.Graft(g.ToTree()));

            int answers = 0;

            using (var iter = g.Pursue().GetEnumerator())
            {
                while (iter.MoveNext())
                {
                    Console.WriteLine("--------------------------------------------------");
                    Console.WriteLine("     Answer #" + (answers + 1).ToString());
                    Console.WriteLine("--------------------------------------------------\n");
                    Console.WriteLine(iter.Current.ToString(1));
                    //Console.WriteLine(String.Join("\n", iter.Current.Subs.Select(x => $"{x.Key}: {x.Value}")));
                    //IO.Prompt(true);
                    //Console.WriteLine();
                    //Console.WriteLine(IO.Graft(g.ToTree()));

                    ++answers;
                    IO.Prompt(true);
                }
            }

            Console.WriteLine("--------------------------------------------------\n");
            //Console.WriteLine(IO.Graft(g.ToTree()));

            if (answers == 0)
            {
                Console.WriteLine("The sphinx has judged your vow and found it wanting...");
            }
            else
            {
                Console.WriteLine("Your vow has been judged, so says the sphinx of black quartz!");
            }
            Console.WriteLine();
            IO.Prompt(true);

            Environment.Exit(0);
        }
    }
}




