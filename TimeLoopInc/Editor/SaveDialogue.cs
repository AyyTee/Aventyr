using Game.Rendering;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ui;
using static Ui.ElementEx;

namespace TimeLoopInc.Editor
{
    public class SaveDialogue : Element, IElement
    {
        bool _isSaving;
        DateTime _saveStart;
        readonly IEditorController _editor;
        readonly TimeSpan _animationLength = TimeSpan.FromSeconds(0.15);
        string _saveName;

        public IEnumerable<IElement> Children { get; }
        public Action<string> OnSave { get; }

        public SaveDialogue(IEditorController editor = null, Action<string> onSave = null)
        {
            _editor = editor;
            var font = _editor.Window.Fonts.Inconsolata;
            OnSave = onSave ?? (_ => { });
            Children = new[]
            {
                new Rectangle(
                    color: _ => new Color4(0, 0, 0, AnimationT() * 0.3f), 
                    hidden: _ => AnimationT() <= 0)
                {
                    new Frame(AlignX(0.5f), FallInOut, ChildrenMaxX(), ChildrenMaxY())
                    {
                        new StackFrame(thickness: _ => 50, isVertical: false, spacing: _ => 20)
                        {
                            new TextBlock(y: AlignY(0.5f), font: _ => font, text: _ => "File Name:"),
                            new TextBox(
                                y: AlignY(0.5f),
                                width: _ => 400,
                                font: _ => font,
                                getText: _ => _saveName,
                                setText: text => _saveName = text,
                                backgroundColor: args => ValidName(((TextBox)args.Self).Text) ? Color4.White : Color4.Red),
                            new Button(width: _ => 100, onClick: Save)
                            {
                                new TextBlock(AlignX(0.5f), AlignY(0.5f),  _ => font, _ => "Save")
                            },
                            new Button(width: _ => 100, onClick: _ => Hide())
                            {
                                new TextBlock(AlignX(0.5f), AlignY(0.5f),  _ => font, _ => "Cancel")
                            }
                        }
                    }
                }
            };
        }

        public SaveDialogue(out SaveDialogue id, IEditorController editor = null, Action<string> onSave = null)
            : this(editor, onSave)
        {
            id = this;
        }

        public void Show()
        {
            if (AnimationT() <= 0)
            {
                _isSaving = true;
                _saveStart = DateTime.UtcNow;
                _saveName = _editor?.LevelName ?? "";
            }
        }

        public void Hide()
        {
            if (AnimationT() >= 0)
            {
                _isSaving = false;
                _saveStart = DateTime.UtcNow;
            }
        }

        bool ValidName(string fileName)
        {
            return Regex.IsMatch(fileName, @"^[\w]+$");
        }

        void Save(ClickArgs args)
        {
            Hide();
            OnSave(_saveName);
        }

        float AnimationT()
        {
            var t = (float)MathHelper.Clamp((DateTime.UtcNow - _saveStart).TotalSeconds / _animationLength.TotalSeconds, 0, 1);
            if (!_isSaving)
            {
                t = 1 - t;
            }
            return t;
        }

        float FallInOut(ElementArgs args)
        {
            var height = args.Self.Height;
            var startValue = -height;
            var endValue = (args.Parent.Height- height) / 2;
            return (endValue - startValue) * AnimationT() + startValue;
        }

        public IEnumerator<IElement> GetEnumerator() => Children.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
