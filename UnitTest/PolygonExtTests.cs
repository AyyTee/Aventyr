using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTK;
using Game;
using System.Linq;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class PolygonExtTests
    {
        [TestMethod]
        public void DecomposeConcaveTest0()
        {
            Vector2[] vertices = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };

            var concaveList = PolygonExt.DecomposeConcave(vertices);
            Assert.IsTrue(concaveList.Count == 1);
            Assert.IsTrue(concaveList[0].SequenceEqual(vertices));
        }

        [TestMethod]
        public void DecomposeConcaveTest1()
        {
            Vector2[] vertices = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
            vertices = vertices.Reverse().ToArray();

            var concaveList = PolygonExt.DecomposeConcave(vertices);
            Assert.IsTrue(concaveList.Count == 1);
            Assert.IsTrue(concaveList[0].SequenceEqual(vertices));
        }

        [TestMethod]
        public void DecomposeConcaveTest2()
        {
            Vector2[] vertices = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0.2f, 0.2f),
                new Vector2(0, 1)
            };

            var concaveList = PolygonExt.DecomposeConcave(vertices);

            Vector2[] convex0 = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0.2f, 0.2f)
            };
            Vector2[] convex1 = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(0.2f, 0.2f),
                new Vector2(0, 1)
            };
            Assert.IsTrue(concaveList.Count == 2);
            Assert.IsTrue(concaveList[0].SequenceEqual(convex0));
            Assert.IsTrue(concaveList[1].SequenceEqual(convex1));
        }

        [TestMethod]
        public void DecomposeConcaveTest3()
        {
            Vector2[] vertices = new Vector2[] {
                new Vector2(1.33376145f, -3.00228786f),
                new Vector2(2.027403f, -3.060203f),
                new Vector2(2.08520651f, -2.34591722f),
                new Vector2(1.66131449f, -1.51580155f),
                new Vector2(1.08327973f, -1.16831112f),
                new Vector2(0.92913723f, 0.08651507f),
                new Vector2(1.54570746f, 0.144430041f),
                new Vector2(2.10447431f, 0.06720996f),
                new Vector2(2.45129514f, -0.5119406f),
                new Vector2(3.02932978f, -0.376805425f),
                new Vector2(2.66324115f, 0.3760903f),
                new Vector2(1.83472478f, 0.6270555f),
                new Vector2(0.9676728f, 0.588445544f),
                new Vector2(0.794262469f, 0.8780209f),
                new Vector2(1.41083276f, 1.148291f),
                new Vector2(1.31449366f, 1.9591018f),
                new Vector2(0.7557268f, 2.59616756f),
                new Vector2(-0.0535217524f, 2.57686234f),
                new Vector2(-0.6315564f, 2.113542f),
                new Vector2(-0.7086276f, 1.36064637f),
                new Vector2(-0.381074667f, 0.7814957f),
                new Vector2(0.119888663f, 0.8008007f),
                new Vector2(0.293299079f, 0.491920352f),
                new Vector2(-0.6893598f, 0.491920352f),
                new Vector2(-1.17105544f, 0.7235807f),
                new Vector2(-1.44080484f, 1.36064637f),
                new Vector2(-1.51787627f, 1.93979681f),
                new Vector2(-1.47934043f, 2.40311718f),
                new Vector2(-1.74909008f, 2.42242241f),
                new Vector2(-1.78762567f, 2.21006727f),
                new Vector2(-1.98030388f, 2.17145681f),
                new Vector2(-2.057375f, 2.306592f),
                new Vector2(-2.307857f, 2.28728724f),
                new Vector2(-2.288589f, 1.97840679f),
                new Vector2(-2.11517859f, 1.59230649f),
                new Vector2(-1.88396466f, 0.935935855f),
                new Vector2(-1.55641186f, 0.7042757f),
                new Vector2(-1.26739442f, 0.3760903f),
                new Vector2(-0.8049668f, 0.0285999775f),
                new Vector2(-0.169128656f, -0.0486201048f),
                new Vector2(-0.07278955f, -0.5698556f),
                new Vector2(-0.111325145f, -1.20692122f),
                new Vector2(-0.496681571f, -1.47719145f),
                new Vector2(-0.8049668f, -1.7088517f),
                new Vector2(-0.882038f, -2.172172f),
                new Vector2(-0.997645f, -2.982983f),
                new Vector2(-0.6315564f, -3.00228786f),
                new Vector2(-0.304003358f, -3.04089785f),
                new Vector2(-0.169128656f, -2.34591722f),
                new Vector2(0.196959972f, -1.76676679f),
                new Vector2(0.6015842f, -1.63163161f),
                new Vector2(0.8713337f, -1.76676679f),
                new Vector2(1.160351f, -2.24939227f),
                new Vector2(1.275958f, -2.57757759f)
            };
            Assert.IsTrue(PolygonExt.IsSimple(vertices));

            var concaveList = PolygonExt.DecomposeConcave(vertices);
            foreach (List<Vector2> concave in concaveList)
            {
                Assert.IsTrue(MathExt.IsConvex(concave));
            }
        }

        [TestMethod]
        public void DecomposeConcaveTest4()
        {
            Vector2[] vertices = new Vector2[] {
                new Vector2(3.70192051f, 0.5707729f),
                new Vector2(3.607961f, -0.034065783f),
                new Vector2(2.85628319f, -0.598581851f),
                new Vector2(2.25225639f, 0.207869753f),
                new Vector2(1.50057864f, 0.812708437f),
                new Vector2(0.03749132f, 0.9067946f),
                new Vector2(-1.03633356f, 0.94711715f),
                new Vector2(-1.45244122f, 0.34227854f),
                new Vector2(-0.9021058f, 0.6648591f),
                new Vector2(-1.22425318f, -0.128151566f),
                new Vector2(-0.418884277f, 0.194429025f),
                new Vector2(0.7489009f, -1.29750645f),
                new Vector2(-0.6202264f, -0.2625602f),
                new Vector2(0.3327937f, -1.35127f),
                new Vector2(1.16500807f, -2.42653871f),
                new Vector2(1.03077984f, -1.915786f),
                new Vector2(1.50057864f, -2.53406549f),
                new Vector2(1.35292768f, -3.60933423f),
                new Vector2(-0.230964661f, -4.415786f),
                new Vector2(1.36635041f, -4.496431f),
                new Vector2(3.13816237f, -3.837829f),
                new Vector2(2.722055f, -2.8566463f),
                new Vector2(1.44688725f, -1.5797646f),
                new Vector2(0.09118271f, 0.5304505f),
                new Vector2(1.21869946f, 0.530450463f),
                new Vector2(2.211988f, -1.40503347f),
                new Vector2(3.94353127f, -1.257184f),
                new Vector2(4.13145065f, 0.0600202233f),
                new Vector2(3.79588032f, 1.71324587f),
                new Vector2(3.09789371f, 2.31808472f),
                new Vector2(1.31265926f, 2.17023516f),
                new Vector2(0.03749132f, 2.07614923f),
                new Vector2(-1.88197136f, 1.56539667f),
                new Vector2(-1.82828f, 2.03582668f),
                new Vector2(-1.13029337f, 2.694429f),
                new Vector2(-0.8484144f, 3.78313851f),
                new Vector2(-1.84170294f, 3.8637836f),
                new Vector2(-2.87525988f, 3.541203f),
                new Vector2(-3.19740725f, 2.882601f),
                new Vector2(-3.291367f, 1.84765482f),
                new Vector2(-3.291367f, 0.570773244f),
                new Vector2(-3.291367f, -1.20342028f),
                new Vector2(-3.19740725f, 0.6783001f),
                new Vector2(-2.861837f, 2.46593428f),
                new Vector2(-2.53968954f, 3.0170095f),
                new Vector2(-2.48599815f, 2.3584075f),
                new Vector2(-2.44572973f, 1.32346129f),
                new Vector2(-2.59338045f, -0.1281515f),
                new Vector2(-2.44572973f, -1.9695493f),
                new Vector2(-1.82828f, -3.09858155f),
                new Vector2(-1.13029337f, -3.36739874f),
                new Vector2(1.03077984f, -2.70879674f),
                new Vector2(0.373062134f, -1.72761393f),
                new Vector2(-0.7544546f, -1.44535589f),
                new Vector2(-1.84170294f, -0.0743881f),
                new Vector2(-1.22425318f, 1.37722468f),
                new Vector2(1.36635041f, 1.37722456f),
                new Vector2(3.33950448f, 0.30195576f)
            };

            Assert.IsTrue(PolygonExt.IsSimple(vertices));

            var concaveList = PolygonExt.DecomposeConcave(vertices);
            foreach (List<Vector2> concave in concaveList)
            {
                Assert.IsTrue(MathExt.IsConvex(concave));
            }
        }
    }
}
