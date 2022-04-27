using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Data;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Script;
using System.Web.Script.Serialization;
using System.Windows.Forms;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YLWService.Extensions
{
    public static class CommonExtensions
    {
        public static JObject ToJson(this DataTable dt)
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = 2147483647;

            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;

            for (int ii = 0; ii < dt.Rows.Count; ii++)
            {
                DataRow dr = dt.Rows[ii];
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }

            JObject dataTable = new JObject();
            try
            {
                JArray jobj = JArray.Parse(serializer.Serialize(rows));
                dataTable.Add(dt.TableName, jobj);
            }
            catch { }

            return dataTable;
        }

        public static string ToJsonString(this DataTable dt)
        {
            return JsonConvert.SerializeObject(dt.ToJson());
        }

        public static JObject ToJson(this DataSet ds)
        {
            System.Web.Script.Serialization.JavaScriptSerializer serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            serializer.MaxJsonLength = 2147483647;

            Dictionary<string, List<Dictionary<string, object>>> dtList = new Dictionary<string, List<Dictionary<string, object>>>();
            List<Dictionary<string, object>> rows;
            Dictionary<string, object> row;

            foreach (DataTable dt in ds.Tables)
            {
                rows = new List<Dictionary<string, object>>();
                for (int ii = 0; ii < dt.Rows.Count; ii++)
                {
                    DataRow dr = dt.Rows[ii];
                    row = new Dictionary<string, object>();
                    foreach (DataColumn col in dt.Columns)
                    {
                        row.Add(col.ColumnName, dr[col]);
                    }
                    rows.Add(row);
                }
                dtList.Add(dt.TableName, rows);
            }

            JObject dataSet = new JObject();
            try
            {
                string str = serializer.Serialize(dtList);
                JObject jobj = JObject.Parse(str);
                dataSet.Add(ds.DataSetName, jobj);
            }
            catch { }

            return dataSet;
        }

        public static string ToJsonString(this DataSet ds)
        {
            return JsonConvert.SerializeObject(ds.ToJson());
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> list)
        {
            if (list == null) return null;

            Type type = typeof(T);
            var properties = type.GetProperties();

            DataTable dataTable = new DataTable();
            foreach (PropertyInfo info in properties)
            {
                dataTable.Columns.Add(new DataColumn(info.Name, Nullable.GetUnderlyingType(info.PropertyType) ?? info.PropertyType));
            }

            foreach (T entity in list)
            {
                object[] values = new object[properties.Length];
                for (int i = 0; i < properties.Length; i++)
                {
                    values[i] = properties[i].GetValue(entity);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public static void AddColumn(this DataGridView grv, string type, string name, string title, int width, bool visible = true, bool rdonly = false)
        {
            DataGridViewColumn dgvf;
            if (type.ToUpper() == "TEXTBOX")
            {
                dgvf = new DataGridViewTextBoxColumn();
                dgvf.DataPropertyName = name;
            }
            else if (type.ToUpper() == "BUTTON")
            {
                dgvf = new DataGridViewButtonColumn();
                dgvf.DataPropertyName = name;
            }
            else
            {
                dgvf = new DataGridViewTextBoxColumn();
                dgvf.DataPropertyName = name;
            }
            dgvf.HeaderText = title;
            dgvf.Name = name;
            dgvf.Width = width;
            dgvf.Visible = visible;
            dgvf.ReadOnly = rdonly;
            grv.Columns.Add(dgvf);
        }

        public static DataGridViewColumn GetPropertyColumn(this DataGridView dgv, string dataPropertyName)
        {
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (col.DataPropertyName == dataPropertyName) return col;
            }
            return null;
        }

        public static Uri AddQuery(this Uri uri, string name, string value)
        {
            string newUrl = uri.OriginalString;

            if (newUrl.EndsWith("&") || newUrl.EndsWith("?"))
                newUrl = string.Format("{0}{1}={2}", newUrl, name, value);
            else if (newUrl.Contains("?"))
                newUrl = string.Format("{0}&{1}={2}", newUrl, name, value);
            else
                newUrl = string.Format("{0}?{1}={2}", newUrl, name, value);

            return new Uri(newUrl);
        }

        public static TableRow GetRow(this Table oTable, int idx)
        {
            List<TableRow> rows = oTable.Elements<TableRow>().ToList();
            return rows?[idx];
        }

        public static TableCell GetCell(this TableRow oRow, int idx)
        {
            List<TableCell> cells = oRow.Elements<TableCell>().ToList();
            return cells?[idx];
        }

        public static Stream ToStream(this Image image, ImageFormat format)
        {
            var stream = new System.IO.MemoryStream();
            image.Save(stream, format);
            stream.Position = 0;
            return stream;
        }

        public static Stream ToStream(this string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream, Encoding.Default);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
