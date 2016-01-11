using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public struct PolyCoord
    {
        public int LineIndex;
        float _lineT;
        public float LineT 
        {
            get
            {
                return _lineT;
            }
            set
            {
                Debug.Assert(value >= 0 && value < 1, "LineT must have a value [0,1).");
                _lineT = value;
            }
        }

        #region Constructors
        public PolyCoord(int lineIndex, float lineT)
        {
            LineIndex = lineIndex;
            _lineT = 0;
            LineT = lineT;
        }
        #endregion
    }
}
