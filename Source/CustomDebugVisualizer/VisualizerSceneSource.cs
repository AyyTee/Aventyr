using System.IO;
using Microsoft.VisualStudio.DebuggerVisualizers;

namespace CustomDebugVisualizer
{
    public class VisualizerSceneSource : VisualizerObjectSource
    {
        public override void GetData(object target, Stream outgoingData)
        {
            Serializer.Serialize(target, outgoingData);
        }
    }
}
