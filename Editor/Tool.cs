using Game;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public abstract class Tool
    {
        protected InputExt _input { get { return _controller.InputExt; } }
        public bool Enabled { get; private set; }

        public ControllerEditor _controller { get; private set; }

        #region constructors
        public Tool()
            : this(null)
        {
        }

        public Tool(ControllerEditor controller)
        {
            Enabled = false;
            _controller = controller;
        }
        #endregion

        public virtual void Update()
        {
            Debug.Assert(Enabled == true, "Tool has not been enabled.  Call Enable before calling Update.");
        }
        public virtual void Enable()
        {
            Enabled = true;
        }
        public virtual void Disable()
        {
            Enabled = false;
        }
        public abstract Tool Clone();
    }
}
