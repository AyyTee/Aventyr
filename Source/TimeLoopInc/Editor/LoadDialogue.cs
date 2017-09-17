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
using Ui.Elements;
using Ui.Args;
using Game.Serialization;

namespace TimeLoopInc.Editor
{
    public class LoadDialogue : Element
    {
        bool _isLoading;
        DateTime _loadStart;
        readonly IEditorController _editor;
        readonly TimeSpan _animationLength = TimeSpan.FromSeconds(0.15);

        string[] _files = new string[0];

        public IEnumerable<Element> Children { get; }
        public Action<SceneBuilder> OnLoad { get; }

        public string SelectedFile { get; set; }

        public LoadDialogue(IEditorController editor = null, Action<SceneBuilder> onLoad = null)
        {
            _editor = editor;

            OnLoad = onLoad ?? (_ => { });
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
                                new Button(width: _ => 100, onClick: _ => Load(), enabled: _ => SelectedFile != null)
                                {
                                    new TextBlock(AlignX(0.5f), AlignY(0.5f), _ => "Load")
                                },
                                new Button(width: _ => 100, onClick: _ => Hide())
                                {
                                    new TextBlock(AlignX(0.5f), AlignY(0.5f), _ => "Cancel")
                                }
                            },
                            new StackFrame(spacing: _ => 1)
                            {
                                new DataTemplate<string>(
                                    () => _files.ToOrderedSet(),
                                    name => new Radio<string>(
                                        height: ChildrenMaxY(), 
                                        onClick: RadioClick,
                                        target: name, 
                                        getValue: _ => SelectedFile, 
                                        setValue: value => SelectedFile = value)
                                    {
                                        new TextBlock(_ => 5, AlignY(0.5f), _ => name)
                                    })
                            }
                        }
                    }
                }
            };
        }

        public LoadDialogue(out LoadDialogue id, IEditorController editor = null, Action<SceneBuilder> onLoad = null)
            : this(editor, onLoad)
        {
            id = this;
        }

        void Load()
        {
            if (File.Exists(SelectedFile))
            {
                Hide();
                var scene = Serializer.Deserialize<SceneBuilder>(File.ReadAllText(SelectedFile));
                OnLoad(scene);
                _editor.LevelName = Path.GetFileNameWithoutExtension(SelectedFile);
            }
        }

        void RadioClick(ClickArgs args)
        {
            if (args.IsDoubleClick)
            {
                Load();
            }
        }

        public void Show()
        {
            if (AnimationT() <= 0)
            {
                _isLoading = true;
                _loadStart = DateTime.UtcNow;
                SelectedFile = null;
                if (Directory.Exists(_editor.SavePath))
                {
                    _files = Directory.GetFiles(_editor.SavePath, "*", SearchOption.AllDirectories);
                }
            }
        }

        public void Hide()
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

        public override IEnumerator<Element> GetEnumerator() => Children.GetEnumerator();
    }
}
