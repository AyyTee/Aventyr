using Game;
using Game.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc.Editor
{
    public class ToolData
    {
        public ITool Tool { get; }
        public Hotkey Hotkey { get; }
        public string Name { get; }
        public string Hint { get; }

        public ToolData(string name, ITool tool, string hint, Hotkey hotkey = null)
        {
            DebugEx.Assert(tool != null);
            Name = name;
            Tool = tool;
            Hint = hint;
            Hotkey = hotkey;
        }
    }
}
