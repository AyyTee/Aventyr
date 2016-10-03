using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EditorLogic;
using Game;
using System.Linq;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using System.Collections.Generic;
using OpenTK;

namespace UnitTest
{
    [TestClass]
    public class SceneTests
    {
        [TestMethod]
        public void StepTest0()
        {
            EditorScene editorScene = EditorLogic.Serializer.Deserialize(@"Data\cornerFallThrough.xml");
            editorScene.ActiveCamera = null;
            Scene scene = LevelExport.Export(editorScene);
            float stepSize = 1 / (float)60;

            Actor actor = scene.SceneNodes.OfType<Actor>().Where(item => item.Body.IsStatic).First();
            Actor box = scene.SceneNodes.OfType<Actor>().Where(item => !item.Body.IsStatic).First();

            Transform2 transformPrevious = box.GetTransform();

            scene.Step(stepSize);
            scene.Step(stepSize);
            Transform2 transform = box.GetTransform();
            Assert.IsTrue(Math.Abs(transform.Position.Y - transformPrevious.Position.Y) < 0.3);
        }
    }
}
