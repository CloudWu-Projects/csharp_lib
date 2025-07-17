using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wu_jiaxing20220115
{
    class Template
    {
        [ColumnMapping("incodenumber")]
        public string gateNo_in { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ColumnMappingAttribute : Attribute
    {
        public string ColumnName { get; }
        public int Order { get; }

        public ColumnMappingAttribute(string columnName)
        : this(columnName, -1)
        {
        }

        public ColumnMappingAttribute(int order)
        : this(null, order)
        {
            
        }
        public ColumnMappingAttribute(string columnName, int order)
        {
            ColumnName = columnName;
            Order = order;
        }
    }
}
