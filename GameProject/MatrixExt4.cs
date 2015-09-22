using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FarseerPhysics;
using OpenTK;

namespace Game
{
    public class MatrixExt4
    {
        public static Matrix4 ConvertTo(FarseerPhysics.Common.Transform transform)
        {
            Matrix4 matrix = Matrix4.CreateRotationZ(transform.R.Angle);
            matrix = matrix * Matrix4.CreateTranslation(new Vector3(transform.Position.X, transform.Position.Y, 1));
            return matrix;
        }
    }
}
