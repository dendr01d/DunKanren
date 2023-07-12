namespace DunKanren.Goals
{
    public static class MathGoals
    {
        public static Goal Unito(Term num)
        {
            return num == Cons.Truct(0);
        }

        public static Goal Succo(Term num, Term numPlusOne)
        {
            return !Unito(numPlusOne) & StdGoals.Conso(Term.NIL, num, numPlusOne);
        }

        public static Goal Addo(Term a, Term b, Term sum)
        {
            return new Disj()
            {
                Goal.AND(Unito(b), a == sum),
                new CallFresh((pB, pS) => new Conj()
                {
                    Succo(pB, b),
                    Succo(pS, sum),
                    Addo(a, pB, pS)
                })
            };
        }

        public static Goal NotAddo(Term a, Term b, Term sum)
        {
            return new Conj()
            {
                Goal.OR(!Unito(b), a != sum),
                new CallFresh((pB, pS) => new Disj()
                {
                    !Succo(pB, b),
                    !Succo(pS, sum),
                    NotAddo(a, pB, sum)
                })
            };
        }

        public static Goal Multiplyo(Term a, Term b, Term product)
        {
            return new Disj()
            {
                new Conj()
                {
                    Unito(b),
                    Unito(product)
                },
                new CallFresh((pB, sub) => new Conj()
                {
                    Succo(pB, b),
                    Addo(a, sub, product),
                    Multiplyo(a, pB, sub),
                })
            };
        }
    }
}
