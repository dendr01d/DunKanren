namespace AutomaticTesting
{
    using DunKanren;

    [TestClass]
    public class Stream_Tests
    {
        [TestInitialize]
        public void Boilerplate()
        {
            IO.DisablePrompting();
        }

        [TestMethod]
        public void Test_Empty()
        {
            Stream str = Stream.Empty();
            Assert.AreEqual(false, str.Any());
        }

        [TestMethod]
        public void Test_Single()
        {
            State? s1 = null;

            Stream str1 = Stream.Singleton(s1);
            Assert.AreEqual(false, str1.Any());

            State s2 = State.InitialState();

            Stream str2 = Stream.Singleton(s2);
            Assert.AreEqual(1, str2.Count());
            Assert.AreSame(s2, str2.First());
        }

        [TestMethod]
        public void Test_Copy()
        {
            State s = State.InitialState();

            Stream str1 = Stream.Singleton(s);
            Stream str2 = Stream.New(str1);

            Assert.AreSame(str1.First(), str2.First());
            Assert.AreEqual(str1.Count(), str2.Count());
        }

        [TestMethod]
        public void Test_Constructor()
        {
            List<State> states = new()
            {
                State.InitialState(),
                State.InitialState(),
                State.InitialState()
            };

            Stream str1 = Stream.New(states);

            Assert.AreSame(str1.First(), states.First());
            Assert.AreSame(str1.Skip(1).First(), states[1]);
            Assert.AreSame(str1.Skip(2).First(), states[2]);
        }

        [TestMethod]
        public void Test_Interleave()
        {
            List<State> states1 = new()
            {
                State.InitialState(),
                State.InitialState(),
                State.InitialState()
            };

            List<State> states2 = new()
            {
                State.InitialState(),
                State.InitialState(),
                State.InitialState()
            };

            Stream str1 = Stream.New(states1);
            Stream str2 = Stream.New(states2);

            Stream mix = Stream.Interleave(str1, str2);

            Assert.AreSame(mix.Skip(0).First(), states1[0]);
            Assert.AreSame(mix.Skip(1).First(), states2[0]);
            Assert.AreSame(mix.Skip(2).First(), states1[1]);
            Assert.AreSame(mix.Skip(3).First(), states2[1]);
            Assert.AreSame(mix.Skip(4).First(), states1[2]);
            Assert.AreSame(mix.Skip(5).First(), states2[2]);
        }


    }
}
