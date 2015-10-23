using FarseerPhysics.Dynamics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    public static class BodyExt
    {
        public static void SetUserData(Body body, Entity entity)
        {
            //Ugly solution to storing Game classes in a way that still works when deserializing the data.
            //This list is intended to only store one element.
            var a = new List<BodyUserData>();
            body.UserData = a;
            a.Add(new BodyUserData(entity));
        }

        public static BodyUserData GetUserData(Body body)
        {
            return ((List<BodyUserData>)body.UserData)[0];
        }
    }
}
