using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IName
    {
        // Simple name used to quickly identify objects. Cannot be assumed to be unique or constant.
        string Name { get; }
    }
}
