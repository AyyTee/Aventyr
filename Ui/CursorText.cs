using Equ;
using Game.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui
{
    public class CursorText : MemberwiseEquatable<CursorText>
    {
        public string Text { get; }
        public int? CursorIndex { get; }

        public CursorText(string text, int? cursorIndex)
        {
            DebugEx.Assert(cursorIndex == null || (cursorIndex >= 0 && cursorIndex <= text.Length));
            Text = text;
            CursorIndex = cursorIndex;
        }

        public override string ToString() => $"\"{Text}\", {CursorIndex}";
    }
}
