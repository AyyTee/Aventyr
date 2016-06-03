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
        StateList stateList;
        EditorScene scene;

        public void Initialize()
        {
            StateList stateList = new StateList();
            EditorScene scene = new EditorScene();
        }

        [TestMethod]
        public void RenameTest0()
        {
            Initialize();
            EditorObject editorObject = new EditorObject(scene);
            string oldName = "old name";
            string newName = "new name";

            editorObject.Name = oldName;
            stateList.Add(new Rename(editorObject, newName));
            Assert.IsTrue(editorObject.Name == newName);

            stateList.Undo();
            Assert.IsTrue(editorObject.Name == oldName);

            stateList.Redo();
            Assert.IsTrue(editorObject.Name == newName);
        }
    }
}
