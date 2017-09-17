using Game;
using Game.Common;
using Newtonsoft.Json;
using NUnit.Framework;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeLoopInc;
using TimeLoopInc.Editor;

namespace TimeLoopIncTests
{
    [TestFixture]
    public class SceneRenderTests
    {
        public static TimeLoopInc.Scene LoadLevel(string filename)
        {
            var workingDir = TestContext.CurrentContext.TestDirectory;
            var levelData = File.ReadAllText(Path.Combine(workingDir, "TimeLoopIncTests", "Levels", filename));
            var level = Serializer.Deserialize<SceneBuilder>(levelData);
            return level.CreateScene();
        }

        [Test]
        public void CorrectCameraTransformTest0()
        {
            var scene = LoadLevel("PlayerLeftOfPortal.xml");
            scene = new TimeLoopInc.Scene(
                FloorTool.FloorRectangle(new RectangleI(new Vector2i(-11, -11), new Vector2i(22, 22))),
                scene.Portals,
                scene.GetEntities());

            var window = new FakeVirtualWindow(Config.Resources, () => new Vector2i(1000, 800));

            var sceneRender = new SceneRender(window, scene);
            scene.Step(new MoveInput(GridAngle.Right));

            var layer = sceneRender.Render(0.1);
            var result = layer.Camera.WorldTransform;
            var expected = new Transform2(new Vector2(-3.4f, 3.5f), (float)(2 * Math.PI), 15);

            Assert.IsTrue(expected.AlmostEqual(result));
        }

        [Test]
        public void CorrectCameraTransformTest1()
        {
            var scene = LoadLevel("PlayerLeftOfPortal.xml");
            scene = new TimeLoopInc.Scene(
                FloorTool.FloorRectangle(new RectangleI(new Vector2i(-11, -11), new Vector2i(22, 22))),
                scene.Portals,
                scene.GetEntities());

            var window = new FakeVirtualWindow(Config.Resources, () => new Vector2i(1000, 800));

            var sceneRender = new SceneRender(window, scene);
            scene.Step(new MoveInput(GridAngle.Right));

            var layer = sceneRender.Render(0.9);
            var result = layer.Camera.WorldTransform;
            var expected = new Transform2(new Vector2(-1.6f, 3.5f), 0, 15);

            Assert.IsTrue(expected.AlmostEqual(result));
        }
    }
}
