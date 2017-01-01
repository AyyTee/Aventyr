using Game;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Game.Serialization;

namespace UnitTest
{
    [TestClass]
    public class DeepCloneTests
    {
        [TestMethod]
        public void CloneTest0()
        {
            Scene scene = new Scene();
            SceneNode node0 = new SceneNode(scene);

            DeepClone.Clone(node0);

            SceneNode node0Clone = (SceneNode)scene.SceneObjects[1];
            
            Assert.IsTrue(node0Clone.Parent == null);
            Assert.IsTrue(node0Clone.Children.Count == 0);
            Assert.IsTrue(scene.SceneObjects.Count == 2);
        }

        [TestMethod]
        public void CloneTest1()
        {
            Scene scene = new Scene();
            SceneNode node0 = new SceneNode(scene);
            SceneNode node1 = new SceneNode(scene);
            node1.SetParent(node0);

            DeepClone.Clone(node0);

            SceneNode node0Clone = (SceneNode)scene.SceneObjects[2];

            Assert.IsTrue(node0Clone.Parent == null);
            Assert.IsTrue(node0Clone.Children.Count == 1);
            Assert.IsTrue(node0Clone.Children[0].Parent == node0Clone);
            Assert.IsTrue(scene.SceneObjects.Count == 4);
        }
    }
}
