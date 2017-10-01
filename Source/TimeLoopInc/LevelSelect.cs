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

    public static class LevelSelect
    {
        public static Element GetElements(ElementFunc<float> x)
        {
            var gridColumns = 4;

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
                    arg => Enumerable.Repeat(ColumnWidth(arg, gridColumns), gridColumns), 
                    arg => MoreEnumerable.Repeat(new[] { ColumnWidth(arg, gridColumns) }, 3))
                {
                    //new TextBlock(text: _ => "Aadsfa sdf asdf")
                    new DataTemplate<LevelPreview, Element>(() => levels, data => new TextBlock(text: _ => data.Name))
                }
            };
        }

        static float ColumnWidth(ElementArgs arg, int columns) => arg.Parent.Width * 0.8f / columns;
    }
}
