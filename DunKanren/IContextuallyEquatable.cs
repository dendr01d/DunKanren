using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren
{
    public interface IContextuallyEquatable<T>
        where T : ADT.Term
    {
        bool Equals(T other, State context);
        bool Equals(T t1, T t2, State context)
        {
            return t1.ContextuallyEquals(t2, context);
        }
    }
}
