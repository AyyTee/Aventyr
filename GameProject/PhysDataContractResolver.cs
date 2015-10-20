using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Game
{
    public class PhysDataContractResolver : DataContractResolver
    {
        private Dictionary<string, XmlDictionaryString> dictionary = new Dictionary<string, XmlDictionaryString>();
        Assembly assembly;

        public PhysDataContractResolver(Assembly assembly)
        {
            this.assembly = assembly;
        }

        // Used at deserialization
        // Allows users to map xsi:type name to any Type 
        public override Type ResolveName(string typeName, string typeNamespace, Type DeclaredType, DataContractResolver knownTypeResolver)
        {
            XmlDictionaryString tName;
            XmlDictionaryString tNamespace;
            if (dictionary.TryGetValue(typeName, out tName) && dictionary.TryGetValue(typeNamespace, out tNamespace))
            {
                return this.assembly.GetType(tNamespace.Value + "." + tName.Value);
            }
            else
            {
                return null;
            }
        }

        // Used at serialization
        // Maps any Type to a new xsi:type representation
        public override bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName, out XmlDictionaryString typeNamespace)
        {
            string name = dataContractType.Name;
            string namesp = dataContractType.Namespace;
            typeName = new XmlDictionaryString(XmlDictionary.Empty, name, 0);
            typeNamespace = new XmlDictionaryString(XmlDictionary.Empty, namesp, 0);
            if (!dictionary.ContainsKey(dataContractType.Name))
            {
                dictionary.Add(name, typeName);
            }
            if (!dictionary.ContainsKey(dataContractType.Namespace))
            {
                dictionary.Add(namesp, typeNamespace);
            }
            return true;
            /*DataContractResolver a = new DataContractResolver();
            a.TryResolveType(,)*/
        }

        /*public override bool TryResolveType(Type dataContractType, Type declaredType, DataContractResolver knownTypeResolver, out)
        {

        }*/
    }
}
