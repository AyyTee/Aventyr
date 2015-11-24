using Game;
using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelEditor
{
    public class ControllerEditor : Controller
    {
        Scene Level;
        Vector2 MouseDragPos;
        Vector3 CameraDragPos;
        public ControllerEditor(Window window)
            : base(window)
        {
        }

        public ControllerEditor(Size canvasSize, InputExt input)
            : base(canvasSize, input)
        {
        }

        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Level = new Scene();
            renderer.AddScene(Level);

            Model background = Model.CreatePlane();
            background.TextureId = Renderer.Textures["grid.png"];
            background.Transform.Position = new Vector3(0, 0, -10f);
            float size = 100;
            background.Transform.Scale = new Vector3(size, size, size);
            background.TransformUv.Scale = new Vector2(size, size);
            Entity back = new Entity(Level, new Vector2(0f, 0f));
            back.Models.Add(background);

            Camera cam = Camera.CameraOrtho(new Vector3(0, 0, 10f), 10, CanvasSize.Width / (float)CanvasSize.Height);

            Level.ActiveCamera = cam;
        }

        public override void OnRenderFrame(OpenTK.FrameEventArgs e)
        {
            base.OnRenderFrame(e);
        }

        public override void OnUpdateFrame(OpenTK.FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            Camera cam = Level.ActiveCamera;
            
            if (InputExt.MouseDown(MouseButton.Middle))
            {
                if (InputExt.MousePress(MouseButton.Middle))
                {
                    MouseDragPos = cam.ScreenToWorld(InputExt.MousePos);
                    CameraDragPos = cam.Transform.Position;
                }
                Vector3 camPosPrev = cam.Transform.Position; 
                cam.Transform.Position = CameraDragPos;
                Vector2 offset = MouseDragPos - cam.ScreenToWorld(InputExt.MousePos);
                cam.Transform.Position = camPosPrev;
                cam.Transform.Position = CameraDragPos + new Vector3(offset.X, offset.Y, 0);
            }
            else
            {
                cam.Scale = MathHelper.Clamp(cam.Scale / (float)Math.Pow(1.2, InputExt.MouseWheelDelta()), 0.05f, 1000f);
            }
        }

        public override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
        }

        public override void OnResize(EventArgs e, System.Drawing.Size canvasSize)
        {
            base.OnResize(e, canvasSize);
        }
    }
}
