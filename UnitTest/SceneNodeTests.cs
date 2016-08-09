using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using Game.Portals;
using System.Collections.Generic;
using OpenTK;
using System.Linq;

namespace UnitTest
{
    [TestClass]
    public class SceneNodeTests
    {
        #region SetScene tests
        [TestMethod]
        public void SetSceneTest0()
        {
            Scene source = new Scene();
            Scene destination = new Scene();
            SceneNode node0 = new SceneNode(source);

            node0.SetParent(destination.Root);

            Assert.IsTrue(node0.Scene == destination);
            Assert.IsTrue(node0.Parent == destination.Root);
            Assert.IsTrue(source.Root.Children.Count == 0);
        }

        [TestMethod]
        public void SetSceneTest1()
        {
            Scene source = new Scene();
            Scene destination = new Scene();
            SceneNode node0 = new SceneNode(source);
            SceneNode node1 = new SceneNode(source);
            node0.SetParent(node1);

            node1.SetParent(destination.Root);

            Assert.IsTrue(node0.Scene == destination);
            Assert.IsTrue(node1.Scene == destination);
            Assert.IsTrue(node0.Parent == node1);
            Assert.IsTrue(node1.Parent == destination.Root);
            Assert.IsTrue(source.Root.Children.Count == 0);
        }

        [TestMethod]
        public void SetSceneTest2()
        {
            Scene source = new Scene();
            Scene destination = new Scene();
            SceneNode node0 = new SceneNode(source);
            SceneNode node1 = new SceneNode(source);
            SceneNode node2 = new SceneNode(source);
            SceneNode node3 = new SceneNode(source);
            node0.SetParent(node1);
            node1.SetParent(node2);

            node1.SetParent(destination.Root);

            Assert.IsTrue(node0.Scene == destination);
            Assert.IsTrue(node1.Scene == destination);
            Assert.IsTrue(node2.Scene == source);
            Assert.IsTrue(node3.Scene == source);
            Assert.IsTrue(node2.Children.Count == 0);
        }
        #endregion
        #region GetWorldTransform tests
        [TestMethod]
        public void GetWorldTransformTest0()
        {
            Scene scene = new Scene();
            FloatPortal p0 = new FloatPortal(scene);
            FloatPortal p1 = new FloatPortal(scene);
            FloatPortal p2 = new FloatPortal(scene);
            FloatPortal p3 = new FloatPortal(scene);
            p0.Linked = p1;
            p1.Linked = p0;
            p2.Linked = p3;
            p3.Linked = p2;

            p0.SetTransform(new Transform2(new Vector2(0, 0)));
            p1.SetTransform(new Transform2(new Vector2(10, 0)));
            p2.SetTransform(new Transform2(new Vector2(0, 10)));
            p3.SetTransform(new Transform2(new Vector2(10, 10)));

            //Make sure this doesn't hang.
            p0.GetWorldTransform();
        }

        //[TestMethod]
        public void GetWorldTransformTest1()
        {
            Scene scene = new Scene();

            Wall wall = new Wall(scene);
            wall.Vertices = PolygonFactory.CreateRectangle(4, 4);
            FixturePortal p0 = new FixturePortal(scene, wall, new PolygonCoord(0, 0.5f));
            FixturePortal p1 = new FixturePortal(scene, wall, new PolygonCoord(2, 0.5f));
            FloatPortal p2 = new FloatPortal(scene);
            FloatPortal p3 = new FloatPortal(scene);
            p0.Linked = p1;
            p1.Linked = p0;
            p2.Linked = p3;
            p3.Linked = p2;

            p2.SetTransform(new Transform2(new Vector2(0, 10)));
            p3.SetTransform(new Transform2(new Vector2(10, 10)));

            //Make sure this doesn't hang.
            p0.GetWorldTransform();
        }

        //[TestMethod]
        public void GetWorldTransformTest2()
        {
            Scene scene = new Scene();

            Wall wall0 = new Wall(scene);
            wall0.Vertices = PolygonFactory.CreateRectangle(4, 4);
            FixturePortal p0 = new FixturePortal(scene, wall0, new PolygonCoord(0, 0.5f));
            FloatPortal p1 = new FloatPortal(scene);
            FloatPortal p2 = new FloatPortal(scene);

            Wall wall1 = new Wall(scene);
            wall1.Vertices = PolygonFactory.CreateRectangle(4, 4);
            wall1.SetTransform(new Transform2(new Vector2(-50, 50)));
            FixturePortal p3 = new FixturePortal(scene, wall1, new PolygonCoord(2, 0.5f));
            p0.Linked = p1;
            p1.Linked = p0;
            p2.Linked = p3;
            p3.Linked = p2;

            p1.SetTransform(new Transform2(new Vector2(10, 0)));
            p2.SetTransform(new Transform2(new Vector2(0, 1), 1, (float)Math.PI/2));

            p0.Path.Enter(p2, p0);

            //Make sure this doesn't hang.
            p0.GetWorldTransform();
        }
        #endregion
        #region GetWorldVelocity tests
        class Transformable : SceneNode, IPortalable
        {
            Transform2 Transform = new Transform2();
            Transform2 Velocity = Transform2.CreateVelocity();
            public Action<EnterCallbackData, Transform2, Transform2> EnterPortal { get; set; }
            public bool IsPortalable { get { return true; } }

            public Transformable(Scene scene)
                : base(scene)
            {
            }

            public List<IPortal> GetPortalChildren()
            {
                throw new NotImplementedException();
            }

            public override Transform2 GetTransform()
            {
                return Transform.ShallowClone();
            }

            public override Transform2 GetVelocity()
            {
                return Velocity.ShallowClone();
            }

            public override void SetTransform(Transform2 transform)
            {
                Transform = transform.ShallowClone();
                base.SetTransform(transform);
            }

            public void SetVelocity(Transform2 velocity)
            {
                Velocity = velocity.ShallowClone();
            }
        }

        class Wall : Transformable, IWall
        {
            public IList<Vector2> Vertices { get; set; } = new List<Vector2>();

            public Wall(Scene scene)
                : base(scene)
            {
            }

            public IList<Vector2> GetWorldVertices()
            {
                Vector2[] worldVertices = Vector2Ext.Transform(Vertices, GetWorldTransform().GetMatrix()).ToArray();
                return worldVertices;
            }
        }

        /// <summary>
        /// Returns a simple to calculate approximation of world velocity.  
        /// Has the side effect of moving IPortalable instances slightly.
        /// </summary>
        public Transform2 ApproximateVelocity(SceneNode node)
        {
            Transform2 previous = node.GetWorldTransform();
            float epsilon = 0.00005f;
            foreach (IPortalable p in node.Scene.SceneNodeList.OfType<IPortalable>())
            {
                p.SetTransform(p.GetTransform().Add(p.GetVelocity().Multiply(epsilon)));
            }
            Transform2 current = node.GetWorldTransform();
            return current.Minus(previous).Multiply(1/epsilon);
        }

        public Transform2 GetRandomTransform(Random random)
        {
            return new Transform2(
                    new Vector2((float)random.NextDouble() * 100 - 50, (float)random.NextDouble() * 100 - 50),
                    1 + (float)random.NextDouble(),
                    (float)random.NextDouble() * 2 + 1f,
                    random.NextDouble() > 0.5);
        }

        public Transform2 GetRandomVelocity(Random random)
        {
            return Transform2.CreateVelocity(
                    new Vector2((float)random.NextDouble() * 100 - 50, (float)random.NextDouble() * 100 - 50),
                    (float)random.NextDouble() * 100 - 50,
                    (float)random.NextDouble() - 0.5f);
        }

        /// <summary>
        /// If the node has no parent then its world velocity equals its local velocity.
        /// </summary>
        [TestMethod]
        public void GetWorldVelocityTest0()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetVelocity(Transform2.CreateVelocity(new Vector2(2.3f, -55f), 23.11f, 13.4f));
            Assert.IsTrue(node.GetWorldVelocity() == node.GetVelocity());
        }

        /// <summary>
        /// Changing the node's transform should have no effect on the world velocity.
        /// </summary>
        [TestMethod]
        public void GetWorldVelocityTest1()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(100f, 33f), 13f, 311f));
            node.SetVelocity(Transform2.CreateVelocity(new Vector2(-1.3f, -5f), 3.11f, -14f));
            Assert.IsTrue(node.GetWorldVelocity() == node.GetVelocity());
        }

        /// <summary>
        /// If the node is parented to another node that has default orientation and no velocity 
        /// then the child node's world velocity equals it's local velocity.
        /// </summary>
        [TestMethod]
        public void GetWorldVelocityTest2()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(100f, 33f), 13f, 311f));
            node.SetVelocity(Transform2.CreateVelocity(new Vector2(-1.3f, -5f), 3.11f, -14f));
            
            Transformable parent = new Transformable(scene);
            node.SetParent(parent);

            Assert.IsTrue(node.GetWorldVelocity() == node.GetVelocity());
        }

        /// <summary>
        /// The child nodes angular velocity should equal it's angular velocity plus parent angular 
        /// velocity if everything else is at default.
        /// </summary>
        [TestMethod]
        public void GetWorldVelocityTest3()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2());
            node.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 10f));

            Transformable parent = new Transformable(scene);
            node.SetParent(parent);
            parent.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 5.5f));

            Assert.AreEqual(node.GetWorldVelocity().Rotation, node.GetVelocity().Rotation + parent.GetVelocity().Rotation, 0.00001);
            Assert.AreEqual(node.GetWorldVelocity().Position, Vector2.Zero);
            Assert.AreEqual(node.GetWorldVelocity().Scale, Vector2.Zero);
        }

        [TestMethod]
        public void GetWorldVelocityTest4()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(2, 0)));

            Transformable parent = new Transformable(scene);
            node.SetParent(parent);
            parent.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 5.5f));

            Transform2 worldVelocity = node.GetWorldVelocity();
            Assert.AreEqual(node.GetVelocity().Rotation + parent.GetVelocity().Rotation, worldVelocity.Rotation, 0.00001);
            Assert.AreEqual(new Vector2(0, 2 * 5.5f), worldVelocity.Position);
            Assert.AreEqual(Vector2.Zero, worldVelocity.Scale);
        }

        [TestMethod]
        public void GetWorldVelocityTest5()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(2, 0)));

            Transformable parent = new Transformable(scene);
            node.SetParent(parent);
            parent.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 5.5f));

            Transform2 worldVelocity = node.GetWorldVelocity();
            Assert.AreEqual(node.GetVelocity().Rotation + parent.GetVelocity().Rotation, worldVelocity.Rotation, 0.00001);
            Assert.AreEqual(new Vector2(0, 2 * 5.5f), worldVelocity.Position);
            Assert.AreEqual(Vector2.Zero, worldVelocity.Scale);
        }

        [TestMethod]
        public void GetWorldVelocityTest6()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(2, 0)));

            Transformable parent = new Transformable(scene);
            node.SetParent(parent);
            parent.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 5.5f));

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.05f));
        }

        [TestMethod]
        public void GetWorldVelocityTest7()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(2, 0)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(), 1, -13.3f));
            node.SetParent(parent);
            parent.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 5.5f));

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.05f));
        }

        [TestMethod]
        public void GetWorldVelocityTest8()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(2, 0)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(-1.9f, 3.3f), 1, -13.3f));
            node.SetParent(parent);
            parent.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 5.5f));

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.05f));
        }

        [TestMethod]
        public void GetWorldVelocityTest9()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(2, 0)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(-1.9f, 3.3f), 3.7f, -13.3f));
            node.SetParent(parent);
            parent.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 5.5f));

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.05f));
        }

        [TestMethod]
        public void GetWorldVelocityTest10()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(2, 0)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(-1.9f, 3.3f), -3.7f, -13.3f));
            node.SetParent(parent);
            parent.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 5.5f));

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.05f));
        }

        [TestMethod]
        public void GetWorldVelocityTest11()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(2, 0)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(-1.9f, 3.3f), 3.7f, -13.3f, true));
            node.SetParent(parent);
            parent.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 5.5f));

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.05f));
        }

        [TestMethod]
        public void GetWorldVelocityTest12()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(3, 0)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(Vector2.Zero, 2f));
            node.SetParent(parent);
            parent.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 0f, 1f));

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.05f));
        }

        [TestMethod]
        public void GetWorldVelocityTest13()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(3, 0)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(Vector2.Zero, 2f, -5.3f, true));
            node.SetParent(parent);
            parent.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 0f, 1f));

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.05f));
        }

        [TestMethod]
        public void GetWorldVelocityTest14()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(Vector2.Zero, 24));
            node.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 0, 0));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(Vector2.Zero, 1));
            parent.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 0, 2));
            node.SetParent(parent);

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.05f));
        }

        [TestMethod]
        public void GetWorldVelocityTest15()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(Vector2.Zero, 4));
            node.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 0, 5));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(Vector2.Zero, 2));
            parent.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 0, 4));
            node.SetParent(parent);

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.05f));
        }

        [TestMethod]
        public void GetWorldVelocityTest16()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(0, 0), 1, 1, false));
            node.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 10));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(0, 0), 1, 0, true));
            //parent.SetVelocity(Transform2.CreateVelocity(new Vector2(0,0), 0));
            node.SetParent(parent);

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.05f));
        }

        [TestMethod]
        public void GetWorldVelocityTest17()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(0, 0), 1, 1, false));
            node.SetVelocity(Transform2.CreateVelocity(new Vector2(3, 0), 10));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(0, 0), -1, 2, true));
            parent.SetVelocity(Transform2.CreateVelocity(new Vector2(0,0), 5));
            node.SetParent(parent);

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.05f));
        }

        [TestMethod]
        public void GetWorldVelocityTest18()
        {
            Random random = new Random(0);
            for (int i = 0; i < 50; i++)
            {
                Scene scene = new Scene();
                Transformable node = new Transformable(scene);
                node.SetTransform(GetRandomTransform(random));
                node.SetVelocity(GetRandomVelocity(random));

                Transformable parent = new Transformable(scene);
                parent.SetTransform(GetRandomTransform(random));
                parent.SetVelocity(GetRandomVelocity(random));
                node.SetParent(parent);

                Transform2 result = node.GetWorldVelocity();
                Transform2 expected = ApproximateVelocity(node);
                Assert.IsTrue(result.AlmostEqual(expected, 0.05f, 0.05f));
            }
        }

        [TestMethod]
        public void GetWorldVelocityWithPortalTest0()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(10, 0)));

            Transformable parent = new Transformable(scene);
            node.SetParent(parent);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(4, 0)));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(30, 0)));
            exit.SetVelocity(Transform2.CreateVelocity(new Vector2(2, 0)));

            exit.Linked = enter;
            enter.Linked = exit;

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.05f));
        }

        [TestMethod]
        public void GetWorldVelocityWithPortalTest1()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(10, 0)));
            //node.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 10f));

            Transformable parent = new Transformable(scene);
            //parent.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 5.5f));
            node.SetParent(parent);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(4, 0)));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(30, 0), 3.3f, 5.5f));
            exit.SetVelocity(Transform2.CreateVelocity(new Vector2(2, 0)));

            exit.Linked = enter;
            enter.Linked = exit;

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.05f));
        }

        [TestMethod]
        public void GetWorldVelocityWithPortalTest2()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(10, 0)));
            //node.SetVelocity(Transform2.CreateVelocity(Vector2.Zero, 10f));

            Transformable parent = new Transformable(scene);
            parent.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 1f));
            node.SetParent(parent);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(4, 0)));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(30, 0), 1f, 0f));
            //exit.SetVelocity(Transform2.CreateVelocity(new Vector2(2, 0)));

            exit.Linked = enter;
            enter.Linked = exit;

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.05f));
        }

        [TestMethod]
        public void GetWorldVelocityWithPortalTest3()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(10, 0)));
            node.SetVelocity(Transform2.CreateVelocity(new Vector2(-2.3f, -56), 10f, 0.4f));

            Transformable parent = new Transformable(scene);
            parent.SetVelocity(Transform2.CreateVelocity(new Vector2(1.5f, 5.2f), 1f, 2.3f));
            node.SetParent(parent);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(4, 0)));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(30, 0), 1f, 0f));

            exit.Linked = enter;
            enter.Linked = exit;

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.05f));
        }

        [TestMethod]
        public void GetWorldVelocityWithPortalTest4()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(10, 0)));

            Transformable parent = new Transformable(scene);
            node.SetParent(parent);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(4, 0)));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(100, 0), 1f, 0f));
            exit.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 1f, 0f));

            exit.Linked = enter;
            enter.Linked = exit;

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.05f));
        }

        [TestMethod]
        public void GetWorldVelocityWithPortalTest5()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(10, 0)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(0, 0.45f)));
            node.SetParent(parent);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(4, 0)));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(100, 0), 1f, 0f));
            exit.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 1f, 0f));

            exit.Linked = enter;
            enter.Linked = exit;

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.06f));
        }

        [TestMethod]
        public void GetWorldVelocityWithPortalTest6()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(10, 0)));

            Transformable parent = new Transformable(scene);
            node.SetParent(parent);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(4, 0)));
            enter.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), -1.5f, 0f));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(100, 0), 1f, 0f));
            exit.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 1f, 0f));

            exit.Linked = enter;
            enter.Linked = exit;

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.06f));
        }

        [TestMethod]
        public void GetWorldVelocityWithPortalTest7()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(10, 0)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(0, 0.45f)));
            node.SetParent(parent);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(4, 0)));
            enter.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), -15f, 0f));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(100, 0), 1f, 0f));
            exit.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 10f, 0f));

            exit.Linked = enter;
            enter.Linked = exit;

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.06f));
        }

        [TestMethod]
        public void GetWorldVelocityWithPortalTest8()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(10, 0)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(0, 0f)));
            node.SetParent(parent);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(4, 0), 1f));
            enter.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 0f, 0f));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(100, 0), 2f, 0f));
            exit.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 1f, 0f));

            exit.Linked = enter;
            enter.Linked = exit;

            node.GetWorldTransform();
            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.06f));
        }

        [TestMethod]
        public void GetWorldVelocityWithPortalTest9()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(10, 0)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(0, 0.45f)));
            node.SetParent(parent);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(4, 0), 1f));
            enter.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), -4f, 0f));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(100, 0), 1.5f, 0f));
            exit.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 6f, 0f));

            exit.Linked = enter;
            enter.Linked = exit;

            node.GetWorldTransform();
            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.06f));
        }

        [TestMethod]
        public void GetWorldVelocityWithPortalTest10()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(10, 0)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(0, 0f)));
            node.SetParent(parent);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(4, 0), 1f, (float)Math.PI/4));
            enter.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), -4f, 0f));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(100, 0), 1.5f, 0f));
            exit.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 6f, 0f));

            exit.Linked = enter;
            enter.Linked = exit;

            node.GetWorldTransform();
            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.1f));
        }

        [TestMethod]
        public void GetWorldVelocityWithPortalTest11()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(10, 12)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(1, 1.2f)));
            node.SetParent(parent);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(4, 5), 1f, (float)Math.PI / 4));
            enter.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), -4f, 0f));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(100, 0), 1.5f, 3.5f));
            exit.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 6f, 0f));

            exit.Linked = enter;
            enter.Linked = exit;

            node.GetWorldTransform();
            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.1f));
        }

        [TestMethod]
        public void GetWorldVelocityWithPortalTest12()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(10, 0)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(0, 0)));
            node.SetParent(parent);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(4, 0), 1f, 0));
            enter.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 2f, 0f));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(100, 0), 1f, 0, true));
            exit.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 0f, 0f));

            exit.Linked = enter;
            enter.Linked = exit;

            node.GetWorldTransform();
            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.1f));
        }

        [TestMethod]
        public void GetWorldVelocityWithPortalTest13()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(10, 0)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(0, 0)));
            node.SetParent(parent);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(4, 0), 1f, 0, true));
            enter.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 2f, 0f));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(100, 0), 1f, 0, true));
            exit.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 0f, 0f));

            exit.Linked = enter;
            enter.Linked = exit;

            node.GetWorldTransform();
            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.1f));
        }

        [TestMethod]
        public void GetWorldVelocityWithPortalTest14()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(10, 0)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(0, 0)));
            node.SetParent(parent);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(4, 0), 1.5f, 0, true));
            enter.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 2f, 0f));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(100, 0), -1.2f, 0, true));
            exit.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 3f, 0f));

            exit.Linked = enter;
            enter.Linked = exit;

            node.GetWorldTransform();
            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.1f));
        }

        [TestMethod]
        public void GetWorldVelocityWithPortalTest15()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(10, -0.2f)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(0, 0.45f)));
            node.SetParent(parent);

            FloatPortal enter = new FloatPortal(scene);
            enter.SetTransform(new Transform2(new Vector2(4, 0), 1.5f, 0, true));
            enter.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 2f, 0f));

            FloatPortal exit = new FloatPortal(scene);
            exit.SetTransform(new Transform2(new Vector2(100, 0), -1.2f, 0));
            exit.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 3f, 0f));

            exit.Linked = enter;
            enter.Linked = exit;

            node.GetWorldTransform();
            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.1f));
        }

        [TestMethod]
        public void GetWorldVelocityWithPortalTest16()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(10, 0)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(0, 0)));
            node.SetParent(parent);

            FloatPortal enter0 = new FloatPortal(scene);
            enter0.SetTransform(new Transform2(new Vector2(4, 0), 1f, 0));
            enter0.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 0f, 0f));

            FloatPortal exit0 = new FloatPortal(scene);
            exit0.SetTransform(new Transform2(new Vector2(100, 0), 1f, (float)Math.PI));
            exit0.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 0f, 0f));

            exit0.Linked = enter0;
            enter0.Linked = exit0;

            FloatPortal enter1 = new FloatPortal(scene);
            enter1.SetTransform(new Transform2(new Vector2(104, 0), 1f, 0));
            enter1.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 1f, 0f));

            FloatPortal exit1 = new FloatPortal(scene);
            exit1.SetTransform(new Transform2(new Vector2(50, 30), 1f, 0));
            exit1.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 0f, 0f));

            exit1.Linked = enter1;
            enter1.Linked = exit1;

            node.GetWorldTransform();
            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.1f));
        }

        [TestMethod]
        public void GetWorldVelocityWithPortalTest17()
        {
            Scene scene = new Scene();
            Transformable node = new Transformable(scene);
            node.SetTransform(new Transform2(new Vector2(10, -0.2f)));

            Transformable parent = new Transformable(scene);
            parent.SetTransform(new Transform2(new Vector2(0, 0.4f)));
            parent.SetVelocity(Transform2.CreateVelocity(new Vector2(1f, 5f), 3f, 0.3f));
            node.SetParent(parent);

            FloatPortal enter0 = new FloatPortal(scene);
            enter0.SetTransform(new Transform2(new Vector2(4, 0), 1.2f, 0, true));
            enter0.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 1.9f, 0f));

            FloatPortal exit0 = new FloatPortal(scene);
            exit0.SetTransform(new Transform2(new Vector2(100, 0), 1.5f, (float)Math.PI));
            exit0.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), -1.3f, 0f));

            exit0.Linked = enter0;
            enter0.Linked = exit0;

            FloatPortal enter1 = new FloatPortal(scene);
            enter1.SetTransform(new Transform2(new Vector2(104, 0), 1.3f, -2f));
            enter1.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), 1f, 0f));

            FloatPortal exit1 = new FloatPortal(scene);
            exit1.SetTransform(new Transform2(new Vector2(50, 30), -0.4f, 0.5f, true));
            exit1.SetVelocity(Transform2.CreateVelocity(new Vector2(0, 0), -2f, 0f));

            exit1.Linked = enter1;
            enter1.Linked = exit1;

            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.1f));
        }

        [TestMethod]
        public void GetWorldVelocityFixturePortalTest0()
        {
            Scene scene = new Scene();
            Wall parent = new Wall(scene);
            parent.Vertices = PolygonExt.SetNormals(new Vector2[] {
                new Vector2(0, 0),
                new Vector2(2, 0),
                new Vector2(0, 2)
            });
            parent.SetVelocity(Transform2.CreateVelocity(new Vector2(4.2f, -3.4f), -2.1f, 0.2f));

            FixturePortal childPortal = new FixturePortal(scene, parent, new PolygonCoord(0, 0.5f));

            Transform2 result = childPortal.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(childPortal);
            Assert.IsTrue(result.AlmostEqual(expected, 0.1f));
        }

        [TestMethod]
        public void GetWorldVelocityFixturePortalTest1()
        {
            Scene scene = new Scene();
            Wall parent = new Wall(scene);
            parent.Vertices = PolygonFactory.CreateRectangle(4, 4);
            parent.SetVelocity(Transform2.CreateVelocity(new Vector2(4.2f, -3.4f), -2.1f, 0.2f));

            FixturePortal childPortal = new FixturePortal(scene, parent, new PolygonCoord(0, 0.5f));

            FloatPortal enter = new FloatPortal(scene);
            FloatPortal exit = new FloatPortal(scene);
            enter.Linked = exit;
            exit.Linked = enter;

            enter.SetTransform(new Transform2(new Vector2(0, 1), 1, (float)Math.PI/2));
            exit.SetTransform(new Transform2(new Vector2(100, 0)));

            childPortal.Path.Enter(enter, childPortal);

            Transform2 result = childPortal.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(childPortal);
            Assert.IsTrue(result.AlmostEqual(expected, 0.1f));
        }

        [TestMethod]
        public void GetWorldVelocityFixturePortalTest2()
        {
            Scene scene = new Scene();
            Wall parent = new Wall(scene);
            parent.Vertices = PolygonFactory.CreateRectangle(4, 4);
            parent.SetVelocity(Transform2.CreateVelocity(new Vector2(4.2f, -3.4f), -2.1f, 0.2f));

            FixturePortal childPortal = new FixturePortal(scene, parent, new PolygonCoord(0, 0.5f));
            FloatPortal childExit = new FloatPortal(scene);
            childExit.SetTransform(new Transform2(new Vector2(20, 20)));
            childExit.Linked = childPortal;
            childPortal.Linked = childExit;


            FloatPortal enter = new FloatPortal(scene);
            FloatPortal exit = new FloatPortal(scene);
            enter.Linked = exit;
            exit.Linked = enter;

            enter.SetTransform(new Transform2(new Vector2(0, 1), 1, (float)Math.PI / 2));
            exit.SetTransform(new Transform2(new Vector2(100, 0)));

            childPortal.Path.Enter(enter, childPortal);

            Transform2 result = childPortal.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(childPortal);
            Assert.IsTrue(result.AlmostEqual(expected, 0.1f));
        }
        #endregion

        #region TransformUpdate tests
        [TestMethod]
        public void TransformUpdateTest0()
        {
            Scene scene = new Scene();
            FloatPortal portal = new FloatPortal(scene);

            FloatPortal child = new FloatPortal(scene);
            portal.Linked = child;
            child.Linked = portal;

            child.SetParent(portal);

            Transform2 begin = new Transform2(new Vector2(5, 0));
            child.SetTransform(begin);

            Assert.IsTrue(child.WorldTransformPrevious == begin);

            Transform2 t = new Transform2(new Vector2(1, 1));
            portal.SetTransform(t);

            Assert.IsTrue(child.WorldTransformPrevious.AlmostEqual(begin.Transform(t)));
        }

        [TestMethod]
        public void TransformUpdateTest1()
        {
            Scene scene = new Scene();
            Wall wall = new Wall(scene);
            wall.Vertices = PolygonFactory.CreateRectangle(2, 2);

            FixturePortal child = new FixturePortal(scene, wall, new PolygonCoord(0, 0.5f));

            Transform2 begin = child.GetTransform();
            Assert.IsTrue(child.WorldTransformPrevious == begin);

            child.SetPosition(wall, new PolygonCoord(1, 0.6f));
            Transform2 moved = child.GetTransform();
            Assert.IsTrue(child.WorldTransformPrevious == moved);

            Transform2 t = new Transform2(new Vector2(1, 1));
            wall.SetTransform(t);

            Assert.IsTrue(child.WorldTransformPrevious.AlmostEqual(moved.Transform(t)));
        }

        [TestMethod]
        public void TransformUpdateTest2()
        {
            Scene scene = new Scene();
            Actor actor = new Actor(scene, PolygonFactory.CreateRectangle(2, 2));

            FixturePortal child = new FixturePortal(scene, actor, new PolygonCoord(0, 0.5f));

            Transform2 begin = child.GetTransform();
            Assert.IsTrue(child.WorldTransformPrevious == begin);

            child.SetPosition(actor, new PolygonCoord(1, 0.6f));
            Transform2 moved = child.GetTransform();
            Assert.IsTrue(child.WorldTransformPrevious == moved);

            Transform2 t = new Transform2(new Vector2(1, 1));
            actor.SetTransform(t);

            Assert.IsTrue(child.WorldTransformPrevious.AlmostEqual(moved.Transform(t)));
        }
        #endregion
    }
}
