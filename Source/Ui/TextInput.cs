using Game.Rendering;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui
{
    public static class TextInput
    {
        public static CursorText Update(IVirtualWindow window, CursorText cursorText)
        {
            return window.KeyString == "" ?
                MoveCursor(window, cursorText) :
                InsertText(window, cursorText, window.KeyString);
        }

        public static CursorText MoveCursor(IVirtualWindow window, CursorText cursorText)
        {
            var controlHeld = window.ButtonDown(KeyBoth.Control);

            var index = (int)cursorText.CursorIndex;
            var text = cursorText.Text;
            if (window.ButtonPress(Key.Left))
            {
                index--;
                if (controlHeld)
                {
                    index = WordStart(text, index);
                }
            }
            else if (window.ButtonPress(Key.Right))
            {
                index++;
                if (controlHeld)
                {
                    index = WordStartNext(text, index);
                }
            }
            return new CursorText(text, MathHelper.Clamp(index, 0, text.Length));
        }

        public static CursorText InsertText(IVirtualWindow window, CursorText cursorText, string newText)
        {
            var controlHeld = window.ButtonDown(KeyBoth.Control);
            var builder = new StringBuilder(cursorText.Text);
            var cursorIndex = (int)cursorText.CursorIndex;
            for (int i = 0; i < newText.Length; i++)
            {
                if (newText[i] == '\b')
                {
                    if (cursorIndex > 0)
                    {
                        if (controlHeld)
                        {
                            var cursorIndexPrev = cursorIndex;
                            cursorIndex = WordStart(builder.ToString(), cursorIndex - 1);
                            builder.Remove(cursorIndex, cursorIndexPrev - cursorIndex);
                        }
                        else
                        {
                            cursorIndex--;
                            builder.Remove(cursorIndex, 1);
                        }
                    }
                    continue;
                }
                builder.Insert(cursorIndex, newText[i]);
                cursorIndex++;
            }
            return new CursorText(builder.ToString(), cursorIndex);
        }

        public static int WordStart(string text, int charIndex)
        {
            while (charIndex > 0)
            {
                if (char.IsLetterOrDigit(text[charIndex]) &&
                    !char.IsLetterOrDigit(text[charIndex - 1]))
                {
                    break;
                }
                charIndex--;
            }
            return charIndex;
        }

        public static int WordStartNext(string text, int charIndex)
        {
            while (charIndex < text.Length)
            {
                if (char.IsLetterOrDigit(text[charIndex]) &&
                    !char.IsLetterOrDigit(text[charIndex - 1]))
                {
                    break;
                }
                charIndex++;
            }
            return charIndex;
        }
    }
}
