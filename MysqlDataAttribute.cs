using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ziedden.Mysql
{
    [AttributeUsage(AttributeTargets.Field,AllowMultiple = true)]
    public class MysqlDataAttribute : Attribute
    {
        public string FieldName;
        public MysqlDataAttribute(string FieldName = null)
        {
            this.FieldName = FieldName;
        }

    }
}
