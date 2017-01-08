using Game;
using Game.Portals;
using EditorLogic;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTK;
using System.Collections.Generic;
using Game.Common;
using Game.Models;

namespace GameTests
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
        /// Portals attached to a square should have no clipping regardless of its orientation and position.
        /// </summary>
        [TestMethod]
        public void FixturePortal()
        {
            EditorPortal portal0, portal1;
            EditorScene scene = new EditorScene();
            EditorWall polygon = new EditorWall(scene, PolygonFactory.CreateRectangle(2, 2));

            portal0 = new EditorPortal(scene);
            portal1 = new EditorPortal(scene);
            portal0.SetTransform(polygon, new PolygonCoord(0, 0.5f));
            portal1.SetTransform(polygon, new PolygonCoord(1, 0.5f));
            portal0.Linked = portal1;
            portal1.Linked = portal0;

            for (float i = 0; i < MathExt.Tau; i += 0.01f)
            {
                polygon.SetTransform(new Transform2(new Vector2(100000, -123), 1, i));
                List<Clip.ClipModel> clipmodels = Clip.GetClipModels(polygon, scene.GetPortalList(), 2);
                Assert.IsTrue(clipmodels.Count == polygon.GetModels().Count);
            }
        }
    }
}
