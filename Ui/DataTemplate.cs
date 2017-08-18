using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui
{
    public class DataTemplate<T> where T : class
    {
        public DataTemplate(Func<IEnumerable<T>> data, Func<T, IElement> template)
        {

        }
    }
}
