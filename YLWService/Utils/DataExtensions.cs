using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetroSoft.HIS.Utils;

namespace MetroSoft.HIS.Extensions
{
    public static class DataExtensions
    {
        public static bool HasCellChanged(this DataRow row, string colnm)
        {
            if (!row.HasVersion(DataRowVersion.Original))
            {
                // Row has been added. All columns have changed. 
                return true;
            }
            if (!row.HasVersion(DataRowVersion.Current))
            {
                // Row has been removed. No columns have changed.
                return false;
            }
            var originalVersion = row[colnm, DataRowVersion.Original];
            var currentVersion = row[colnm, DataRowVersion.Current];
            if (originalVersion == DBNull.Value && currentVersion == DBNull.Value)
            {
                return false;
            }
            else if (originalVersion != DBNull.Value && currentVersion != DBNull.Value)
            {
                return !originalVersion.Equals(currentVersion);
            }
            return true;
        }

        public enum DataTableMoveRowDirection
        {
            Up = 0,
            Down = 1
        }

        /// <summary>
        /// DataColumn의 DataType과 일치하는 SqlDbType을 리턴합니다.
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static SqlDbType GetSqlDbType(this DataColumn col)
        {
            return SqlUtil.ToSqlDbType(col.DataType);
        }

        public static int Index(this DataRow dr)
        {
            if (dr == null || dr.Table.Rows.Count < 1) return -1;
            return dr.Table.Rows.IndexOf(dr);
        }

        public static object GetField(this DataRow dr, string fld)
        {
            if (dr == null || dr.Table.Columns.Contains(fld) == false) return null;
            return dr[fld];
        }

        public static long GetLong(this DataRow dr, string fld)
        {
            return cUtil.ConvertToLong(dr[fld]);
        }

        public static long GetLong(this DataRow dr, int idx)
        {
            return cUtil.ConvertToLong(dr[idx]);
        }

        public static float GetFloat(this DataRow dr, string fld)
        {
            return cUtil.ConvertToFloat(dr[fld]);
        }

        public static float GetFloat(this DataRow dr, int idx)
        {
            return cUtil.ConvertToFloat(dr[idx]);
        }

        public static double GetDouble(this DataRow dr, string fld)
        {
            return cUtil.ConvertToDouble(dr[fld]);
        }

        public static double GetDouble(this DataRow dr, int idx)
        {
            return cUtil.ConvertToDouble(dr[idx]);
        }

        public static decimal GetDecimal(this DataRow dr, string fld)
        {
            return cUtil.ConvertToDecimal(dr[fld]);
        }

        public static decimal GetDecimal(this DataRow dr, int idx)
        {
            return cUtil.ConvertToDecimal(dr[idx]);
        }

        public static string GetString(this DataRow dr, string fld)
        {
            return cUtil.ConvertToString(dr[fld]);
        }

        public static string GetString(this DataRow dr, int idx)
        {
            return cUtil.ConvertToString(dr[idx]);
        }

        public static DataRow GetNextRow(this DataRow dr)
        {
            if (dr == null) return null;
            int idx = dr.Table.Rows.IndexOf(dr);
            idx++;
            if (idx >= 0 && idx <= dr.Table.Rows.Count - 1) return dr.Table.Rows[idx];
            else return null;
        }

        public static DataRow GetPreviousRow(this DataRow dr)
        {
            if (dr == null) return null;
            int idx = dr.Table.Rows.IndexOf(dr);
            idx--;
            if (idx >= 0 && idx <= dr.Table.Rows.Count - 1) return dr.Table.Rows[idx];
            else return null;
        }

        public static DataRow GetFirstRow(this DataTable dt)
        {
            if (dt == null || dt.Rows.Count < 1) return null;
            return dt.Rows[0];
        }

        public static DataRow GetLastRow(this DataTable dt)
        {
            if (dt == null || dt.Rows.Count < 1) return null;
            return dt.Rows[dt.Rows.Count - 1];
        }

        public static bool IsNull(this DataRow dr)
        {
            if (dr == null) return true;
            return false;
        }

        public static int FieldCount(this DataTable dt)
        {
            if (dt == null) return 0;
            return dt.Columns.Count;
        }

        public static object GetFieldFirst(this DataRow[] rows, string fld)
        {
            if (rows == null || rows.Length < 1) return null;
            if (rows[0].Table.Columns.Contains(fld) == false) return null;
            return rows[0][fld];
        }

        public static int RecordCount(this DataTable dt)
        {
            if (dt == null) return 0;
            return dt.Rows.Count;
        }

        public static object ReadTableXY(this DataRow dr, int col, int row)
        {
            return ReadTableXY(dr.Table, col, row);
        }

        public static object ReadTableXY(this DataRow dr, string col, int row)
        {
            return ReadTableXY(dr.Table, col, row);
        }

        public static object ReadTableXY(this DataTable dt, int col, int row)
        {
            if (dt == null) return null;
            try
            {
                return dt.Rows[row][col];
            }
            catch
            {
                return null;
            }
        }

        public static object ReadTableXY(this DataTable dt, string col, int row)
        {
            if (dt == null) return null;
            try
            {
                return dt.Rows[row][col];
            }
            catch
            {
                return null;
            }
        }

        public static string GetFieldString(this DataRow dr, string colDelimeter)
        {
            if (dr == null) return "";
            string ret = "";
            for (int jj = 0; jj < dr.Table.Columns.Count; jj++)
            {
                ret = ret + dr[jj] + colDelimeter;
            }
            return ret;
        }

        public static string GetRecordString(this DataTable dt, string colDelimeter, string rowDelimeter)
        {
            if (dt == null) return "";
            string ret = "";
            for (int ii = 0; ii < dt.Rows.Count; ii++)
            {
                for (int jj = 0; jj < dt.Columns.Count; jj++)
                {
                    ret = ret + dt.Rows[ii][jj] + colDelimeter;
                }
                ret = ret + rowDelimeter;
            }
            return ret;
        }

        public static int MoveRow(this DataTable dt, DataRow row, DataTableMoveRowDirection direction)
        {
            DataRow oldRow = row;
            DataRow newRow = dt.NewRow();

            if (oldRow.RowState == DataRowState.Deleted) oldRow.RejectChanges();
            newRow.ItemArray = oldRow.ItemArray;

            int oldRowIndex = dt.Rows.IndexOf(row);

            if (direction == DataTableMoveRowDirection.Down)
            {
                int newRowIndex = oldRowIndex + 1;

                if (oldRowIndex < (dt.Rows.Count))
                {
                    dt.Rows.Remove(oldRow);
                    dt.Rows.InsertAt(newRow, newRowIndex);
                    return dt.Rows.IndexOf(newRow);
                }
            }

            if (direction == DataTableMoveRowDirection.Up)
            {
                int newRowIndex = oldRowIndex - 1;

                if (oldRowIndex > 0)
                {
                    dt.Rows.Remove(oldRow);
                    dt.Rows.InsertAt(newRow, newRowIndex);
                    return dt.Rows.IndexOf(newRow);
                }
            }

            return 0;
        }

        public static DataTable JoinTables(this DataTable table, DataTable joinTable)
        {
            DataTable resultTable = new DataTable();
            var innerTableColumns = new List<string>();
            foreach (DataColumn column in table.Columns)
            {
                innerTableColumns.Add(column.ColumnName);
                resultTable.Columns.Add(column.ColumnName);
            }

            var outerTableColumns = new List<string>();
            foreach (DataColumn column in joinTable.Columns)
            {
                if (!innerTableColumns.Contains(column.ColumnName))
                {
                    outerTableColumns.Add(column.ColumnName);
                    resultTable.Columns.Add(column.ColumnName);
                }
            }

            for (int i = 0; i < table.Rows.Count; i++)
            {
                var row = resultTable.NewRow();
                innerTableColumns.ForEach(x =>
                {
                    row[x] = table.Rows[i][x];
                });
                outerTableColumns.ForEach(x =>
                {
                    row[x] = joinTable.Rows[i][x];
                });
                resultTable.Rows.Add(row);
            }
            return resultTable;
        }

        //bException == true 이면 psparam 가 합계 제외 필드임
        public static void AddSummaryDataRow(this DataTable pdt, string title_col, string title, string psparam, bool bException = false)
        {
            string[] param = psparam.Split(';');
            DataRow dr = pdt.NewRow();
            for (int col = 0; col < pdt.Columns.Count; col++)
            {
                if (param.Contains<string>(pdt.Columns[col].ColumnName) == bException)
                    dr[col] = DBNull.Value;
                else
                {
                    dr[col] = pdt.Compute("SUM([" + pdt.Columns[col].ColumnName + "])", "");
                }
            }
            dr[pdt.Columns[title_col].Ordinal] = title;
            pdt.Rows.Add(dr);
        }

        public static void CopyToItemArray(this DataRow pdr, DataRow drT)
        {
            foreach (DataColumn col in pdr.Table.Columns)
            {
                if (drT.Table.Columns.Contains(col.ColumnName)) drT[col.ColumnName] = pdr[col.ColumnName];
            }
        }
    }
}
