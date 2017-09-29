using Game;
using Game.Rendering;
using Game.Serialization;
using NUnit.Framework;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using System.Drawing;
using System.Drawing.Imaging;
using TimeLoopInc;
using Game.Models;
using System.Collections.Immutable;
using UnitTest;

namespace GameTests
{
    [TestFixture]
    public class RendererTests
    {
        Resources _resources;
        Layer _layer;
        Func<Vector2i> _clientSizeFunc;
        Renderer _renderer;
        FakeVirtualWindow _virtualWindow;

        [SetUp]
        public void SetUp()
        {
            var size = new Vector2i(400, 300);
            var _window = ResourceController.GetWindow(size, "");

            _window.ClientSize = new Size(size.X, size.Y);

            var resourceFolder = Path.Combine(
                TestContext.CurrentContext.TestDirectory,
                Resources.ResourcePath);

            _resources = Serializer.Deserialize<Resources>(
                File.ReadAllText(Path.Combine(resourceFolder, "Assets.json")));

            _clientSizeFunc = () => (Vector2i)_window.ClientSize;

            _renderer = new Renderer(_clientSizeFunc, _resources);
            _virtualWindow = new FakeVirtualWindow(_resources, _clientSizeFunc);
            _layer = new Layer
            {
                Camera = new HudCamera2(_clientSizeFunc),
            };
            _virtualWindow.Layers.Add(_layer);
            _renderer.Windows.Add(_virtualWindow);
        }

        public static void BitmapCompare(Bitmap expected, Bitmap result)
        {
            Assert.AreEqual(expected.Size, result.Size, "Bitmap sizes aren't equal.");
            for (int y = 0; y < result.Height; y++)
            {
                for (int x = 0; x < result.Width; x++)
                {
                    if (expected.GetPixel(x, y) != result.GetPixel(x, y))
                    {
                        Assert.Fail($"Bitmaps are not equal at {x},{y}.");
                    }
                }
            }
        }

        public static Bitmap GrabScreenshot(Vector2i clientSize)
        {
            // Read OpenGL buffer into a bitmap so we can iterate the pixels
            var bmp = new Bitmap(clientSize.X, clientSize.Y);
            var data = bmp.LockBits(
                new Rectangle(new Point(), new Size(clientSize.X, clientSize.Y)),
                ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            GL.Flush();
            GL.ReadBuffer(ReadBufferMode.Back);
            GL.ReadPixels(0, 0, clientSize.X, clientSize.Y, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            DebugEx.GlAssert();
            bmp.UnlockBits(data);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);

            return bmp;
        }

        [Test]
        public void HudDrawingTest0()
        {
            var size = _clientSizeFunc();

            _layer.DepthTest = false;
            _layer.Renderables.Add(Draw.Line(new LineF(new Vector2(), (Vector2)size), Color4.DarkOrange, 5f));

            for (int y = 0; y < size.Y; y += 100)
            {
                for (int x = 0; x < size.X; x += 100)
                {
                    var v = new Vector2(x, y);
                    _layer.Renderables.Add(Draw.Rectangle(v, v + Vector2.One, Color4.Green));
                    var text = Draw.Text(
                        _resources.DefaultFont,
                        v,
                        $"{x}\n{y}",
                        new Color4(0.1f, 0.2f, 0.3f, 1));
                    _layer.Renderables.Add(text);
                }
            }

            _renderer.Render();
            var result = GrabScreenshot(_clientSizeFunc());
            var expected = new Bitmap(Image.FromFile(Path.Combine(Paths.Data, "HudDrawingTest0.png")));
            BitmapCompare(expected, result);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void DepthTest0(bool drawRedFirst)
        {
            _layer.Camera = new GridCamera(
                new Transform2(size: 15),
                (float)_clientSizeFunc().XRatio);

            var output = new List<IRenderable>();

            Action drawRed = () =>
            {
                var redSquare = SceneRender.CreateSquare(new Vector2i(), 2, Color.Red);
                redSquare.Models[0].Transform.Position = new Vector3(0, 0, 0.0007f);
                output.Add(redSquare);
            };

            Action drawWhite = () =>
            {
                var whiteSquare = new Model(ModelFactory.CreatePlaneMesh(new Vector2(), Vector2.One));
                output.Add(new Renderable(
                    new Transform2(),
                    new[] { whiteSquare }.ToList()));
            };

            if (drawRedFirst)
            {
                drawRed();
                drawWhite();
            }
            else
            {
                drawWhite();
                drawRed();
            }

            _layer.Renderables.AddRange(output);

            _layer.Camera.GetViewMatrix(false);

            _renderer.Render();
            var bitmap = GrabScreenshot(_clientSizeFunc());

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    if (pixel != Color4.Red && pixel != Renderer.BackgroundColor)
                    {
                        Assert.Fail($"White square is visible at {x},{y}.");
                    }
                }
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void OrthographicTransparencyCorrectOrdering0(bool mirrorX)
        {
            _layer.Camera = new GridCamera(
                new Transform2(size: 30, mirrorX: mirrorX),
                (float)_clientSizeFunc().XRatio);

            var mesh = new Mesh();
            var transparentColor = new Color4(1, 1, 1, 0.5f);
            var blackColor = new Color4(0, 0, 0, 1f);
            ModelFactory.AddCircle(mesh, new Vector3(15, 10, 0), 4f, 5, blackColor);
            ModelFactory.AddCircle(mesh, new Vector3(15, 10, 0.1f), 2f, 5, transparentColor);
            _layer.Renderables.Add(new Renderable(new Model(mesh)));

            _renderer.Render();
            var bitmap = GrabScreenshot(_clientSizeFunc());

            var blendColor = new Color4(255, 180, 217, 255);
            blendColor.A = 1;
            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    if (pixel != blendColor && pixel != blackColor && pixel != Renderer.BackgroundColor)
                    {
                        Assert.Fail($"Black shape visible under white shape at {x},{y}.");
                    }
                }
            }
        }

        [Test]
        public void DrawBoxTest()
        {
            _layer.Camera = new SimpleCamera2 { Aspect = (float)_clientSizeFunc().XRatio, WorldTransform = new Transform2(mirrorX: true) };

            var box = new Renderable(
                new Transform2(),
                ModelResources.Box(_resources).Load(_virtualWindow).Models
                    .Select(item =>
                    {
                        var clone = item.ShallowClone();
                        clone.Mesh = new ReadOnlyMesh(
                            clone.Mesh
                                .GetVertices()
                                .Select(v => v.With(color: new Color4(1, 1, 1, 0.5f)))
                                .ToImmutableArray(),
                            clone.Mesh
                                .GetIndices()
                                .ToImmutableArray());
                        return clone;
                    })
                    .ToList());

            _layer.Renderables.Add(box);

            _renderer.Render();

            var bitmap = GrabScreenshot(_clientSizeFunc());
        }

        public Bitmap RenderData(string filename)
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Data", filename);
            var data = File.ReadAllText(path);
            var frame = Serializer.Deserialize<SimpleRenderWindow>(data);

            ResourceController.GetWindow(frame.CanvasSize, "");

            _renderer = new Renderer(() => frame.CanvasSize, _resources);
            _renderer.Windows.Add(frame);

            _renderer.Render();

            return GrabScreenshot(frame.CanvasSize);
        }

        [Test]
        public void RenderBoxTest()
        {
            var result = RenderData("LevelEditor.json");
            var expected = new Bitmap(Image.FromFile(Path.Combine(Paths.Data, "LevelEditor.png")));
            BitmapCompare(expected, result);
        }

        [Test]
        public void PerspectivePortalClippingTest()
        {
            var result = RenderData("PerspectiveClipping.json");
            //var expected = new Bitmap(Image.FromFile(Path.Combine(Paths.Data, "LevelEditor.png")));
            //BitmapCompare(expected, result);
        }
    }
}
