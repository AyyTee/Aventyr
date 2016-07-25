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

            public void SetTransform(Transform2 transform)
            {
                Transform = transform.ShallowClone();
            }

            public void SetVelocity(Transform2 velocity)
            {
                Velocity = velocity.ShallowClone();
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

            node.GetWorldTransform();
            Transform2 result = node.GetWorldVelocity();
            Transform2 expected = ApproximateVelocity(node);
            Assert.IsTrue(result.AlmostEqual(expected, 0.1f));
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
        #endregion
    }
}
