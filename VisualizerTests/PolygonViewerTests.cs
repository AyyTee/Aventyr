using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CustomDebugVisualizer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTK;
using System.IO;

namespace VisualizerTests
{
    [TestClass]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class PolygonViewerTests
    {
        /// <summary>
        /// This is less of a unit test and more of a way to quickly preview the visualizer.
        /// </summary>
        [TestMethod]
        [Ignore]
        public void ShowVisualizerTest()
        {
            var vectors = new List<Vector2>
            {
                new Vector2(),
                new Vector2(1, 0),
                new Vector2(0, 2)
            };

            PolygonViewer.TestShowVisualizer(vectors);
        }

        [TestMethod]
        public void Vector2ArrayTest()
        {
            var vectors = new[]
            {
                new Vector2(),
                new Vector2(1, 0),
                new Vector2(0, 2)
            };

            PolygonViewer.GetGrid(vectors);
        }

        [TestMethod]
        public void Vector2dArrayTest()
        {
            var vectors = new[]
            {
                new Vector2d(),
                new Vector2d(1, 0),
                new Vector2d(0, 2)
            };

            PolygonViewer.GetGrid(vectors);
        }

        [TestMethod]
        public void Vector2ListTest()
        {
            var vectors = new List<Vector2>
            {
                new Vector2(),
                new Vector2(1, 0),
                new Vector2(0, 2)
            };

            PolygonViewer.GetGrid(vectors);
        }

        [TestMethod]
        public void Vector2dListTest()
        {
            var vectors = new List<Vector2d>
            {
                new Vector2d(),
                new Vector2d(1, 0),
                new Vector2d(0, 2)
            };

            PolygonViewer.GetGrid(vectors);
        }

        [TestMethod]
        public void InvalidTypeTest0()
        {
            try
            {
                PolygonViewer.GetGrid(new object());
                Assert.Fail();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(e, typeof(InvalidDataException));
            }
        }
    }
}
