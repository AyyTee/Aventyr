using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using System.Runtime.Serialization;

namespace Game.Rendering
{
    [DataContract]
    public class SimpleRenderWindow : IRenderWindow
    {
        [DataMember]
        public Vector2i CanvasPosition { get; private set; }
        [DataMember]
        public Vector2i CanvasSize { get; private set; }
        [DataMember]
        public List<IRenderLayer> Layers { get; private set; }
        [DataMember]
        public float RendersPerSecond { get; private set; }

        public SimpleRenderWindow()
        {
        }

        public SimpleRenderWindow(IRenderWindow window)
        {
            CanvasPosition = window.CanvasPosition;
            CanvasSize = window.CanvasSize;
            Layers = window.Layers.ToList();
            RendersPerSecond = window.RendersPerSecond;
        }
    }
}
