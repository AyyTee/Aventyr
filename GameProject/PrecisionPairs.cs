using Game.Common;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class PrecisionPairs
    {
        public static List<Tuple<string, string>> Pairs =>
            new List<Tuple<string, string>>()
            {
                new Tuple<string, string>(nameof(Transform2d), "Transform2"),
                new Tuple<string, string>(nameof(Transform3d), nameof(Transform3)),
                new Tuple<string, string>(nameof(Vector2d), nameof(Vector2)),
                new Tuple<string, string>(nameof(Vector3d), nameof(Vector3)),
                new Tuple<string, string>(nameof(Matrix2d), nameof(Matrix2)),
                new Tuple<string, string>(nameof(Matrix3d), nameof(Matrix3)),
                new Tuple<string, string>(nameof(Matrix4d), nameof(Matrix4)),
                new Tuple<string, string>(nameof(Quaterniond), nameof(Quaternion)),
                new Tuple<string, string>("double", "float"),
            };
    }
}
