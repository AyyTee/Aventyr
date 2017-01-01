using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EditorLogic;
using EditorLogic.Command;

namespace UnitTest
{
    /// <summary>
    /// Summary description for EditorCommandTests
    /// </summary>
    [TestClass]
    public class EditorCommandTests
    {
        StateList _stateList;
        EditorScene _scene;

        public void Initialize()
        {
            _stateList = new StateList();
            _scene = new EditorScene();
        }

        [TestMethod]
        public void RenameTest0()
        {
            Initialize();
            EditorObject editorObject = new EditorObject(_scene);
            string oldName = "old name";
            string newName = "new name";

            editorObject.Name = oldName;
            _stateList.Add(new Rename(editorObject, newName));
            Assert.IsTrue(editorObject.Name == newName);

            _stateList.Undo();
            Assert.IsTrue(editorObject.Name == oldName);

            _stateList.Redo();
            Assert.IsTrue(editorObject.Name == newName);
        }
    }
}
