using System;
using NUnit.Framework;
using Game;
using Game.Models;
using OpenTK;
using OpenTK.Graphics;

namespace GameTests
{
    [TestFixture]
    public class VertexTests
    {
        #region Equal tests
        [Test]
        public void EqualTest0()
        {
            Vertex v0 = new Vertex();
            Vertex v1 = new Vertex();

            Assert.IsTrue(v0.Equals(v1));
            Assert.IsTrue(v1.Equals(v0));

            Assert.IsTrue(v0.Equals((object)v1));
            Assert.IsTrue(v1.Equals((object)v0));

            Assert.IsTrue(v0 == v1);
            Assert.IsFalse(v0 != v1);
        }

        [Test]
        public void EqualTest1()
        {
            Vertex v0 = new Vertex(new Vector3(2f, 234.4f, 30f), new Vector2(-341f, 234000f), new Color4(9f, 24.4f, 330f, 1f), new Vector3(255f, 2.4f, 10.315f));
            Vertex v1 = new Vertex(new Vector3(2f, 234.4f, 30f), new Vector2(-341f, 234000f), new Color4(9f, 24.4f, 330f, 1f), new Vector3(255f, 2.4f, 10.315f));

            Assert.IsTrue(v0.Equals(v1));
            Assert.IsTrue(v1.Equals(v0));

            Assert.IsTrue(v0.Equals((object)v1));
            Assert.IsTrue(v1.Equals((object)v0));

            Assert.IsTrue(v0 == v1);
            Assert.IsFalse(v0 != v1);
        }

        [Test]
        public void EqualTest2()
        {
            //Position is different
            Vertex v0 = new Vertex(new Vector3(0f, 234.4f, 30f), new Vector2(-341f, 234000f), new Color4(9f, 24.4f, 330f, 1f), new Vector3(255f, 2.4f, 10.315f));
            Vertex v1 = new Vertex(new Vector3(2f, 234.4f, 30f), new Vector2(-341f, 234000f), new Color4(9f, 24.4f, 330f, 1f), new Vector3(255f, 2.4f, 10.315f));

            Assert.IsFalse(v0.Equals(v1));
            Assert.IsFalse(v1.Equals(v0));

            Assert.IsFalse(v0.Equals((object)v1));
            Assert.IsFalse(v1.Equals((object)v0));

            Assert.IsFalse(v0 == v1);
            Assert.IsTrue(v0 != v1);
        }

        [Test]
        public void EqualTest3()
        {
            //Texture coord is different
            Vertex v0 = new Vertex(new Vector3(2f, 234.4f, 30f), new Vector2(-341f, 99000f), new Color4(9f, 24.4f, 330f, 1f), new Vector3(255f, 2.4f, 10.315f));
            Vertex v1 = new Vertex(new Vector3(2f, 234.4f, 30f), new Vector2(-341f, 234000f), new Color4(9f, 24.4f, 330f, 1f), new Vector3(255f, 2.4f, 10.315f));

            Assert.IsFalse(v0.Equals(v1));
            Assert.IsFalse(v1.Equals(v0));

            Assert.IsFalse(v0.Equals((object)v1));
            Assert.IsFalse(v1.Equals((object)v0));

            Assert.IsFalse(v0 == v1);
            Assert.IsTrue(v0 != v1);
        }

        [Test]
        public void EqualTest4()
        {
            //Color is different
            Vertex v0 = new Vertex(new Vector3(2f, 234.4f, 30f), new Vector2(-341f, 234000f), new Color4(9f, 21f, 330f, 1f), new Vector3(255f, 2.4f, 10.315f));
            Vertex v1 = new Vertex(new Vector3(2f, 234.4f, 30f), new Vector2(-341f, 234000f), new Color4(9f, 24.4f, 330f, 1f), new Vector3(255f, 2.4f, 10.315f));

            Assert.IsFalse(v0.Equals(v1));
            Assert.IsFalse(v1.Equals(v0));

            Assert.IsFalse(v0.Equals((object)v1));
            Assert.IsFalse(v1.Equals((object)v0));

            Assert.IsFalse(v0 == v1);
            Assert.IsTrue(v0 != v1);
        }

        [Test]
        public void EqualTest5()
        {
            //Normal is different
            Vertex v0 = new Vertex(new Vector3(2f, 234.4f, 30f), new Vector2(-341f, 234000f), new Color4(9f, 24.4f, 330f, 1f), new Vector3(-123f, 2.4f, 10.315f));
            Vertex v1 = new Vertex(new Vector3(2f, 234.4f, 30f), new Vector2(-341f, 234000f), new Color4(9f, 24.4f, 330f, 1f), new Vector3(255f, 2.4f, 10.315f));

            Assert.IsFalse(v0.Equals(v1));
            Assert.IsFalse(v1.Equals(v0));

            Assert.IsFalse(v0.Equals((object)v1));
            Assert.IsFalse(v1.Equals((object)v0));

            Assert.IsFalse(v0 == v1);
            Assert.IsTrue(v0 != v1);
        }

        [Test]
        public void GetHashCodeTest0()
        {
            Vertex v0 = new Vertex();
            Vertex v1 = new Vertex();

            Assert.IsTrue(v0.GetHashCode() == v1.GetHashCode());
        }
#endregion

        #region GetHashCode tests
        [Test]
        public void GetHashCodeTest1()
        {
            Vertex v0 = new Vertex(new Vector3(2f, 234.4f, 30f), new Vector2(-341f, 234000f), new Color4(9f, 24.4f, 330f, 1f), new Vector3(255f, 2.4f, 10.315f));
            Vertex v1 = new Vertex(new Vector3(2f, 234.4f, 30f), new Vector2(-341f, 234000f), new Color4(9f, 24.4f, 330f, 1f), new Vector3(255f, 2.4f, 10.315f));

            Assert.IsTrue(v0.GetHashCode() == v1.GetHashCode());
        }

        [Test]
        public void GetHashCodeTest2()
        {
            Random random = new Random(13245);
            for (int i = 0; i < 100; i++)
            {
                Vertex[] vertices = GetRandomVertexPair(random);
                Assert.IsTrue(vertices[0].GetHashCode() == vertices[1].GetHashCode());
                Assert.IsTrue(vertices[0].Equals(vertices[1]));
                Assert.IsTrue(vertices[1].Equals(vertices[0]));
            }
        }
        #endregion

        /// <summary>
        /// Randomly creates two vertices that are identical.
        /// </summary>
        /// <param name="random">Random number generator used to create vertices.</param>
        /// <returns>Array containing two vertices.</returns>
        public static Vertex[] GetRandomVertexPair(Random random)
        {
            Vector3 position = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            Color4 color = new Color4((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            Vector3 normal = new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble());
            Vector2 textureCoord = new Vector2((float)random.NextDouble(), (float)random.NextDouble());
            return new Vertex[]
            {
                new Vertex(position, textureCoord, color, normal),
                new Vertex(position, textureCoord, color, normal)
            };
        }
    }
}
