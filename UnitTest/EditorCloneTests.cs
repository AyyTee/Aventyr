using EditorLogic;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;

namespace UnitTest
{
    [TestClass]
    public class EditorCloneTests
    {
        EditorScene _scene;
        EditorScene _clipboard;

        public void Init()
        {
            _scene = new EditorScene();
            _clipboard = new EditorScene();
        }

        /*[TestMethod]
        public void CloneNothing()
        {
            Init();
            EditorClone.Clone(_scene, _clipboard);
            Assert.IsTrue(_clipboard.Children.Count == 0);
            Assert.IsTrue(_clipboard.Scene.SceneNodeList.Count == 1);
            Assert.IsTrue(_scene.Children.Count == 0);
            Assert.IsTrue(_scene.Scene.SceneNodeList.Count == 1);
        }

        [TestMethod]
        public void CloneEditorEntity()
        {
            Init();
            EditorEntity entity = new EditorEntity(_scene, new Entity(_scene.Scene));
            int sceneNodeCount = _scene.Scene.SceneNodeList.Count;
            EditorClone.Clone(_scene, _clipboard);

            Assert.IsTrue(_scene.Children.Count == 1);
            EditorEntity clone = (EditorEntity)_clipboard.Children[0];

            Assert.IsTrue(_scene.Scene.SceneNodeList.Count == sceneNodeCount);

            Assert.IsTrue(_clipboard.Children.Count == 1);
            Assert.IsTrue(clone.Entity.Parent == _clipboard.Scene.Root);
            Assert.IsTrue(clone.Children.Count == 0);
            Assert.IsTrue(clone.Entity.Children.Count == 0);
        }

        [TestMethod]
        public void CloneEditorPortal()
        {
            Init();
            EditorPortal portal = new EditorPortal(_scene);
            int sceneNodeCount = _scene.Scene.SceneNodeList.Count;
            EditorClone.Clone(_scene, _clipboard);

            Assert.IsTrue(_scene.Children.Count == 1);
            EditorPortal clone = (EditorPortal)_clipboard.Children[0];

            Assert.IsTrue(_scene.Scene.SceneNodeList.Count == sceneNodeCount);

            Assert.IsTrue(clone.PortalEntity.Parent == _clipboard.Scene.Root);
            Assert.IsTrue(clone.Children.Count == 0);
            Assert.IsTrue(clone.PortalEntity.Children.Count == 1);
        }*/
    }
}
