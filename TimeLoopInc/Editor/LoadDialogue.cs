using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui;
using Game;
using static Ui.ElementEx;

namespace TimeLoopInc.Editor
{
    public class LoadDialogue : Element, IElement
    {
        bool _isLoading;
        DateTime _loadStart;
        readonly IEditorController _editor;
        readonly TimeSpan _animationLength = TimeSpan.FromSeconds(0.15);

        string[] _files = new string[0];

        public IEnumerable<IElement> Children { get; }

        public LoadDialogue(IEditorController editor)
        {
            _editor = editor;
            var font = _editor.Window.Fonts.Inconsolata;

            Children = new[]
            {
                new Rectangle(
                    color: _ => new Color4(0, 0, 0, AnimationT() * 0.3f),
                    hidden: _ => AnimationT() <= 0)
                {
                    new Frame(AlignX(0.5f), FallInOut, ChildrenMaxX(), ChildrenMaxY())
                    {
                        new StackFrame(thickness: ChildrenMaxX(), spacing: _ => 10)
                        {
                            new StackFrame(thickness: _ => 50, isVertical: false, spacing: _ => 20)
                            {
                                new TextBlock(y: AlignY(0.5f), font: _ => font, text: _ => "File Name:"),
                                new Button(width: _ => 100, onClick: Load)
                                {
                                    new TextBlock(AlignX(0.5f), AlignY(0.5f),  _ => font, _ => "Load")
                                },
                                new Button(width: _ => 100, onClick: Hide)
                                {
                                    new TextBlock(AlignX(0.5f), AlignY(0.5f),  _ => font, _ => "Cancel")
                                }
                            },
                            new StackFrame(spacing: _ => 1)
                            {
                                new DataTemplate<string>(
                                    () => _files.ToOrderedSet(),
                                    name => new Button(height: ChildrenMaxY())
                                    {
                                        new TextBlock(_ => 5, AlignY(0.5f), _ => _editor.Window.Fonts.Inconsolata, _ => name)
                                    })
                            }
                        }
                    }
                }
            };
        }

        public LoadDialogue(out LoadDialogue id, IEditorController editor)
            : this(editor)
        {
            id = this;
        }

        void Load(ClickArgs args)
        {

        }

        public void Show()
        {
            if (AnimationT() <= 0)
            {
                _isLoading = true;
                _loadStart = DateTime.UtcNow;
                _files = Directory.GetFiles(_editor.SavePath, "*", SearchOption.AllDirectories);
            }
        }

        public void Hide(ClickArgs args)
        {
            if (AnimationT() >= 0)
            {
                _isLoading = false;
                _loadStart = DateTime.UtcNow;
            }
        }

        float AnimationT()
        {
            var t = (float)MathHelper.Clamp((DateTime.UtcNow - _loadStart).TotalSeconds / _animationLength.TotalSeconds, 0, 1);
            if (!_isLoading)
            {
                t = 1 - t;
            }
            return t;
        }

        float FallInOut(ElementArgs args)
        {
            var height = args.Self.Height;
            var startValue = -height;
            var endValue = (args.Parent.Height - height) / 2;
            return (endValue - startValue) * AnimationT() + startValue;
        }

        public IEnumerator<IElement> GetEnumerator() => Children.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
