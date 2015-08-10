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
using QuickFont;

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
        /// <summary>
        /// Intended to keep pointless messages from the Poly2Tri library out of the console window
        /// </summary>
        public static StreamWriter Log = new StreamWriter("Triangulating.txt");

        Model background;
        QFont Default;
        Entity box2;
        List<Portal> portals = new List<Portal>();
        public static List<int> iboGarbage = new List<int>();

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
            Default = new QFont(@"fonts\Times.ttf", 72, new QFontBuilderConfiguration(false));

            InputExt = new InputExt(this);
            lastMousePos = new Vector2(Mouse.X, Mouse.Y);

            // Load shaders from file
            Shaders.Add("default", new ShaderProgram(@"assets\shaders\vs.glsl", @"assets\shaders\fs.glsl", true));
            Shaders.Add("textured", new ShaderProgram(@"assets\shaders\vs_tex.glsl", @"assets\shaders\fs_tex.glsl", true));

            // Load textures from file
            textures.Add("default.png", loadImage(@"assets\default.png"));
            textures.Add("grid.png", loadImage(@"assets\grid.png"));
            // Create our objects

            background = Model.CreatePlane();
            background.TextureID = textures["grid.png"];
            background.Transform.Position = new Vector3(0, 0, -10f);
            float size = 100;
            background.Transform.Scale = new Vector3(size, size, size);
            background.TransformUV.Scale = new Vector2(size, size);
            Entity back = new Entity(new Vector2(0f, 0f));
            back.Models.Add(background);
            objects.Add(back);

            Portal portal0 = new Portal(true);
            //portal0.Transform.Rotation = (float)Math.PI/2f;
            portal0.Transform.Position = new Vector2(-1f, 0f);
            //portal0.Transform.Scale = new Vector2(-3f, -3f);
            portal0.Models[0].TransformUV.Scale = new Vector2(5f, 5f);
            objects.Add(portal0);
            portals.Add(portal0);

            Portal portal1 = new Portal(true);
            portal1.Transform.Rotation = 0.1f;
            portal1.Transform.Position = new Vector2(1f, -0.1f);
            portal1.Transform.Scale = new Vector2(-1.5f, 1.5f);
            objects.Add(portal1);
            portals.Add(portal1);
            Portal.Link(portal0, portal1);

            Model tc = Model.CreateCube();
            tc.Transform.Position = new Vector3(1f, 3f, 0);
            Entity box = new Entity(new Vector2(0,0));
            box.Models.Add(tc);
            objects.Add(box);

            Model tc2 = Model.CreateCube();
            tc2.Transform.Position = new Vector3(-1f, 3f, 0);
            tc2.Transform.Rotation = new Quaternion(1, 0, 0, 1);
            box2 = new Entity(new Vector2(0, 0));
            box2.Models.Add(tc2);
            objects.Add(box2);

            Entity last = new Entity();
            last.Models.Add(new Model());
            objects.Add(last);

            cam = Camera.CameraOrtho(new Vector3(0f, 0f, 10f), 10, Width / (float)Height);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            initProgram();

            GL.ClearColor(Color.CornflowerBlue);
            GL.ClearStencil(0);
            //GL.ClearDepth(1);
            GL.PointSize(5f);

            OnUpdateFrame(new FrameEventArgs());
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            TimeRenderDelta += (float)e.Time;
            GL.Viewport(0, 0, Width, Height);

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            Shaders["textured"].EnableVertexAttribArrays();
            Shaders["default"].EnableVertexAttribArrays();

            // Update model view matrices
            viewMatrix = cam.GetViewMatrix();
            GL.Disable(EnableCap.StencilTest);
            GL.Enable(EnableCap.DepthTest);
            
            DrawScene(viewMatrix, (float)e.Time);
            DrawDebug();
            
            Vector2 viewPos = new Vector2(cam.Transform.Position.X, cam.Transform.Position.Y);
            DrawPortalAll(portals.ToArray(), viewMatrix, viewPos, 4, TimeRenderDelta);
            Shaders["textured"].DisableVertexAttribArrays();
            Shaders["default"].DisableVertexAttribArrays();

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            GL.Flush();
            SwapBuffers();
        }

        private void DrawPortalAll(Portal[] portals, Matrix4 viewMatrix, Vector2 viewPos, int depth, float timeDelta)
        {
            
            //_DrawPortalAll(portals, null, viewMatrix, viewPos, depth, timeDelta, 0);

            //stopgap solution. portals will only recursively draw themselves, not any other portals
            IOrderedEnumerable<Portal> portalSort = portals.OrderByDescending(item => (item.Transform.Position - viewPos).Length);
            foreach (Portal p in portalSort)
            {
                GL.Clear(ClearBufferMask.StencilBufferBit);
                _DrawPortalAll(null, p, viewMatrix, viewPos, depth, timeDelta, 0);
            }
        }

        private void _DrawPortalAll(Portal[] portals, Portal previous, Matrix4 viewMatrix, Vector2 viewPos, int depth, float timeDelta, int count)
        {
            /*IOrderedEnumerable<Portal> portalSort = portals.OrderByDescending(item => (item.Transform.Position - viewPos).Length);
            foreach (Portal p in portalSort)
            {
                if (p.Linked != null && p.Linked != previous)
                {
                    DrawPortal(p, portals, viewMatrix, viewPos, depth, timeDelta, count);
                }
            }*/

            //stopgap solution. portals will only recursively draw themselves, not any other portals
            DrawPortal(null, previous, viewMatrix, viewPos, depth, timeDelta, count);
        }

        public void DrawPortal(Portal[] portals, Portal portalEnter, Matrix4 viewMatrix, Vector2 viewPos, int depth, float timeDelta, int count)
        {
            if (depth <= 0)
            {
                return;
            }
            GL.Clear(ClearBufferMask.DepthBufferBit);

            //Start using the stencil 
            GL.ColorMask(false, false, false, false);
            //GL.DepthMask(false);
            GL.Enable(EnableCap.StencilTest);
            GL.Disable(EnableCap.DepthTest);
            //Place a 1 where rendered 
            GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
            //Replace where rendered 
            GL.StencilOp(StencilOp.Incr, StencilOp.Incr, StencilOp.Incr);

            TextWriter console = Console.Out;
            Console.SetOut(Controller.Log);
            Entity fov = new Entity();
            //fov.Models.Clear();
            Vector2[] a = portalEnter.GetFOV(viewPos, 50);
            if (a.Length >= 3)
            {
                fov.Models.Add(Model.CreatePolygon(a));
                fov.Render(viewMatrix, timeDelta);
            }
            Console.SetOut(console);
            //Reenable color 
            GL.ColorMask(true, true, true, true);
            //GL.DepthMask(true);
            //Where a 1 was not rendered 
            GL.StencilFunc(StencilFunction.Less, count, 0xFF);
            //Keep the pixel 
            GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);


            GL.Enable(EnableCap.DepthTest);
            Matrix4 portalMatrix = Portal.GetMatrix(portalEnter.Linked, portalEnter) * viewMatrix;
            DrawScene(portalMatrix, timeDelta);

            //GL.Disable(EnableCap.StencilTest);

            Entity fovOutline = new Entity();
            Vector2[] verts = portalEnter.GetFOV(viewPos, 50, 2);
            if (verts.Length > 0)
            {
                fovOutline.Models.Add(Model.CreateLine(new Vector2[] { verts[1], verts[2] }));
                fovOutline.Models.Add(Model.CreateLine(new Vector2[] { verts[0], verts[3] }));
            }
            GL.LineWidth(2f);
            fovOutline.Render(viewMatrix, timeDelta);
            GL.LineWidth(1f);

            _DrawPortalAll(portals, portalEnter, portalMatrix, VectorExt2.Transform(viewPos, Portal.GetMatrix(portalEnter, portalEnter.Linked)), depth - 1, timeDelta, count + 1);

            
        }

        private void DrawDebug()
        {
            Vector3[] vector = new Vector3[6];
            float size = 1 / (float)80;
            vector[0] = cam.Transform.Position - cam.GetRight() * cam.Transform.Scale.Y * size + new Vector3(0, 0, -2);
            vector[1] = cam.Transform.Position + cam.GetRight() * cam.Transform.Scale.Y * size + new Vector3(0, 0, -2);
            vector[2] = cam.Transform.Position + new Vector3(0, 0, -2);
            vector[3] = cam.Transform.Position + cam.GetUp() * cam.Transform.Scale.Y * size + new Vector3(0, 0, -2);
            vector[4] = cam.Transform.Position + new Vector3(0, 0, -2);
            vector[5] = cam.Transform.Position + (cam.GetUp() + cam.GetRight()) * cam.Transform.Scale.Y * size + new Vector3(0, 0, -2);
            GL.LineWidth(2f);
            GL.Begin(PrimitiveType.Lines);
            foreach (Vector3 v in vector)
            {
                GL.Vertex3(v);
            }
            /*Matrix4 m = Portal.GetMatrix(portals[0], portals[1]);
            foreach (Vector3 v in vector)
            {
                GL.Vertex3(Vector3.Transform(v, m));
            }*/
            GL.End();
            GL.LineWidth(1f);
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

            #region camera movement
            if (Focused)
            {
                if (InputExt.KeyPress(Key.Escape))
                {
                    Exit();
                }
                Vector3 v = new Vector3();
                float camSpeed = .009f;
                if (InputExt.KeyDown(Key.W))
                {
                    v += cam.GetUp() * camSpeed * cam.Transform.Scale.Y;
                }
                else if (InputExt.KeyDown(Key.S))
                {
                    v -= cam.GetUp() * camSpeed * cam.Transform.Scale.Y;
                }
                if (InputExt.KeyDown(Key.A))
                {
                    v -= cam.GetRight() * camSpeed * cam.Transform.Scale.X;
                }
                else if (InputExt.KeyDown(Key.D))
                {
                    v += cam.GetRight() * camSpeed * cam.Transform.Scale.X;
                }
                if (InputExt.MouseWheelDelta() != 0)
                {
                    cam.Transform.Scale /= (float)Math.Pow(1.2, InputExt.MouseWheelDelta());
                }
                Vector2[] vArray = new Vector2[2];
                IntersectPoint i = new IntersectPoint();
                Portal portalEnter = null;
                foreach (Portal p in portals)
                {
                    vArray = VectorExt2.Transform(p.GetVerts(), p.Transform.GetMatrix());
                    portalEnter = p;
                    Vector2 camPos = new Vector2(cam.Transform.Position.X, cam.Transform.Position.Y);
                    i = MathExt.LineIntersection(vArray[0], vArray[1], camPos, camPos + new Vector2(v.X, v.Y), true);
                    if (i.Exists)
                    {
                        break;
                    }
                }
                cam.Transform.Position += v;
                if (i.Exists)
                {
                    Portal portalExit = portalEnter.Linked;
                    //Transform2D t = Portal.GetTransform(portalLast, portalExit);
                    Matrix4 m = Portal.GetMatrix(portalEnter, portalExit);
                    Transform2D t = cam.Transform.GetTransform2D();
                    Vector2 v0 = new Vector2(t.Position.X, t.Position.Y);
                    Vector2 v1 = v0 + new Vector2(1, 0);
                    Vector2 v2 = v0 + new Vector2(0, 1);
                    Vector2 v3 = VectorExt2.Transform(v0, m);
                    Vector2 v4 = VectorExt2.Transform(v1, m);
                    Vector2 v5 = VectorExt2.Transform(v2, m);

                    Vector2 v6 = v0 + new Vector2((float)Math.Cos(cam.Transform.Rotation.W), (float)Math.Sin(cam.Transform.Rotation.W));
                    Vector2 v7 = VectorExt2.Transform(v6, m) - v3;
                    Transform2D t0 = portalEnter.Transform;
                    Transform2D t1 = portalExit.Transform;

                    cam.Transform.Position = new Vector3(v3.X, v3.Y, cam.Transform.Position.Z);
                    /*cam.Transform.Rotation += new Quaternion(0, 0, 0, (float)(MathExt.AngleLine(v3, v5) + Math.PI));
                    float angle0 = (float)MathExt.AngleLine(v3, v4);
                    float angle1 = (float)MathExt.AngleLine(v3, v5);*/
                    Transform2D tEnter = portalEnter.Transform;
                    Transform2D tExit = portalExit.Transform;
                    float flipX = 1;
                    float flipY = 1;
                    if (Math.Sign(tEnter.Scale.X) == Math.Sign(tExit.Scale.X))
                    {
                        flipX = -1;
                    }
                    if (Math.Sign(tEnter.Scale.Y) != Math.Sign(tExit.Scale.Y))
                    {
                        flipY = -1;
                    }
                    cam.Transform.Scale *= new Vector3(flipX * (v4 - v3).Length, flipY * (v5 - v3).Length, 1);

                    float angle;
                    if (flipX != flipY)
                    {
                        angle = (float)MathExt.AngleVector(v7);
                        cam.Transform.Rotation = new Quaternion(cam.Transform.Rotation.X, cam.Transform.Rotation.Y, cam.Transform.Rotation.Z, angle);
                        //angle = m.ExtractRotation(false).W;
                        /*Matrix4 m2 = m * Matrix4.CreateTranslation(new Vector3(v3.X - v0.X, v3.Y - v0.Y, 0)).Inverted();
                        m2 = Matrix4.CreateScale(new Vector3(flipX * (v4 - v3).Length, flipY * (v5 - v3).Length, 1)).Inverted() * m2;
                        Vector3 v7 = Vector3.Transform(new Vector3(1, 0, 0), m);

                        angle = -(float)(MathExt.AngleVector(new Vector2(v7.X, v7.Y)));
                        m2 = m2 * Matrix4.CreateRotationZ(angle).Inverted();*/

                        /*{
                            if (portalExit.Transform.Rotation == 0 || portalEnter.Transform.Rotation == 0)
                            {
                                if (portalExit.Transform.Rotation < 0 || portalEnter.Transform.Rotation < 0)
                                {
                                    angle = -Math.Abs(portalExit.Transform.Rotation - portalEnter.Transform.Rotation);
                                }
                                else
                                {
                                    angle = Math.Abs(portalExit.Transform.Rotation - portalEnter.Transform.Rotation);
                                }
                                cam.Transform.Rotation += new Quaternion(0, 0, 0, -2f * cam.Transform.Rotation.W);
                            }
                            else
                            {
                                angle = portalExit.Transform.Rotation - portalEnter.Transform.Rotation + (float)Math.PI;
                                //cam.Transform.Rotation += new Quaternion(0, 0, 0, -2f * cam.Transform.Rotation.W);
                            }
                            //cam.Transform.Rotation += new Quaternion(0, 0, 0, -2f * cam.Transform.Rotation.W);
                        }*/
                    }
                    else
                    {
                        angle = portalExit.Transform.Rotation - portalEnter.Transform.Rotation;
                        cam.Transform.Rotation += new Quaternion(0, 0, 0, angle);
                    }
                }
            }
            #endregion
            Console.Write(e.Time);
            Console.WriteLine();

            box2.Models[0].Transform.Rotation += new Quaternion(0, 0, 0, .01f);
            
            foreach (Entity v in objects)
            {
                v.StepUpdate();
            }
            
            //get rid of all ibo elements no longer used
            foreach (int iboElement in iboGarbage)
            {
                int a = iboElement;
                GL.DeleteBuffers(1, ref a);
            }
            iboGarbage.Clear();
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
