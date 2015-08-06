using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Game
{
    class Controller : GameWindow
    {
        public Controller() : base(1024, 768, new GraphicsMode(32, 24, 0, 4), "Game", GameWindowFlags.FixedWindow)
        {
            
        }
        InputExt InputExt;
        Camera cam;
        Vector2 lastMousePos = new Vector2();

        List<Entity> objects = new List<Entity>();
        public static Dictionary<string, int> textures = new Dictionary<string, int>();
        public static Dictionary<string, ShaderProgram> Shaders = new Dictionary<string, ShaderProgram>();

        Matrix4 viewMatrix;

        float Time = 0.0f;
        /// <summary>
        /// The difference in seconds between the last OnUpdateEvent and the current OnRenderEvent.
        /// </summary>
        float TimeRenderDelta = 0.0f;

        void initProgram()
        {
            //GraphicsContext.CurrentContext.SwapInterval = 1;
            InputExt = new InputExt(this);
            lastMousePos = new Vector2(Mouse.X, Mouse.Y);

            // Load shaders from file
            Shaders.Add("default", new ShaderProgram(@"assets\shaders\vs.glsl", @"assets\shaders\fs.glsl", true));
            Shaders.Add("textured", new ShaderProgram(@"assets\shaders\vs_tex.glsl", @"assets\shaders\fs_tex.glsl", true));

            // Load textures from file
            textures.Add("opentksquare.png", loadImage(@"assets\opentksquare.png"));
            textures.Add("opentksquare2.png", loadImage(@"assets\opentksquare2.png"));
            textures.Add("grid.png", loadImage(@"assets\grid.png"));
            // Create our objects

            Portal portal = new Portal(true);
            portal.Transform.Rotation = new Quaternion(0, 1.5f, 1, 0);
            objects.Add(portal);

            /*Cube cube = new Cube(Shaders["default"]);
            Entity e = new Entity(new Vector3(-1f, 0, 0));
            e.Transform.Rotation = new Quaternion(0, 0, 0, 1f);
            e.Models.Add(cube);
            objects.Add(e);*/

            TexturedCube tc = new TexturedCube(Shaders["textured"]);
            tc.Transform.Position = new Vector3(1f, 3f, 0);
            tc.TextureID = textures["opentksquare.png"];
            Entity box = new Entity(new Vector3(0,0,0));
            box.Models.Add(tc);
            objects.Add(box);

            Plane background = new Plane(Shaders["textured"]);
            background.TextureID = textures["grid.png"];
            background.Transform.Scale = new Vector3(10f, 10f, 10f);
            background.TextureScale = 10;
            Entity back = new Entity(new Vector3(0f, 0f, 0f));
            back.Models.Add(background);
            objects.Add(back);

            cam = Camera.CameraOrtho(new Vector3(0f, 0f, 10f), 10, Width / (float)Height);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            initProgram();

            GL.ClearColor(Color.CornflowerBlue);
            GL.ClearStencil(0);
            GL.PointSize(5f);

            OnUpdateFrame(new FrameEventArgs());
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            TimeRenderDelta += (float)e.Time;
            GL.Viewport(0, 0, Width, Height);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            Shaders["textured"].EnableVertexAttribArrays();
            Shaders["default"].EnableVertexAttribArrays();

            /*Matrix4 Mat = Matrix4.Identity * viewMatrix;
            GL.UniformMatrix4(shaders[activeShader].GetUniform("modelview"), false, ref Mat);*/
            GL.ColorMask(false, false, false, false); //Start using the stencil 
            GL.Enable(EnableCap.StencilTest);

            //Place a 1 where rendered 
            GL.StencilFunc(StencilFunction.Always, 1, 1);
            //Replace where rendered 
            GL.StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);
            //Render stencil triangle 
            //objects[1].Render(viewMatrix, TimeRenderDelta);
            //Reenable color 
            GL.ColorMask(true, true, true, true);
            //Where a 1 was not rendered 
            GL.StencilFunc(StencilFunction.Notequal, 1, 1);
            //Keep the pixel 
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            DrawScene(viewMatrix, (float)e.Time);


            Shaders["textured"].DisableVertexAttribArrays();
            Shaders["default"].DisableVertexAttribArrays();
            GL.Flush();
            SwapBuffers();
        }

        private void DrawScene(Matrix4 viewMatrix, float timeDelta)
        {
            // Draw all our objects
            foreach (Entity v in objects)
            {
                v.Render(viewMatrix, (float)Math.Min(TimeRenderDelta, 1 / UpdateFrequency));
            }
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);
            Time += (float)e.Time;
            TimeRenderDelta = 0;

            InputExt.Update();
            if (Focused)
            {
                if (InputExt.KeyDown(Key.W))
                {
                    cam.Position += new Vector3(0, 0.02f, 0) * cam.Scale;
                }
                else if (InputExt.KeyDown(Key.S))
                {
                    cam.Position -= new Vector3(0, 0.02f, 0) * cam.Scale;
                }
                if (InputExt.KeyDown(Key.A))
                {
                    cam.Position -= new Vector3(0.02f, 0, 0) * cam.Scale;
                }
                else if (InputExt.KeyDown(Key.D))
                {
                    cam.Position += new Vector3(0.02f, 0, 0) * cam.Scale;
                }
                if (InputExt.MouseWheelDelta() != 0)
                {
                    cam.Scale /= (float)Math.Pow(1.2, InputExt.MouseWheelDelta());
                }
            }
            objects[1].Transform.Rotation += new Quaternion(0, 0, 0, .01f);
            // Update model view matrices
            viewMatrix = cam.GetViewMatrix();
            foreach (Entity v in objects)
            {
                v.StepUpdate();
            }
        }

        int loadImage(Bitmap image)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }

        int loadImage(string filename)
        {
            try
            {
                Bitmap file = new Bitmap(filename);
                return loadImage(file);
            }
            catch (FileNotFoundException e)
            {
                return -1;
            }
        }
    }
}
