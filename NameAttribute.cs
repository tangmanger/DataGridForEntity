using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataGridForEntity
{
    [AttributeUsage(AttributeTargets.All)]
    public sealed class NameAttribute : Attribute
    {
        private readonly string _Attributes;
        public  NameAttribute()
        {

        }
        public  string Attributes
        {
            get { return _Attributes; }
        }

        public  NameAttribute(string name)
        {
            _Attributes = name;
        }
    }
}
