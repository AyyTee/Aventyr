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
            var expected = new Vector2i(fontFile.CharLookup['Q'].Width, fontFile.Common.LineHeight);
            Assert.AreEqual(expected, result);
        }

        [TestCase(0, 0)]
        [TestCase(5, 10)]
        [TestCase(-7, -4)]
        public void SizeTestNoChar(int lineSpacing, int charSpacing)
        {
            var (font, fontFile) = GetFont();

            var result = font.GetSize("", new Font.Settings(lineSpacing: lineSpacing, charSpacing: charSpacing));
            var expected = new Vector2i(0, fontFile.Common.LineHeight);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SizeWithEmptyLineBreak()
        {
            var (font, fontFile) = GetFont();

            var result = font.GetSize("\n", new Font.Settings());
            var expected = new Vector2i(0, fontFile.Common.LineHeight * 2);
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void SizeWithTextWrap()
        {
            var (font, fontFile) = GetFont();
            var text = "Portal Link";
            var size = font.GetSize("Portal");

            var result = font.GetSize(text, new Font.Settings(maxWidth: size.X));
            var expected = new Vector2i(size.X, fontFile.Common.LineHeight * 2);
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
                fontFile.Common.LineHeight * lineCount + lineSpacing * (lineCount - 1));
            Assert.AreEqual(expected, result);
        }

        public string[] GlyphsToText(Font.Glyph[][] glyphs)
        {
            return glyphs
                .Select(item => string.Join("", item.Select(glyph => glyph.Char)))
                .ToArray();
        }

        #region TextWrap tests
        [Test]
        public void TextWrapTest0()
        {
            var (font, fontFile) = GetFont();

            var text = "ab ab";
            var size = font.GetSize("ab");

            var glyphs = font.GetGlyphs(text, new Font.Settings(maxWidth: size.X + 1));
            var result = GlyphsToText(glyphs);
            var expected = new[] 
            {
                "ab",
                " ",
                "ab"
            };
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TextWrapTest1()
        {
            var (font, fontFile) = GetFont();

            var text = "test test";
            var size = font.GetSize("test");

            var glyphs = font.GetGlyphs(text, new Font.Settings(maxWidth: size.X));
            var result = GlyphsToText(glyphs);
            var expected = new[]
            {
                "test",
                " ",
                "test"
            };
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TextWrapTest2()
        {
            var (font, fontFile) = GetFont();

            var text = "ababab";
            var size = font.GetSize("ab");

            var glyphs = font.GetGlyphs(text, new Font.Settings(maxWidth: size.X + 1));
            var result = GlyphsToText(glyphs);
            var expected = new[]
            {
                "ab",
                "ab",
                "ab"
            };
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TextWrapTest3()
        {
            var (font, fontFile) = GetFont();

            var text = "ababab";
            var size = font.GetSize("ab");

            var glyphs = font.GetGlyphs(text, new Font.Settings(maxWidth: size.X));
            var result = GlyphsToText(glyphs);
            var expected = new[]
            {
                "ab",
                "ab",
                "ab"
            };
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TextWrapTest4()
        {
            var (font, fontFile) = GetFont();

            var text = "test word   w";
            var size = font.GetSize("test word  ");

            var glyphs = font.GetGlyphs(text, new Font.Settings(maxWidth: size.X));
            var result = GlyphsToText(glyphs);
            var expected = new[]
            {
                "test ",
                "word   w"
            };
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TextWrapTest5()
        {
            var (font, fontFile) = GetFont();

            var text = "test word\tw";
            var size = font.GetSize("test word  ");

            var glyphs = font.GetGlyphs(text, new Font.Settings(maxWidth: size.X));
            var result = GlyphsToText(glyphs);
            var expected = new[]
            {
                "test ",
                "word    w"
            };
            Assert.AreEqual(expected, result);
        }

        [TestCase(0)]
        [TestCase(3)]
        public void TextWrapTest6(int maxWidth)
        {
            var (font, fontFile) = GetFont();

            var text = "test word";

            var glyphs = font.GetGlyphs(text, new Font.Settings(maxWidth: maxWidth));
            var result = GlyphsToText(glyphs);
            var expected = new[]
            {
                "t", "e", "s", "t", " ", "w", "o", "r", "d"
            };
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TextWrapTest7()
        {
            var (font, fontFile) = GetFont();

            var text = "test\ntest";
            var size = font.GetSize("test");

            var glyphs = font.GetGlyphs(text, new Font.Settings(maxWidth: size.X));
            var result = GlyphsToText(glyphs);
            var expected = new[]
            {
                "test",
                "test"
            };
            Assert.AreEqual(expected, result);
        }

        [Test]
        public void TextWrapTest8()
        {
            var (font, fontFile) = GetFont();

            var text = "test \ntest";
            var size = font.GetSize("test");

            var glyphs = font.GetGlyphs(text, new Font.Settings(maxWidth: size.X));
            var result = GlyphsToText(glyphs);
            var expected = new[]
            {
                "test",
                " ",
                "test"
            };
            Assert.AreEqual(expected, result);
        }
        #endregion

        [Test]
        public void GetBaselineTest0()
        {
            var (font, fontFile) = GetFont();

            var text = "test";
            var glyphs = font.GetGlyphs(text, new Font.Settings());
            var glyph = glyphs[0][0];

            var result = font.BaselinePosition(text, 0, new Font.Settings());
            var expected = new Vector2i(glyph.Point.X, fontFile.Common.Base);
            Assert.AreEqual(result, expected);
        }

        [Test]
        public void GetBaselineTest1()
        {
            var (font, fontFile) = GetFont();

            var text = "test";
            var glyphs = font.GetGlyphs(text, new Font.Settings());
            var glyph = glyphs[0][3];

            var result = font.BaselinePosition(text, 4, new Font.Settings());
            var expected = new Vector2i(glyph.Point.X + glyph.FontChar.XAdvance, fontFile.Common.Base);
            Assert.AreEqual(result, expected);
        }

        [TestCase(0)]
        [TestCase(2)]
        [TestCase(-3)]
        public void GetBaselineTest2(int lineSpacing)
        {
            var (font, fontFile) = GetFont();

            var text = "test\n";
            var glyphs = font.GetGlyphs(text, new Font.Settings());

            var result = font.BaselinePosition(text, 5, new Font.Settings(lineSpacing: lineSpacing));
            var expected = new Vector2i(0, fontFile.Common.LineHeight + lineSpacing + fontFile.Common.Base);
            Assert.AreEqual(result, expected);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(-1)]
        public void GetBaselineTest3(int charIndex)
        {
            var (font, fontFile) = GetFont();

            var text = "";
            var glyphs = font.GetGlyphs(text, new Font.Settings());

            var result = font.BaselinePosition(text, charIndex, new Font.Settings());
            var expected = new Vector2i(0, fontFile.Common.Base);
            Assert.AreEqual(result, expected);
        }
    }
}
