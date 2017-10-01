using Equ;
using Game;
using Game.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ui;
using Ui.Elements;
using static Ui.ElementEx;
using MoreLinq;
using TimeLoopInc.Editor;

namespace TimeLoopInc
{
    public class LevelPreview : MemberwiseEquatable<LevelPreview>
    {
        public string Name { get; }
        [MemberwiseEqualityIgnore]
        public ITexture Thumbnail { get; }

        public LevelPreview(string name, ITexture thumbnail)
        {
            Name = name;
            Thumbnail = thumbnail;
        }
    }

    public static class Menus
    {
        public static Element LevelSelect(ElementFunc<float> x)
        {
            const int gridColumns = 4;

            Func<ElementArgs, float> columnWidth = arg => arg.Parent.Width * 0.8f / gridColumns;

            var levels = new OrderedSet<LevelPreview>(new[]
            {
                new LevelPreview("Introduction", null),
                new LevelPreview("Level 1", null),
                new LevelPreview("Level 2", null),
                new LevelPreview("Level 3", null),
                new LevelPreview("Level 4", null),
                new LevelPreview("Level 5", null),
                new LevelPreview("Level 6", null),
            });

            return new Frame(x)
            {
                new TextBlock(AlignX(0.5f), _ => 10, _ => "Level Select"),
                new Grid(
                    AlignX(0.5f), 
                    AlignY(0.5f), 
                    arg => Enumerable.Repeat(columnWidth(arg), gridColumns), 
                    arg => MoreEnumerable.Repeat(new[] { columnWidth(arg) }, 3),
                    _ => 5,
                    _ => 5)
                {
                    //new TextBlock(text: _ => "Aadsfa sdf asdf")
                    new DataTemplate<LevelPreview, Element>(
                        () => levels, 
                        data => new Button()
                        {
                            new TextBlock(text: _ => data.Name)
                        })
                }
            };
        }

        public static Element LevelEditor(
            EditorController controller,
            ElementFunc<float> x = null,
            ElementFunc<float> y = null,
            ElementFunc<float> width = null,
            ElementFunc<float> height = null,
            ElementFunc<bool> hidden = null)
        {
            var _controller = controller;

            var menuButtons = new Style
            {
                new StyleElement<Button, float>(nameof(Element.Height), _ => 60),
            };

            var centerText = new Style
            {
                new StyleElement<TextBlock, float>(nameof(Element.X), args => AlignX(0.5f)(args)),
                new StyleElement<TextBlock, float>(nameof(Element.Y), args => AlignY(0.5f)(args))
            };

            SaveDialogue saveDialogue;
            LoadDialogue loadDialogue;
            return new Frame(x, y, width, height, hidden)
            {
                (saveDialogue = new SaveDialogue(_controller, _controller.Save)),
                (loadDialogue = new LoadDialogue(_controller, _controller.Load)),
                new Frame(hidden: _ => _controller._isPlaying)
                {
                    new StackFrame(spacing: _ => 20)
                    {
                        new StackFrame(thickness: _ => 200, spacing: _ => 5, style: menuButtons.With(centerText))
                        {
                            new Button(onClick: _controller.Controller.SetMenuState(MenuState.Main))
                            {
                                new TextBlock(text: _ => "Back to Menu")
                            },
                            new Button(onClick: _ => _controller.NewLevel())
                            {
                                new TextBlock(text: _ => "New")
                            },
                            new Button(onClick: _ => saveDialogue.Show())
                            {
                                new TextBlock(text: _ => "Save As...")
                            },
                            new Button(onClick: _ => loadDialogue.Show())
                            {
                                new TextBlock(text: _ => "Load")
                            },
                            new Button(onClick: _ => _controller.Play(), enabled: _ => _controller.CanStart())
                            {
                                new TextBlock(text: _ => "Play")
                            }
                        },
                        new StackFrame(thickness: _ => 120, spacing: _ => 2, style: centerText)
                        {
                            new DataTemplate<ToolData, Element>(
                                () => new OrderedSet<ToolData>(_controller.Tools),
                                data => new Radio<ToolData>(
                                    height: ChildrenMaxY(),
                                    target: data,
                                    getValue: _ => _controller.ToolCurrent,
                                    setValue: _ => _controller.ToolCurrent = data)
                                {
                                    new TextBlock(
                                        text: _ => data.Name,
                                        maxWidth: args => (int)args.Parent.Width - 10,
                                        textAlignment: _ => 0.5f)
                                })
                        }
                    },
                    new TextBox(
                        _ => 220, _ => 10,
                        _ => 200, _ => 90,
                        _controller.TimeOffsetGetText,
                        _controller.TimeOffsetSetText),
                    new TextBlock(x: _ => 5, y: AlignY(1), text: _ => _controller.ToolCurrent.Hint)
                },
                new StackFrame(thickness: _ => 90, spacing: _ => 5, hidden: _ => !_controller._isPlaying, isVertical: false, style: centerText)
                {
                    new Button(width: _ => 200, onClick: _ => _controller._sceneController = null)
                    {
                        new TextBlock(text: _ => "Return to editor")
                    },
                    new Button(width: _ => 200, onClick: _ => _controller._sceneController.SetInput(_controller._sceneController.Input.Clear()))
                    {
                        new TextBlock(text: _ => "Restart")
                    }
                },
                new TextBlock(AlignX(1), _ => 0, _ => _controller.LevelName + (_controller.SaveChangeCurrent == _controller.SceneChangeCurrent ? "" : "*")),
            };
        }
    }
}
