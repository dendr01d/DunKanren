using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren
{
    public interface IContextuallyEquatable
    {
        bool Equals(IContextuallyEquatable other, State context);
        bool Equals(IContextuallyEquatable t1, IContextuallyEquatable t2, State context)
        {
            return t1.Equals(t2, context);
        }
    }

    public interface IEquatableSansContext : IContextuallyEquatable, IEquatable<IEquatableSansContext>
    {
        new bool Equals(IContextuallyEquatable other, State context) => Equals(other);
        new bool Equals(IContextuallyEquatable t1, IContextuallyEquatable t2, State context)
        {
            return t1.Equals(t2);
        }
    }
}
