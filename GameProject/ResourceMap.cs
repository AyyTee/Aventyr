using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public class ResourceMap<T> where T : IResource<T>
    {
        public class ResourceID
        {
            private Guid _id = new Guid();

            public ResourceID()
            {

            }

            public Guid ID
            {
                get { return _id; }
            }
        }

        private Dictionary<ResourceID, T> _idMap = new Dictionary<ResourceID, T>();

        private Dictionary<ResourceID, T> IDMap
        {
            get { return _idMap; }
        }

        public ResourceMap()
        {
        }

        public T Get(ResourceID id)
        {
            return IDMap[id];
        }

        public List<T> ToList()
        {
            return _idMap.Values.ToList<T>();
        }

        public bool Remove(ResourceID id)
        {
            return IDMap.Remove(id);
        }

        /*public bool Remove(T resource)
        {
            return Remove(resource.ID);
        }

        public void Add(T resource)
        {
            IDMap.Add(resource.ID, resource);
        }*/
    }
}
