using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.IO;
using System.Drawing;

namespace Game
{
    public class Renderer
    {
        public List<Scene> RenderScenes = new List<Scene>();
        private Controller _controller;
        public Renderer(Controller controller)
        {
            _controller = controller;
        }

        public static void Init()
        {
            GL.ClearColor(Color.HotPink);
            GL.ClearStencil(0);
            GL.PointSize(5f);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        public void Render()
        {
            GL.Viewport(0, 0, _controller.Width, _controller.Height);
            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            Controller.Shaders["textured"].EnableVertexAttribArrays();
            Controller.Shaders["default"].EnableVertexAttribArrays();
            float TimeRenderDelta = 0;
            //GL.Disable(EnableCap.StencilTest);
            GL.Enable(EnableCap.DepthTest);
            for (int i = 0; i < RenderScenes.Count(); i++)
            {
                Scene scene = RenderScenes[i];
                Camera camera = scene.ActiveCamera;
                scene.DrawScene(camera.GetViewMatrix(), 0);

                TextWriter console = Console.Out;
                Console.SetOut(Controller.Log);
                scene.DrawPortalAll(scene.PortalList.ToArray(), camera.GetViewMatrix(), camera.Viewpoint, 6, TimeRenderDelta);
                Console.SetOut(console);
                GL.Clear(ClearBufferMask.DepthBufferBit);
            }
            
            //GL.Clear(ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.Blend);
            GL.Disable(EnableCap.DepthTest);

            Controller.Shaders["textured"].DisableVertexAttribArrays();
            Controller.Shaders["default"].DisableVertexAttribArrays();

            GL.Flush();
            _controller.SwapBuffers();
        }
    }
}
