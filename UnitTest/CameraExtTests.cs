using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Game;
using OpenTK;

namespace UnitTest
{
    [TestClass]
    public class CameraExtTests
    {
        private class SimpleCamera2 : ICamera2
        {
            public float Aspect
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public double Fov
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public Vector2 ViewOffset
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public float ZFar
            {
                get
                {
                    return 10000;
                }
            }

            public float ZNear
            {
                get
                {
                    return 0.01f;
                }
            }

            public Matrix4 GetViewMatrix(bool isOrtho = true)
            {
                throw new NotImplementedException();
            }

            public Transform2 GetWorldTransform(bool ignorePortals = false)
            {
                throw new NotImplementedException();
            }

            public Transform2 GetWorldVelocity(bool ignorePortals = false)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void ScreenToWorldTest0()
        {
            Controller.CanvasSize = new System.Drawing.Size(800, 600);

            
        }
    }
}
