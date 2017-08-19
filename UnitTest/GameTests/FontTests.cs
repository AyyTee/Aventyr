using Game.Common;
using Game.Rendering;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameTests
{
    [TestFixture]
    public class FontTests
    {
        public static (Font, FontFile) GetFont()
        {
            var workingDir = TestContext.CurrentContext.TestDirectory;
            var fontFile = FontLoader.Load(Path.Combine(workingDir, "Inconsolata.fnt"));
            var id = 0;
            var textures = Enumerable.Repeat(new FakeTexture(ref id, new Vector2i(1024, 1024)), fontFile.Pages.Count);
            return (new Font(fontFile, textures), fontFile);
        }

        [TestCase(0, 0)]
        [TestCase(5, 10)]
        [TestCase(-7, -4)]
        public void SizeTestOneChar(int lineSpacing, int charSpacing)
        {
            var (font, fontFile) = GetFont();

            var result = font.GetSize("Q", new Font.Settings(lineSpacing: lineSpacing, charSpacing: charSpacing));
            var expected = new Vector2i(fontFile.CharLookup['Q'].Width, fontFile.Info.Size);
            Assert.AreEqual(expected, result);
        }

        [TestCase(0, 0)]
        [TestCase(5, 10)]
        [TestCase(-7, -4)]
        public void SizeTestNoChar(int lineSpacing, int charSpacing)
        {
            var (font, fontFile) = GetFont();

            var result = font.GetSize("", new Font.Settings(lineSpacing: lineSpacing, charSpacing: charSpacing));
            var expected = new Vector2i(0, fontFile.Info.Size);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SizeWithEmptyLineBreak()
        {
            var (font, fontFile) = GetFont();

            var result = font.GetSize("\n", new Font.Settings());
            var expected = new Vector2i(0, fontFile.Info.Size * 2);
            Assert.AreEqual(expected, result);
        }

        [TestCase("test", 0, 0)]
        [TestCase("test", 5, 10)]
        [TestCase("test", -7, -4)]
        [TestCase("test\n", 5, 10)]
        [TestCase("test\ny", 5, 10)]
        [TestCase("\ntest\n", 5, 10)]
        public void SizeMatchesModel(string text, int lineSpacing, int charSpacing)
        {
            var (font, fontFile) = GetFont();

            var settings = new Font.Settings(lineSpacing: lineSpacing, charSpacing: charSpacing);
            var result = font.GetSize(text, settings);
            var model = font.GetModel(text, settings);
            // Get the size of the of model.
            var lineCount = text.Split('\n').Length;
            var expected = new Vector2i(
                (int)Math.Round(model.GetVerts().Max(item => item.X)),
                fontFile.Info.Size * lineCount + lineSpacing * (lineCount - 1));
            Assert.AreEqual(expected, result);
        }
    }
}
