using Game;
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
        [BrowsableAttribute(true)]
        Transform2D transform = new Transform2D();

        public Property()
        {

        }
    }
}
