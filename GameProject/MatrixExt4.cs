using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics;
using OpenTK;

namespace Game
{
    public static class MatrixExt4
    {
        private const float EQUALS_ERROR_MARGIN = 0.0001f;
        public static Matrix4 ConvertTo(FarseerPhysics.Common.Transform transform)
        {
            Matrix4 matrix = Matrix4.CreateRotationZ(transform.R.Angle);
            matrix = matrix * Matrix4.CreateTranslation(new Vector3(transform.Position.X, transform.Position.Y, 1));
            return matrix;
        }
        public static bool Equals(Matrix4 matrix0, Matrix4 matrix1)
        {
            if (Math.Abs(matrix0.M11 - matrix1.M11) > EQUALS_ERROR_MARGIN) { return false; }
            if (Math.Abs(matrix0.M12 - matrix1.M12) > EQUALS_ERROR_MARGIN) { return false; }
            if (Math.Abs(matrix0.M13 - matrix1.M13) > EQUALS_ERROR_MARGIN) { return false; }
            if (Math.Abs(matrix0.M14 - matrix1.M14) > EQUALS_ERROR_MARGIN) { return false; }
            if (Math.Abs(matrix0.M21 - matrix1.M21) > EQUALS_ERROR_MARGIN) { return false; }
            if (Math.Abs(matrix0.M22 - matrix1.M22) > EQUALS_ERROR_MARGIN) { return false; }
            if (Math.Abs(matrix0.M23 - matrix1.M23) > EQUALS_ERROR_MARGIN) { return false; }
            if (Math.Abs(matrix0.M24 - matrix1.M24) > EQUALS_ERROR_MARGIN) { return false; }
            if (Math.Abs(matrix0.M31 - matrix1.M31) > EQUALS_ERROR_MARGIN) { return false; }
            if (Math.Abs(matrix0.M32 - matrix1.M32) > EQUALS_ERROR_MARGIN) { return false; }
            if (Math.Abs(matrix0.M33 - matrix1.M33) > EQUALS_ERROR_MARGIN) { return false; }
            if (Math.Abs(matrix0.M34 - matrix1.M34) > EQUALS_ERROR_MARGIN) { return false; }
            if (Math.Abs(matrix0.M41 - matrix1.M41) > EQUALS_ERROR_MARGIN) { return false; }
            if (Math.Abs(matrix0.M42 - matrix1.M42) > EQUALS_ERROR_MARGIN) { return false; }
            if (Math.Abs(matrix0.M43 - matrix1.M43) > EQUALS_ERROR_MARGIN) { return false; }
            if (Math.Abs(matrix0.M44 - matrix1.M44) > EQUALS_ERROR_MARGIN) { return false; }
            return true;
        }
    }
}
