using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;

namespace YLWService
{
    public class YLW_Util
    {
        public const string DATAJSONKEY = "\"JSonData\":";
        public const string DATAJSONKEYVALUE = "\"JSonData\":null";
        //데이타타입
        public const int DataType_String = 0;
        public const int DataType_Int = 1;
        public const int DataType_Date = 2;
        public const int DataType_Bit = 3;
        public const int DataType_Long = 4;
        public const int DataType_Decimal = 5;

        public static string MsgToJs_string(string strMsg, string strData)
        {
            if (strData == string.Empty)
                return strMsg;

            int intIDX = strMsg.IndexOf(DATAJSONKEY);
            int intIDX2 = strMsg.IndexOf(DATAJSONKEYVALUE);

            if (intIDX < 0 || intIDX2 < 0)
            {
                return strMsg;
            }
            else
            {
                string a = strMsg.Substring(0, intIDX + DATAJSONKEY.Length);
                string b = strData;
                string c = strMsg.Substring(intIDX2 + DATAJSONKEYVALUE.Length, strMsg.Length - intIDX2 - DATAJSONKEYVALUE.Length);
                return a + b + c;
            }
        }

        public static string GetData<T>(object objData)
        {
            return GetData<T>(objData, -13, 0, 0, 0); // intDiffHour 이 -13 이면 서버 설정 시간 무시.
        }

        public static T GetData<T>(string strJson)
        {
            if (strJson == null || strJson == string.Empty)
                return default(T);
            else
                if (typeof(T) == typeof(DataSet))
                return JSonToDataSet<T>(strJson);
            else
                return JsonConvert.DeserializeObject<T>(strJson);
        }

        public static T JSonToDataSet<T>(string strJson)
        {
            DataSetJs dsMyData = JsonConvert.DeserializeObject<DataSetJs>(strJson);

            return (T)Convert.ChangeType(DsJsToDataSet(dsMyData), typeof(T));
        }

        public static string GetData<T>(object objData, int intDiffHour, int intDiffMinutes, int intComDiffHour, int intComDiffMinutes)
        {
            if (typeof(T) == typeof(DataSet))
            {
                if (objData == null)
                {
                    return "null";
                }
                else
                {
                    StringBuilder dsString = null;

                    //특수문자는 아래 함수에서 걸러낸다.
                    DSToJSon(objData as DataSet, ref dsString, intDiffHour, intDiffMinutes, intComDiffHour, intComDiffMinutes);


                    //'\\'는 변환시 오히려 오류 발생됨..("1\" 와 같은 문자열의 경우)
                    // \', \"는 변환 필요없음
                    //dsString = dsString.Replace("\n", "\\n").Replace("\\", "\\\\");
                    //.Replace("<", "&lt;").Replace(">", "&gt;").Replace("((?<!\\\\)(\\\\\\\\)*)(\\\\\\\")", "$1&quot;")
                    //.Replace("&", "&amp;").Replace("'", "&#x27;").Replace("/", "&#x2F;");

                    return dsString.ToString();
                }
            }
            else if (typeof(T) == typeof(DataTable))
            {
                if (objData == null)
                {
                    return "null";
                }
                else
                {
                    StringBuilder dsString = null;
                    //특수문자는 아래 함수에서 걸러낸다.
                    DTToDSJSon(objData as DataTable, ref dsString, intDiffHour, intDiffMinutes, intComDiffHour, intComDiffMinutes);


                    //dsString.Replace("\n", "\\n");

                    return dsString.ToString();
                }
            }
            else
            {
                return JsonConvert.SerializeObject(objData);
            }

        }

        public static void DSToJSon(DataSet dsData, ref StringBuilder dsString, int intDiffHour, int intDiffMinutes, int intComDiffHour, int intComDiffMinutes)
        {
            //널값 체크
            if (dsData == null) { return; }

            //스키마 0번은 테이블정보 0~n 은 컬럼정보 테이블이 있다.
            if (dsString == null)
                dsString = new StringBuilder();

            dsString.Append("{ \"Tables\":[");

            for (int t = 0; t < dsData.Tables.Count; t++)
            {
                DTToJSon(dsData.Tables[t], ref dsString, intDiffHour, intDiffMinutes, intComDiffHour, intComDiffMinutes);

                //다음 테이블 체크
                if (t < dsData.Tables.Count - 1)
                    dsString.Append(",");

            }

            dsString.Append("]}"); //, Tables]

        }

        public static void DTToDSJSon(DataTable dtData, ref StringBuilder dsString, int intDiffHour, int intDiffMinutes, int intComDiffHour, int intComDiffMinutes)
        {
            //널값 체크
            if (dtData == null) { return; }

            //스키마 0번은 테이블정보 0~n 은 컬럼정보 테이블이 있다.
            if (dsString == null)
                dsString = new StringBuilder();

            dsString.Append("{ \"Tables\":[");

            DTToJSon(dtData, ref dsString);

            dsString.Append("]}"); //, Tables]

        }

        public static void DTToDSJSon(DataTable dtData, ref StringBuilder dsString)
        {
            DTToDSJSon(dtData, ref dsString, -13, 0, 0, 0);    // intDiffHour 이 -13 이면 서버 설정 시간 무시.
        }

        public static bool DTToJSon(DataTable dtData, ref StringBuilder dsString, int intDiffHour, int intDiffMinutes, int intComDiffHour, int intComDiffMinutes)
        {
            if (dtData == null) return false;

            int intColCount = dtData.Columns.Count;
            DTToJSon(dtData, intColCount, ref dsString, intDiffHour, intDiffMinutes, intComDiffHour, intComDiffMinutes);

            return true;
        }

        public static bool DTToJSon(DataTable dtData, ref StringBuilder dsString)
        {
            return DTToJSon(dtData, ref dsString, -13, 0, 0, 0);   // intDiffHour 이 -13 이면 서버 설정 시간 무시.
        }

        public static void DTToJSon(DataTable dtData, int intColCount, ref StringBuilder dsString, int intDiffHour, int intDiffMinutes, int intComDiffHour, int intComDiffMinutes)
        {
            //Table schema생성
            dsString.Append("{\"TableName\":\"" + dtData.TableName + "\","); //테이블 데이타
                                                                             //dsString.Append("\"Columns\":["); //컬럼정보
            if (dtData.Columns.Contains("_ProgramTimeLogString"))
            {
                intColCount -= 1;
            }

            string[] dataName = new string[intColCount];
            byte[] dataType = new byte[dataName.Length];

            int intTimeLogIndex = -1;

            //Column Schema생성
            for (int c = 0, col = 0; c < dtData.Columns.Count; c++)
            {
                if (dtData.Columns[c].ColumnName == "_ProgramTimeLogString")
                {
                    intTimeLogIndex = c;
                    continue;
                }

                //작업데이타
                dataName[col] = "\"" + dtData.Columns[c].ColumnName + "\"";
                dataType[col] = TypeNameToDevType(dtData.Columns[c].DataType.Name.ToLower());
                col++;
            }

            dsString.Append("\"Columns\":[" + String.Join(",", dataName) + "],");
            dsString.Append("\"ColumnsType\":[" + String.Join(",", dataType) + "],");

            dsString.Append("\"Rows\":[");//로우 데이타 시작
                                          //Data Schema생성

            #region *rows start
            for (int r = 0; r < dtData.Rows.Count; r++)
            {
                dsString.Append("[");

                //Add Column
                for (int c = 0; c < dtData.Columns.Count; c++)
                {
                    if (intTimeLogIndex == c) continue;

                    if (dataType[c] == DataType_String)
                    {
                        dsString.Append("\"" + dtData.Rows[r][c].ToString().Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n") + "\""); //문자
                    }
                    else if (dataType[c] == DataType_Int || dataType[c] == DataType_Long)
                    {
                        if (dtData.Rows[r][c].ToString() == string.Empty)
                            dsString.Append(0);
                        else
                            dsString.Append(dtData.Rows[r][c].ToString());
                    }
                    else if (dataType[c] == DataType_Decimal)
                    {
                        dsString.Append("\"" + dtData.Rows[r][c].ToString().Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n") + "\""); //문자
                    }
                    else if (dataType[c] == DataType_Date && dtData.Rows[r][c].ToString() != string.Empty)
                    {
                        DateTime dt = (DateTime)dtData.Rows[r][c];
                        DateTime newDt = dt;

                        // ServerConfig에서 가져온 서버 시간을 적용할 것 
                        if (intDiffHour != -13)
                        //if (Global.TimeDifference_Hour > 0 || Global.TimeDifference_Minute > 0)
                        {
                            newDt = (dt + TimeSpan.FromHours(intDiffHour)) + TimeSpan.FromMinutes(intDiffMinutes);
                        }

                        // CompanyConfig에서 가져온 서버 시간을 적용할 것 :: 롤백요청 (20180827 박세훈님)
                        // newDt = (newDt + TimeSpan.FromHours(intComDiffHour)) + TimeSpan.FromMinutes(intComDiffMinutes);

                        dsString.Append("\"" + ((DateTime)newDt).ToString("yyyy-MM-dd HH:mm:ss") + "\""); //날짜데이타
                    }
                    else if (dataType[c] == DataType_Bit) //bit
                    {
                        dsString.Append(dtData.Rows[r][c].ToString() == "True" ? "1" : "0");
                    }
                    else //string으로 처리한다.
                    {
                        dsString.Append("\"" + dtData.Rows[r][c].ToString().Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\t", "\\t").Replace("\r", "\\r").Replace("\n", "\\n") + "\""); //기타
                    }

                    //마지막이 아니면 ,를 추가한다.
                    if (c < intColCount - 1)
                        dsString.Append(",");
                }

                //Add Row
                dsString.Append("]");

                //마지막이 아니면 ,를 추가한다.
                if (r < dtData.Rows.Count - 1)
                    dsString.Append(",");

            }
            #endregion

            dsString.Append("]}"); //Rows], Table}

            dataType = null;
        }

        #region *[TypeNameToDevType] 서비스 유형
        public static byte TypeNameToDevType(string strType)
        {
            /* SQL ColumnType
             bigint     binary      bit     char            datetime    datetime2   datetimeoffset  decimal float   image               int         nchar
             numeric    nvarchar    real    smalldatetime   smallint    sql_variant sysname         text    tinyint uniqueidentifier    varbinary   varchar     xml
             */
            if (strType == "nvarchar" || strType == "varchar" || strType == "nchar" || strType == "char")
                return DataType_String;
            else if (strType == "int" || strType == "int32")//strType == "int64") //트리에서 오류났어서 수정함.
                return DataType_Int;
            else if (strType == "numeric")
                return DataType_Long;
            else if (strType == "decimal")
                return DataType_Decimal;
            else if (strType == "datetime")
                return DataType_Date;
            else if (strType == "bit")
                return DataType_Bit;
            else
                return DataType_String;
        }

        private string TypeToDevType(Type type)
        {
            if (type == typeof(string))
                return DataType_String.ToString();
            else if (type == typeof(int))
                return DataType_Int.ToString();
            else if (type == typeof(decimal))
                return DataType_Int.ToString();
            else if (type == typeof(long))
                return DataType_Long.ToString();
            else if (type == typeof(DateTime))
                return DataType_Date.ToString();
            else
                return DataType_String.ToString();
        }

        private Type DevTypeToType(string strType)
        {
            if (strType == DataType_String.ToString())
                return typeof(string);
            else if (strType == DataType_Int.ToString())
                return typeof(int);
            else if (strType == DataType_Decimal.ToString())
                return typeof(decimal);
            else if (strType == DataType_Long.ToString())
                return typeof(long);
            else if (strType == DataType_Date.ToString())
                return typeof(DateTime);
            else
                return typeof(string);
        }

        #endregion




        public class DataSetJs
        {
            public DataTableJs[] Tables = null;
        }
        public class DataTableJs
        {
            public string TableName = null;
            public string[] Columns = null;
            public int[] ColumnsType = null;
            public string[][] Rows = null;
        }

        public static DataSet DsJsToDataSet(DataSetJs dsMyData)
        {
            return DsJsToDataSet(dsMyData, false);
        }

        public static DataSet DsJsToDataSet(DataSetJs dsMyData, bool isKWF)
        {
            if (dsMyData == null)
                return null;

            DataSet dsData = new DataSet();

            foreach (DataTableJs dtMyData in dsMyData.Tables)
            {
                System.Data.DataTable dtData = new System.Data.DataTable(dtMyData.TableName);

                for (int c = 0; c < dtMyData.Columns.Length; c++)
                {
                    dtData.Columns.Add(dtMyData.Columns[c], YLW_Util.GetType(dtMyData.ColumnsType[c]));
                }

                foreach (string[] row in dtMyData.Rows)
                {
                    //아래 주석 코드는 kuiHtml에서 해결해야함.
                    //dtData.Rows.Add(isKWF == true ? Util.ParsingSpecialforXml(row) : row);
                    dtData.Rows.Add(row);
                }

                dsData.Tables.Add(dtData);
            }

            return dsData;
        }

        public static Type GetType(int intJSonType, bool isExcelRpt)
        {
            if (isExcelRpt && intJSonType == DataType_Decimal) return typeof(decimal);

            switch (intJSonType)
            {
                case DataType_String:
                    return typeof(string);

                // int --> long 형으로 변경 (차기이월처리 : 금액관련해서 data overflow) 
                case DataType_Int:
                    return typeof(int);

                case DataType_Long:
                    return typeof(long);

                case DataType_Date:
                    return typeof(DateTime);

                case DataType_Bit:
                    return typeof(Boolean);
                default:
                    return typeof(string);
            }
        }

        public static Type GetType(int intJSonType)
        {
            return GetType(intJSonType, false);
        }

        public static int GetType(Type type)
        {
            if (type == typeof(string))
                return DataType_String;
            else if (type == typeof(int))
                return DataType_Int;

            else if (type == typeof(DateTime))
                return DataType_Date;

            else if (type == typeof(Boolean))
                return DataType_Bit;
            else
                return DataType_String;
        }
    }
}
