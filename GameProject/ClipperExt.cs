using ClipperLib;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class ClipperExt<T>
    {
        Clipper _clipper = new Clipper();
        List<T> _data = new List<T>();
        const int NULL_POINTER = -1;

        public ClipperExt()
        {
            _clipper.StrictlySimple = true;
            _clipper.ZFillFunction = new Clipper.ZFillCallback(
                delegate(IntPoint e1Bot, IntPoint e1Top, IntPoint e2Bot, IntPoint e2Top, ref IntPoint pt)
                {
                    int index0 = (int)e1Bot.Z;
                    int index1 = (int)e1Top.Z;
                    if (index0 == NULL_POINTER)
                    {
                        index0 = (int)e2Bot.Z;
                        index1 = (int)e2Top.Z;
                    }
                    Debug.Assert(index0 != index1);
                    Debug.Assert(index0 != NULL_POINTER && index1 != NULL_POINTER);
                    float t = new Line(ToVector2(e1Top), ToVector2(e1Bot)).NearestT(ToVector2(pt), false);
                    Debug.Assert(t >= 0 && t <= 1);
                    T ptNew = _data[index0];//.Lerp(_data[index1], t);
                    if (pt.Z > 4)
                    {
                    }
                    pt.Z = _data.Count;
                    _data.Add(ptNew);
                });
        }

        public bool AddPath(Vector2[] pg, PolyType polyType, bool isClosed, T[] data)
        {
            return _clipper.AddPath(ToIntPoint(pg, data), polyType, isClosed);
        }

        public bool AddPaths(Vector2[][] pg, PolyType polyType, bool isClosed, T[][] data)
        {
            List<List<IntPoint>> list = new List<List<IntPoint>>();
            for (int i = 0; i < pg.Length; i++)
            {
                list.Add(ToIntPoint(pg[i], data[i]));
            }
            return _clipper.AddPaths(list, polyType, isClosed);
        }

        public bool AddPaths(List<List<IntPoint>> pg, PolyType polyType, bool isClosed, T[][] data)
        {
            List<List<IntPoint>> clone = new List<List<IntPoint>>(pg);
            for (int i = 0; i < clone.Count; i++)
            {
                for (int j = 0; j < clone[i].Count; j++)
                {
                    Debug.Assert(data == null || data[i] == null || data.Length == clone[i].Count);
                    if (data == null || data[i] == null)
                    {
                        clone[i][j] = new IntPoint(clone[i][j].X, clone[i][j].Y, NULL_POINTER);
                    }
                    else
                    {
                        clone[i][j] = new IntPoint(clone[i][j].X, clone[i][j].Y, _data.Count);
                        _data.Add(data[i][j]);
                    }
                }
            }
            return _clipper.AddPaths(pg, polyType, isClosed);
        }

        public bool Execute(ClipType clipType, out PolyTree polyTree, out List<T> data)
        {
            polyTree = new PolyTree();
            bool result = _clipper.Execute(clipType, polyTree);
            data = new List<T>(_data);
            Clear();
            return result;
        }

        public bool Execute(ClipType clipType, out List<List<IntPoint>> solution, out List<T> data)
        {
            solution = new List<List<IntPoint>>();
            bool result = _clipper.Execute(clipType, solution);
            data = new List<T>(_data);
            Clear();
            return result;
        }

        public bool Execute(ClipType clipType, out PolyTree polyTree, out List<T> data, PolyFillType subjFillType, PolyFillType clipFillType)
        {
            polyTree = new PolyTree();
            bool result = _clipper.Execute(clipType, polyTree, subjFillType, clipFillType);
            data = new List<T>(_data);
            Clear();
            return result;
        }

        public bool Execute(ClipType clipType, out List<List<IntPoint>> solution, out List<T> data, PolyFillType subjFillType, PolyFillType clipFillType)
        {
            solution = new List<List<IntPoint>>();
            bool result = _clipper.Execute(clipType, solution, subjFillType, clipFillType);
            data = new List<T>(_data);
            Clear();
            return result;
        }

        public void Clear()
        {
            _clipper.Clear();
            _data.Clear();
        }

        private Vector2 ToVector2(IntPoint point)
        {
            return new Vector2((float)(point.X / ClipperConvert.SCALE_FACTOR), (float)(point.Y / ClipperConvert.SCALE_FACTOR));
        }

        private Vector2[] ToVector2(List<IntPoint> point)
        {
            Vector2[] polygon = new Vector2[point.Count];
            for (int i = 0; i < point.Count; i++)
            {
                polygon[i] = ToVector2(point[i]);
            }
            return polygon;
        }

        private List<IntPoint> ToIntPoint(Vector2[] vertices, T[] data)
        {
            Debug.Assert(data == null || data.Length == vertices.Length);
            var path = new List<IntPoint>();
            for (int i = 0; i < vertices.Length; i++)
            {
                IntPoint point;
                if (data == null)
                {
                    point = new IntPoint(vertices[i].X * ClipperConvert.SCALE_FACTOR, vertices[i].Y * ClipperConvert.SCALE_FACTOR, NULL_POINTER);
                }
                else
                {
                    point = new IntPoint(vertices[i].X * ClipperConvert.SCALE_FACTOR, vertices[i].Y * ClipperConvert.SCALE_FACTOR, _data.Count);
                    _data.Add(data[i]);
                }
                path.Add(point);
            }
            return path;
        }
    }
}
