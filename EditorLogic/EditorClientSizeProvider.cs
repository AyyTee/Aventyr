using Game.Rendering;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorLogic
{
    public class EditorClientSizeProvider : IClientSizeProvider
    {
        readonly GLControl _glControl;

        public Size ClientSize => _glControl.ClientSize;

        public EditorClientSizeProvider(GLControl glControl)
        {
            _glControl = glControl;
        }
    }
}
