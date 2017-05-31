using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public class Serializer : Game.Serialization.Serializer
    {
        protected override IEnumerable<Type> GetKnownTypes()
        {
            return base.GetKnownTypes()
                .Concat(typeof(SceneState).Assembly
                    .GetTypes()
                    .Where(item => Attribute.IsDefined(item, typeof(DataContractAttribute))));
        }
    }
}
