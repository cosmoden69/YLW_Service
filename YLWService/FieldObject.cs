using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YLWService
{
    public class FieldObject
    {
        public string FieldCode = "";
        public string FieldType = "";
        public object FieldValue = null;

        public FieldObject(string cd, string tp, object val)
        {
            this.FieldCode = cd;
            this.FieldType = tp;
            this.FieldValue = val;
        }
    }
}
