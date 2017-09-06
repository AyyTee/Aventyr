using Equ;
using Game.Common;
using OpenTK;
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
            Text = text;
            CursorIndex = cursorIndex == null ?
                cursorIndex :
                MathHelper.Clamp((int)cursorIndex, 0, text.Length);
        }

        public override string ToString() => $"\"{Text}\", {CursorIndex}";
    }
}
