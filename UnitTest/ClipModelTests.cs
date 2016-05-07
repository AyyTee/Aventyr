using Game;
using Editor;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTK;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class ClipModelTests
    {
        public Tuple<EditorScene, EditorWall> CreateTestScene()
        {
            EditorPortal portal0, portal1;
            EditorScene scene = new EditorScene();
            EditorWall polygon = new EditorWall(scene, PolygonFactory.CreateRectangle(2, 2));

            portal0 = new EditorPortal(scene);
            portal1 = new EditorPortal(scene);
            portal0.SetTransform(polygon, new PolygonCoord(0, 0.5f));
            portal1.SetTransform(polygon, new PolygonCoord(1, 0.5f));

            return new Tuple<EditorScene, EditorWall>(scene, polygon);
        }

        /// <summary>
        /// Check that the correct number of ClipModels are created.
        /// </summary>
        [TestMethod]
        public void ClipCount0()
        {
            var testScene = CreateTestScene();

            List<ClipModel> clipmodels = ClipModelCompute.GetClipModels(testScene.Item2, testScene.Item1.GetPortalList(), 0);
            Assert.IsTrue(clipmodels.Count == 0);
        }

        [TestMethod]
        public void ClipCount1()
        {
            var testScene = CreateTestScene();

            List<ClipModel> clipmodels = ClipModelCompute.GetClipModels(testScene.Item2, testScene.Item1.GetPortalList(), 1);
            Assert.IsTrue(clipmodels.Count == 0);
        }
    }
}
