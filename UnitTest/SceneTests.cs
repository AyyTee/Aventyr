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
            EditorScene editorScene = EditorLogic.Serializer.Deserialize(@"Data\physicsBug.xml");
            editorScene.ActiveCamera = null;
            Scene scene = LevelExport.Export(editorScene);
            float stepSize = 1 / (float)60;
            while (scene.Time < 0.74)
            {
                scene.Step(stepSize);
                //Get the falling rectangle actor.
                Actor actor = scene.SceneNodes.OfType<Actor>().Where(item => item.Body.IsStatic == false).First();
                Assert.IsTrue(BodyExt.GetUserData(actor.Body).GetPortalCollisions().Count == 1);
            }
            scene.Step(stepSize);
        }

        [TestMethod]
        public void StepTest1()
        {
            EditorScene editorScene = EditorLogic.Serializer.Deserialize(@"Data\cornerFallThrough.xml");
            editorScene.ActiveCamera = null;
            Scene scene = LevelExport.Export(editorScene);
            float stepSize = 1 / (float)60;

            Actor actor = scene.SceneNodes.OfType<Actor>().Where(item => item.Body.IsStatic).First();
            Actor box = scene.SceneNodes.OfType<Actor>().Where(item => !item.Body.IsStatic).First();
            

            Transform2 transformPrevious = box.GetTransform();
            /*transformPrevious.Position += new Vector2(0, 0.1f);
            box.SetTransform(transformPrevious);*/

            /*FixtureExt.GetUserData(actor.Body.FixtureList[0]).ProcessChanges();

            Actor actor0 = new Actor(scene, Vector2Ext.ConvertTo(((PolygonShape)actor.Body.FixtureList[1].Shape).Vertices));
            actor0.Body.IsStatic = true;
            Actor actor1 = new Actor(scene, Vector2Ext.ConvertTo(((PolygonShape)actor.Body.FixtureList[2].Shape).Vertices));
            actor1.Body.IsStatic = true;

            actor.Remove();

            scene.World.ProcessChanges();
            //*/
            scene.Step(stepSize);
            scene.Step(stepSize);
            Transform2 transform = box.GetTransform();
            Assert.IsTrue(Math.Abs(transform.Position.Y - transformPrevious.Position.Y) < 0.5);
        }
    }
}
