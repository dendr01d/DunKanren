using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DunKanren
{
    /// <summary>
    /// An interface that provides capabilities for reasoning about the "uncertainty" of various objects in terms of their grounded values
    /// </summary>
    public interface IGrounded : IComparable<IGrounded>
    {
        public uint Ungroundedness { get; }
    }
}
