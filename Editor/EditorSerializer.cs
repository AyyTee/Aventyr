using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public class EditorSerializer : Serializer
    {
        public EditorSerializer()
        {
        }

        protected override IEnumerable<Type> GetKnownTypes()
        {
            var types = base.GetKnownTypes();
            var editorTypes = from t in Assembly.GetExecutingAssembly().GetTypes()
                   where Attribute.IsDefined(t, typeof(DataContractAttribute))
                   select t;
            List<Type> typeList = types.ToList();
            typeList.AddRange(editorTypes);
            return typeList;
        }
    }
}
