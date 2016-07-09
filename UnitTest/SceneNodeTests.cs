using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;

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
        [TestMethod]
        public void GetWorldVelocityTest0()
        {
            Scene scene = new Scene();

        }
        #endregion
    }
}
