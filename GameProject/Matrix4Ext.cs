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

        public static bool Equals(Matrix4 matrix0, Matrix4 matrix1)
        {
            if (Math.Abs(matrix0.M11 - matrix1.M11) > EQUALITY_EPSILON) { return false; }
            if (Math.Abs(matrix0.M12 - matrix1.M12) > EQUALITY_EPSILON) { return false; }
            if (Math.Abs(matrix0.M13 - matrix1.M13) > EQUALITY_EPSILON) { return false; }
            if (Math.Abs(matrix0.M14 - matrix1.M14) > EQUALITY_EPSILON) { return false; }
            if (Math.Abs(matrix0.M21 - matrix1.M21) > EQUALITY_EPSILON) { return false; }
            if (Math.Abs(matrix0.M22 - matrix1.M22) > EQUALITY_EPSILON) { return false; }
            if (Math.Abs(matrix0.M23 - matrix1.M23) > EQUALITY_EPSILON) { return false; }
            if (Math.Abs(matrix0.M24 - matrix1.M24) > EQUALITY_EPSILON) { return false; }
            if (Math.Abs(matrix0.M31 - matrix1.M31) > EQUALITY_EPSILON) { return false; }
            if (Math.Abs(matrix0.M32 - matrix1.M32) > EQUALITY_EPSILON) { return false; }
            if (Math.Abs(matrix0.M33 - matrix1.M33) > EQUALITY_EPSILON) { return false; }
            if (Math.Abs(matrix0.M34 - matrix1.M34) > EQUALITY_EPSILON) { return false; }
            if (Math.Abs(matrix0.M41 - matrix1.M41) > EQUALITY_EPSILON) { return false; }
            if (Math.Abs(matrix0.M42 - matrix1.M42) > EQUALITY_EPSILON) { return false; }
            if (Math.Abs(matrix0.M43 - matrix1.M43) > EQUALITY_EPSILON) { return false; }
            if (Math.Abs(matrix0.M44 - matrix1.M44) > EQUALITY_EPSILON) { return false; }
            return true;
        }
    }
}
