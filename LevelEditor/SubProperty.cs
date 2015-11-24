using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelEditor
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class SubProperty : ExpandableObjectConverter
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

        public SubProperty()
        {
        }
    }
}
