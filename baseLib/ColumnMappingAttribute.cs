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
        public string gateNo_in {get;set; }    
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ColumnMappingAttribute:Attribute
    {
        public string ColumnName { get; set; }
        public ColumnMappingAttribute() { 
        }
        public ColumnMappingAttribute(string columnName)
        {
            ColumnName = columnName;
        }
    }
}
