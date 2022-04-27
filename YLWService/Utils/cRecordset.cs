using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MetroSoft.HIS.Extensions;

namespace MetroSoft.HIS
{
    public class cRecordset
    {
        private DataTable _dt = null;
        private int _index = -1;

        public int Index { get { return this._index; } }

        public object this[string fieldname]
        {
            get
            {
                if (_dt == null || _dt.Rows.Count < 1) return "";
                if (this._index < 0 || this._index > _dt.Rows.Count - 1) return "";
                return this._dt.Rows[this._index][fieldname];
            }
        }

        public cRecordset(DataTable dt)
        {
            this._dt = dt;
            MoveFirst();
        }

        public int FieldCount()
        {
            if (_dt == null) return 0;
            return _dt.Columns.Count;
        }

        public int RecordCount()
        {
            if (_dt == null) return 0;
            return _dt.Rows.Count;
        }

        public void Close()
        {
            if (_dt == null) return;
            this._dt.Dispose();
        }

        public void CloseRecordset()
        {
            if (_dt == null) return;
            this._dt.Dispose();
        }        

        public bool MoveNext()
        {
            if (_dt == null || _dt.Rows.Count < 1) return false;
            if (this._index > _dt.Rows.Count - 1) return false;
            this._index++;
            return true;
        }

        public bool MovePrevious()
        {
            if (_dt == null || _dt.Rows.Count < 1) return false;
            if (this._index < 0) return false;
            this._index--;
            return true;
        }

        public bool MoveLast()
        {
            if (_dt == null || _dt.Rows.Count < 1) return false;
            this._index = _dt.Rows.Count - 1;
            return true;
        }

        public bool MoveFirst()
        {
            if (_dt == null || _dt.Rows.Count < 1) return false;
            this._index = 0;
            return true;
        }

        public bool MoveTo(int index)
        {
            if (_dt == null || _dt.Rows.Count < 1) return false;
            if (index < 0 || index > _dt.Rows.Count - 1) return false;
            this._index = index;
            return true;
        }

        public bool MoveTo(DataRow pdr)
        {
            if (_dt == null || _dt.Rows.Count < 1) return false;
            if (pdr == null) return false;
            int index = _dt.Rows.IndexOf(pdr);
            if (index < 0 || index > _dt.Rows.Count - 1) return false;
            this._index = index;
            return true;
        }

        public bool IsEOF()
        {
            if (_dt == null || _dt.Rows.Count < 1) return true;
            if (this._index < 0 || this._index > _dt.Rows.Count - 1) return true;
            return false;
        }

        public object Fields(int fieldIndex)
        {
            if (_dt == null || _dt.Rows.Count < 1) return "";
            if (this._index < 0 || this._index > _dt.Rows.Count - 1) return "";
            return this._dt.Rows[this._index][fieldIndex];
        }

        public object Fields(string fieldname)
        {
            if (_dt == null || _dt.Rows.Count < 1) return "";
            if (this._index < 0 || this._index > _dt.Rows.Count - 1) return "";
            return this._dt.Rows[this._index][fieldname];
        }

        public string GetFieldName(int fieldIndex)
        {
            if (_dt == null) return "";
            if (fieldIndex < 0 || fieldIndex > this._dt.Columns.Count - 1) return "";
            return this._dt.Columns[fieldIndex].ColumnName;
        }

        public string GetString(string fieldname)
        {
            if (_dt == null || _dt.Rows.Count < 1) return "";
            if (this._index < 0 || this._index > _dt.Rows.Count - 1) return "";
            return this._dt.Rows[this._index].GetString(fieldname);
        }

        public string GetString(int idx)
        {
            if (_dt == null || _dt.Rows.Count < 1) return "";
            if (this._index < 0 || this._index > _dt.Rows.Count - 1) return "";
            return this._dt.Rows[this._index].GetString(idx);
        }

        public long GetLong(string fieldname)
        {
            if (_dt == null || _dt.Rows.Count < 1) return 0;
            if (this._index < 0 || this._index > _dt.Rows.Count - 1) return 0;
            return this._dt.Rows[this._index].GetLong(fieldname);
        }

        public long GetLong(int idx)
        {
            if (_dt == null || _dt.Rows.Count < 1) return 0;
            if (this._index < 0 || this._index > _dt.Rows.Count - 1) return 0;
            return this._dt.Rows[this._index].GetLong(idx);
        }

        public float GetFloat(string fieldname)
        {
            if (_dt == null || _dt.Rows.Count < 1) return 0;
            if (this._index < 0 || this._index > _dt.Rows.Count - 1) return 0;
            return this._dt.Rows[this._index].GetFloat(fieldname);
        }

        public float GetFloat(int idx)
        {
            if (_dt == null || _dt.Rows.Count < 1) return 0;
            if (this._index < 0 || this._index > _dt.Rows.Count - 1) return 0;
            return this._dt.Rows[this._index].GetFloat(idx);
        }

        public double GetDouble(string fieldname)
        {
            if (_dt == null || _dt.Rows.Count < 1) return 0;
            if (this._index < 0 || this._index > _dt.Rows.Count - 1) return 0;
            return this._dt.Rows[this._index].GetDouble(fieldname);
        }

        public double GetDouble(int idx)
        {
            if (_dt == null || _dt.Rows.Count < 1) return 0;
            if (this._index < 0 || this._index > _dt.Rows.Count - 1) return 0;
            return this._dt.Rows[this._index].GetDouble(idx);
        }

        public decimal GetDecimal(string fieldname)
        {
            if (_dt == null || _dt.Rows.Count < 1) return 0;
            if (this._index < 0 || this._index > _dt.Rows.Count - 1) return 0;
            return this._dt.Rows[this._index].GetDecimal(fieldname);
        }

        public decimal GetDecimal(int idx)
        {
            if (_dt == null || _dt.Rows.Count < 1) return 0;
            if (this._index < 0 || this._index > _dt.Rows.Count - 1) return 0;
            return this._dt.Rows[this._index].GetDecimal(idx);
        }

        public string GetFieldString(string colDelimeter)
        {
            if (_dt == null || _dt.Rows.Count < 1) return "";
            if (this._index < 0 || this._index > _dt.Rows.Count - 1) return "";
            return this._dt.Rows[this._index].GetFieldString(colDelimeter);
        }

        public string GetRecordString(string colDelimeter, string rowDelimeter)
        {
            if (_dt == null || _dt.Rows.Count < 1) return "";
            return this._dt.GetRecordString(colDelimeter, rowDelimeter);
        }

        public DataRow GetDataRow()
        {
            if (_dt == null || _dt.Rows.Count < 1) return null;
            if (this._index < 0 || this._index > _dt.Rows.Count - 1) return null;
            return this._dt.Rows[this._index];
        }

        public DataTable GetDataTable()
        {
            return this._dt;
        }
    }
}
