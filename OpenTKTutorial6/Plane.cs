using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace OpenTKTutorial6
{
    class Plane : Model
    {
        public float TextureScale = 1;
        
        public Plane()
        {
            VertCount = 4;
            IndiceCount = 6;
            ColorDataCount = 4;
            TextureCoordsCount = 4;
        }

        public override Vector3[] GetVerts()
        {
            return new Vector3[] {
                new Vector3(-0.5f, 0.5f,  0f),
                new Vector3(0.5f, 0.5f,  0f),
                new Vector3(0.5f, -0.5f,  0f),
                new Vector3(-0.5f, -0.5f,  0f)
            };
        }

        public override int[] GetIndices(int offset = 0)
        {
            int[] inds = new int[] {
                0, 2, 1,
                0, 3, 2,
            };

            if (offset != 0)
            {
                for (int i = 0; i < inds.Length; i++)
                {
                    inds[i] += offset;
                }
            }

            return inds;
        }

        public override Vector3[] GetColorData()
        {
            return new Vector3[] {
                new Vector3(1f, 0f, 0f),
                new Vector3( 0f, 0f, 1f),
                new Vector3( 0f, 1f, 0f),
                new Vector3( 1f, 0f, 0f),
            };
        }

        public override void CalculateModelMatrix()
        {
            ModelMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateTranslation(Position);
        }

        public override Vector2[] GetTextureCoords()
        {
            return new Vector2[] {
                new Vector2(-1.0f, 1.0f) * TextureScale,
                new Vector2(0.0f, 1.0f) * TextureScale,
                new Vector2(0.0f, 0.0f) * TextureScale,
                new Vector2(-1.0f, 0.0f) * TextureScale
            };
        }
    }
}
