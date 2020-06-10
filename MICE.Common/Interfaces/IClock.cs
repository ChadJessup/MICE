using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MICE.Common.Interfaces
{
    public interface IClock
    {
        void Reset();
        void Delay();
    }
}
