using Game;
using Game.Portals;
using EditorLogic;
using System;
using NUnit.Framework;
using OpenTK;
using System.Collections.Generic;
using Game.Common;
using Game.Models;
using System.Linq;
using Game.Rendering;

namespace GameTests
{
    [TestFixture]
    public class ClipModelTests
    {
        public Tuple<EditorScene, EditorWall> CreateTestScene()
        {
            EditorPortal portal0, portal1;
            EditorScene scene = new EditorScene(new FakeVirtualWindow());
            EditorWall polygon = new EditorWall(scene, PolygonFactory.CreateRectangle(2, 2));

            portal0 = new EditorPortal(scene);
            portal1 = new EditorPortal(scene);
            portal0.SetTransform(polygon, new PolygonCoord(0, 0.5f));
            portal1.SetTransform(polygon, new PolygonCoord(1, 0.5f));

            return new Tuple<EditorScene, EditorWall>(scene, polygon);
        }
    }
}
