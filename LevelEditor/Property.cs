using Game;
using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelEditor
{
    public class Property
    {
        [BrowsableAttribute(true)]
        public string blah { get; set; }
        [BrowsableAttribute(true)]
        public int test { get; set; }
        [CategoryAttribute("category1")]
        [BrowsableAttribute(true)]
        public string lala 
        {
            get { return "aaa"; } 
            set { } 
        }
        /*[BrowsableAttribute(true)]
        Transform2D transform = new Transform2D();*/
        [System.ComponentModel.TypeConverter(typeof(PropertyGridConverter)), BrowsableAttribute(true)]
        public Vector3 vec3 { get; set; }
        [System.ComponentModel.TypeConverter(typeof(PropertyGridConverter)), BrowsableAttribute(true)]
        public Vector2 vec2 { get; set; }
        [BrowsableAttribute(true)]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public SubProperty subproperty { get; set; }
        public Property()
        {
            subproperty = new SubProperty();
            vec3 = new Vector3(0, 2, 3);
        }
    }
}
