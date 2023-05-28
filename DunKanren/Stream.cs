using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren
{
    public class Stream : IEnumerable<State>
    {
        private IEnumerable<State> _Stream { get; set; }

        private Stream(IEnumerable<State> stream)
        {
            this._Stream = stream;
        }

        private Stream(State s) : this(new State[] { s }) { }

        private Stream() : this(Array.Empty<State>()) { }


        public static Stream New(IEnumerable<State> stream) => new Stream(stream);
        public static Stream New(Stream stream) => stream;
        public static Stream Singleton(State? s) => s is null ? Empty() : new Stream(s);
        public static Stream Empty() => new Stream();

        public static Stream Interleave(Stream s1, Stream s2) => new Stream(InterleaveStreams(s1, s2));


        private static IEnumerable<State> InterleaveStreams(Stream s1, Stream s2)
        {
            using IEnumerator<State> iter1 = s1.GetEnumerator();
            using IEnumerator<State> iter2 = s2.GetEnumerator();

            while (true)
            {
                if (iter1.MoveNext()) yield return iter1.Current; else break;
                if (iter2.MoveNext()) yield return iter2.Current; else break;
            }

            while (iter1.MoveNext()) yield return iter1.Current;
            while (iter2.MoveNext()) yield return iter2.Current;
        }


        public IEnumerator<State> GetEnumerator() => this._Stream.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this._Stream.GetEnumerator();
    }
}
