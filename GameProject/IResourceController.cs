using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public interface IResourceController : IController, IClientSizeProvider
    {
        IInput Input { get; }
    }
}
