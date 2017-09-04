//The MIT License(MIT)

//Copyright(c) 2016 Chevy Ray Johnston

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBuilder
{
    using Game.Common;
    using System;
    using System.Collections.Generic;

    public class RectanglePacker
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        List<Node> nodes = new List<Node>();

        public RectanglePacker(Vector2i atlasSize)
        {
            Width = atlasSize.X;
            Height = atlasSize.Y;
            nodes.Add(new Node(0, 0, int.MaxValue, int.MaxValue));
        }

        public bool Pack(Vector2i size, out Vector2i position)
        {
            int x, y;
            int w = size.X;
            int h = size.Y;
            for (int i = 0; i < nodes.Count; ++i)
            {
                if (w <= nodes[i].W && h <= nodes[i].H)
                {
                    var node = nodes[i];
                    nodes.RemoveAt(i);
                    x = node.X;
                    y = node.Y;
                    int r = x + w;
                    int b = y + h;
                    nodes.Add(new Node(r, y, node.Right - r, h));
                    nodes.Add(new Node(x, b, w, node.Bottom - b));
                    nodes.Add(new Node(r, b, node.Right - r, node.Bottom - b));
                    Width = Math.Max(Width, r);
                    Height = Math.Max(Height, b);
                    position = new Vector2i(x, y);
                    return true;
                }
            }
            position = new Vector2i();
            return false;
        }

        public struct Node
        {
            public int X;
            public int Y;
            public int W;
            public int H;

            public Node(int x, int y, int w, int h)
            {
                X = x;
                Y = y;
                W = w;
                H = h;
            }

            public int Right
            {
                get { return X + W; }
            }

            public int Bottom
            {
                get { return Y + H; }
            }
        }
    }
}
