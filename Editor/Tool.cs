using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public abstract class Tool
    {
        protected InputExt _input { get { return _controller.InputExt; } }

        public ControllerEditor _controller { get; private set; }

        #region constructors
        public Tool()
            : this(null)
        {
        }

        public Tool(ControllerEditor controller)
        {
            _controller = controller;
        }
        #endregion

        public abstract void Update();
        public abstract void Enable();
        public abstract void Disable();
        public abstract Tool Clone();
    }
}
