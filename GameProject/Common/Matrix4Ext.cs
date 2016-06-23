using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics;
using OpenTK;

namespace Game
{
    public static class Matrix4Ext
    {
        private const float EQUALITY_EPSILON = 0.0001f;
        private const int MATRIX_4_SIZE = 4;
        public static Matrix4 ConvertTo(FarseerPhysics.Common.Transform transform)
        {
            Matrix4 matrix = Matrix4.CreateRotationZ(transform.q.GetAngle());
            matrix = matrix * Matrix4.CreateTranslation(new Vector3(transform.p.X, transform.p.Y, 1));
            return matrix;
        }

        public static bool IsMirrored(Matrix4 matrix)
        {
            Vector2[] mirrorTest = new Vector2[3] {
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(0, 0)
            };
            mirrorTest = Vector2Ext.Transform(mirrorTest, matrix);
            return MathExt.AngleDiff(MathExt.AngleVector(mirrorTest[0] - mirrorTest[2]), MathExt.AngleVector(mirrorTest[1] - mirrorTest[2])) > 0;
        }

        public static bool AlmostEqual(Matrix4 matrix0, Matrix4 matrix1)
        {
            return AlmostEqual(matrix0, matrix1, EQUALITY_EPSILON);
        }

        public static bool AlmostEqual(Matrix4 matrix0, Matrix4 matrix1, float delta)
        {
            for (int i = 0; i < MATRIX_4_SIZE; i++)
            {
                for (int j = 0; j < MATRIX_4_SIZE; j++)
                {
                    if (Math.Abs(matrix0[i, j] - matrix1[i, j]) > delta)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
