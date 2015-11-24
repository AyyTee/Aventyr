using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelEditor
{
    public class PropertyGridConverter : System.ComponentModel.ExpandableObjectConverter
    {
        const char _delimiter = ',';

        public override bool CanConvertFrom(System.ComponentModel.ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }

        public override object ConvertFrom(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            try
            {
                var type = context.PropertyDescriptor.PropertyType;
                if (type == typeof(Vector3))
                {
                    string[] tokens = ((string)value).Split(_delimiter);
                    return new Vector3(int.Parse(tokens[0]), int.Parse(tokens[1]), int.Parse(tokens[2]));
                }
                else if (type == typeof(Vector2))
                {
                    string[] tokens = ((string)value).Split(_delimiter);
                    return new Vector2(int.Parse(tokens[0]), int.Parse(tokens[1]));
                }
                return null;
            }
            catch
            {
                return context.PropertyDescriptor.GetValue(context.Instance);
            }
            //throw new ArgumentException("Unhandled conversion type.");
        }

        public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            var type = context.PropertyDescriptor.PropertyType;
            if (type == typeof(Vector3))
            {
                Vector3 p = (Vector3)value;
                return p.X + _delimiter.ToString() + p.Y + _delimiter.ToString() + p.Z;
            }
            else if (type == typeof(Vector2))
            {
                Vector2 p = (Vector2)value;
                return p.X + _delimiter.ToString() + p.Y;
            }
            return null;
            //throw new ArgumentException("Unhandled conversion type.");
        }
    }
}
