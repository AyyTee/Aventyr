using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EditorLogic
{
    //The order in which various UI elements overlap.
    public static class DrawDepth
    {
        const float Offset = 1000;
        public const float EntityActive = 5f + Offset;
        public const float EntityMarker = 0f + Offset;
        public const float CameraMarker = 10f + Offset;
    }
}
