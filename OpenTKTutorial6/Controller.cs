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

namespace OpenTKTutorial6
{
    class Controller : GameWindow
    {
        public Controller() : base(1024, 768, new GraphicsMode(32, 24, 0, 4), "Game", GameWindowFlags.FixedWindow)
        {
            
        }
        //InputExt InputExt;
        int[] indicedata;
        int ibo_elements;
        Camera cam;
        Vector2 lastMousePos = new Vector2();

        List<Model> objects = new List<Model>();
        Dictionary<string, int> textures = new Dictionary<string, int>();
        Dictionary<string, ShaderProgram> shaders = new Dictionary<string, ShaderProgram>();
        string activeShader = "default";

        Matrix4 viewMatrix;
        
        float time = 0.0f;
        float timeDraw = 0.0f;

        void initProgram()
        {
            //GraphicsContext.CurrentContext.SwapInterval = 1;
            lastMousePos = new Vector2(Mouse.X, Mouse.Y);

            GL.GenBuffers(1, out ibo_elements);

            // Load shaders from file
            shaders.Add("default", new ShaderProgram("vs.glsl", "fs.glsl", true));
            shaders.Add("textured", new ShaderProgram("vs_tex.glsl", "fs_tex.glsl", true));

            activeShader = "textured";

            // Load textures from file
            textures.Add("opentksquare.png", loadImage("opentksquare.png"));
            textures.Add("opentksquare2.png", loadImage("opentksquare2.png"));
            textures.Add("grid.png", loadImage("grid.png"));
            // Create our objects
            TexturedCube tc = new TexturedCube();
            tc.TextureID = textures["opentksquare.png"];
            objects.Add(tc);

            TexturedCube tc2 = new TexturedCube();
            tc2.Position += new Vector3(1f, 1f, 1f);
            tc2.TextureID = textures["opentksquare2.png"];
            objects.Add(tc2);
            Plane background = new Plane();
            background.TextureID = textures["grid.png"];
            background.Scale = new Vector3(10f, 10f, 10f);
            background.TextureScale = 10;
            objects.Add(background);

            cam = Camera.CameraOrtho(new Vector3(0f, 0f, 10f), 10, Width / (float)Height);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            initProgram();

            GL.ClearColor(Color.CornflowerBlue);
            GL.ClearStencil(0);
            GL.PointSize(5f);
            //step once to initialize variables before drawing
            OnUpdateFrame(new FrameEventArgs());
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Viewport(0, 0, Width, Height);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            {
                //GL.UniformMatrix4(shaders[activeShader].GetUniform("modelview"), false, ref Matrix4.Identity());
                /*Matrix4 Mat = cam.GetViewMatrix();
                GL.UniformMatrix4(shaders[activeShader].GetUniform("modelview"), false, ref Mat);
                GL.ColorMask(false, false, false, false); //Start using the stencil 
                GL.Enable(EnableCap.StencilTest);

                //Place a 1 where rendered 
                GL.StencilFunc(StencilFunction.Always, 1, 1);
                //Replace where rendered 
                GL.StencilOp(StencilOp.Replace, StencilOp.Replace, StencilOp.Replace);
                activeShader = "default";
                //Render stencil triangle 
                GL.Begin(PrimitiveType.Triangles);
                GL.Vertex2(new Vector2(0.0f, 0.0f));
                GL.Vertex2(new Vector2(1.0f, 0.0f));
                GL.Vertex2(new Vector2(0.0f, 1.0f));
                GL.End();
                activeShader = "textured";
                //Reenable color 
                GL.ColorMask(true, true, true, true);
                //Where a 1 was not rendered 
                GL.StencilFunc(StencilFunction.Notequal, 1, 1);
                //Keep the pixel 
                GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);*/


                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.CullFace);
                GL.CullFace(CullFaceMode.Back);
                DrawScene((float) e.Time);
            }
            timeDraw += (float)e.Time;
            GL.Flush();
            SwapBuffers();
        }

        private void DrawScene(float timeDelta)
        {
            shaders[activeShader].EnableVertexAttribArrays();

            int indiceat = 0;

            // Draw all our objects
            foreach (Model v in objects)
            {
                GL.BindTexture(TextureTarget.Texture2D, v.TextureID);
                GL.UniformMatrix4(shaders[activeShader].GetUniform("modelMatrix"), false, ref v.ModelMatrix);
                GL.UniformMatrix4(shaders[activeShader].GetUniform("viewMatrix"), false, ref viewMatrix);

                GL.Uniform1(shaders[activeShader].GetUniform("timeDelta"), (float)Math.Min(timeDraw - time, 1/UpdateFrequency));
                GL.Uniform3(shaders[activeShader].GetUniform("speed"), ref v.Speed);
                if (shaders[activeShader].GetAttribute("maintexture") != -1)
                {
                    GL.Uniform1(shaders[activeShader].GetAttribute("maintexture"), v.TextureID);
                }

                GL.DrawElements(BeginMode.Triangles, v.IndiceCount, DrawElementsType.UnsignedInt, indiceat * sizeof(uint));
                indiceat += v.IndiceCount;
            }

            shaders[activeShader].DisableVertexAttribArrays();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            List<Vector3> verts = new List<Vector3>();
            List<int> inds = new List<int>();
            List<Vector3> colors = new List<Vector3>();
            List<Vector2> texcoords = new List<Vector2>();

            // Assemble vertex and indice data for all volumes
            int vertcount = 0;
            foreach (Model v in objects)
            {
                verts.AddRange(v.GetVerts().ToList());
                inds.AddRange(v.GetIndices(vertcount).ToList());
                colors.AddRange(v.GetColorData().ToList());
                texcoords.AddRange(v.GetTextureCoords());
                vertcount += v.VertCount;
            }

            Vector3[] vertdata;
            Vector3[] coldata;
            Vector2[] texcoorddata;

            vertdata = verts.ToArray();
            indicedata = inds.ToArray();
            coldata = colors.ToArray();
            texcoorddata = texcoords.ToArray();

            GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vPosition"));

            GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(vertdata.Length * Vector3.SizeInBytes), vertdata, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 0, 0);

            // Buffer vertex color if shader supports it
            if (shaders[activeShader].GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("vColor"));
                GL.BufferData<Vector3>(BufferTarget.ArrayBuffer, (IntPtr)(coldata.Length * Vector3.SizeInBytes), coldata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }


            // Buffer texture coordinates if shader supports it
            if (shaders[activeShader].GetAttribute("texcoord") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, shaders[activeShader].GetBuffer("texcoord"));
                GL.BufferData<Vector2>(BufferTarget.ArrayBuffer, (IntPtr)(texcoorddata.Length * Vector2.SizeInBytes), texcoorddata, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(shaders[activeShader].GetAttribute("texcoord"), 2, VertexAttribPointerType.Float, true, 0, 0);
            }

            // Update object positions
            
            time += (float)e.Time;
            timeDraw = time;
            //objects[0].Position = new Vector3(0.3f, (float)Math.Sin(time)/2, 3.0f);
            objects[0].Position += objects[0].Speed * (float)e.Time;
            objects[0].Speed = new Vector3(0f, (float)Math.Cos(time)/2, 0f);
            //objects[0].Speed = new Vector3(0f, 0f, 0f);
            
            //objects[0].Rotation = new Vector3(0.55f * time, 0.25f * time, 0);
            objects[0].Scale = new Vector3(0.5f, 0.5f, 0.5f);

            
            //objects[1].Position = new Vector3(-1f, 0.5f + (float)Math.Cos(time), 2.0f);
            objects[1].Position += objects[1].Speed * (float)e.Time;
            objects[1].Speed = new Vector3(0f, -1f, 0f);
            objects[1].Rotation = new Vector3(-0.25f * time, -0.35f * time, 0);
            objects[1].Scale = new Vector3(0.7f, 0.7f, 0.7f);

            //objects[2].Rotation = new Vector3(-0.25f * time, -0.35f * time, 0);

            // Update model view matrices
            viewMatrix = cam.GetViewMatrix();
            foreach (Model v in objects)
            {
                v.CalculateModelMatrix();
                //v.ModelViewProjectionMatrix = v.ModelMatrix * cam.GetViewMatrix();
            }

            GL.UseProgram(shaders[activeShader].ProgramID);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            // Buffer index data
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo_elements);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicedata.Length * sizeof(int)), indicedata, BufferUsageHint.StaticDraw);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            if (e.KeyChar == 27)
            {
                Exit();
            }

            switch (e.KeyChar)
            {
                case 'w':
                    cam.Position.Y += 0.1f;
                    break;
                case 'a':
                    cam.Position.X -= 0.1f;
                    break;
                case 's':
                    cam.Position.Y -= 0.1f;
                    break;
                case 'd':
                    cam.Position.X += 0.1f;
                    break;
                case 'q':
                    cam.Scale *= 1.1f;
                    cam.SetFOV(cam.GetFOV() * 1.1f);
                    break;
                case 'e':
                    cam.Scale /= 1.1f;
                    cam.SetFOV(cam.GetFOV() / 1.1f);
                    break;
            }
        }

        protected override void OnFocusedChanged(EventArgs e)
        {
            base.OnFocusedChanged(e);

            /*if (Focused)
            {
                ResetCursor();
            }*/
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
