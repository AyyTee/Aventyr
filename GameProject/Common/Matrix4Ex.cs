using System;
using OpenTK;

namespace Game.Common
{
    public static class Matrix4Ex
    {
        const float EqualityEpsilon = 0.0001f;
        const int Matrix4Size = 4;
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
            mirrorTest = Vector2Ex.Transform(mirrorTest, matrix);
            return MathEx.AngleDiff(MathEx.VectorToAngleReversed(mirrorTest[0] - mirrorTest[2]), MathEx.VectorToAngleReversed(mirrorTest[1] - mirrorTest[2])) > 0;
        }

        public static bool AlmostEqual(Matrix4 matrix0, Matrix4 matrix1)
        {
            return AlmostEqual(matrix0, matrix1, EqualityEpsilon);
        }

        public static bool AlmostEqual(Matrix4 matrix0, Matrix4 matrix1, float delta)
        {
            for (int i = 0; i < Matrix4Size; i++)
            {
                for (int j = 0; j < Matrix4Size; j++)
                {
                    if (Math.Abs(matrix0[i, j] - matrix1[i, j]) > delta)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool AlmostEqual(Matrix4 matrix0, Matrix4 matrix1, float delta, float percent)
        {
            for (int i = 0; i < Matrix4Size; i++)
            {
                for (int j = 0; j < Matrix4Size; j++)
                {
                    if (Math.Abs(matrix0[i, j] - matrix1[i, j]) > delta && Math.Abs(1 - matrix1[i, j] / matrix0[i, j]) > percent)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool AlmostEqual(Matrix4d matrix0, Matrix4d matrix1)
        {
            return AlmostEqual(matrix0, matrix1, EqualityEpsilon);
        }

        public static bool AlmostEqual(Matrix4d matrix0, Matrix4d matrix1, double delta)
        {
            for (int i = 0; i < Matrix4Size; i++)
            {
                for (int j = 0; j < Matrix4Size; j++)
                {
                    if (Math.Abs(matrix0[i, j] - matrix1[i, j]) > delta)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool AlmostEqual(Matrix4d matrix0, Matrix4d matrix1, double delta, double percent)
        {
            for (int i = 0; i < Matrix4Size; i++)
            {
                for (int j = 0; j < Matrix4Size; j++)
                {
                    if (Math.Abs(matrix0[i, j] - matrix1[i, j]) > delta && Math.Abs(1 - matrix1[i, j] / matrix0[i, j]) > percent)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
