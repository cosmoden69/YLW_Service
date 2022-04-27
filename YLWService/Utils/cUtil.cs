/*
 * Created by SharpDevelop.
 * User: hiworld
 * Date: 2012-02-23
 * Time: 오전 9:48
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using System.Windows.Forms;
using System.Reflection;
using Microsoft.VisualBasic;
using System.Globalization;
using System.Security.Principal;

namespace MetroSoft.HIS
{
    /// <summary>
    /// Message 유형 열거형입니다.
    /// </summary>
    public enum MsgType
    {
        /// <summary>
        /// 정상 Message입니다.
        /// </summary>
        Normal,
        /// <summary>
        /// 오류 Message입니다.
        /// </summary>
        Error
    }

    public enum ServiceType
    {
        Entry,  //저장
        Query,  //조회
        Validation  //Vaidation Check
    }
    public enum ServiceMsgType
    {
        Processing, //처리중
        Normal,     //정상
        NoData,     //데이타없음
        ContQry     //연속조회중
    }

    /// <summary>
	/// Description of cUtil.
	/// </summary>
	public class cUtil
    {
        public static string D_LEV1 = Convert.ToString((char)21);
        public static string D_LEV2 = Convert.ToString((char)22);
        public static string D_LEV3 = Convert.ToString((char)23);
        public static string D_LEV4 = Convert.ToString((char)24);
        public static string D_LEV5 = Convert.ToString((char)25);
        public static string vbCrLf = Environment.NewLine;
        public static string vbTab = "\t";

        // TA16 [상병MASTER] Table의  DISEDIV(상병구분) 정의
        public const string HOS_DX_DIV = "2";  // 1 : ICD9 CODE 체계, 2 : ICD10 CODE 체계

        private static ArrayList bindVarNameList = new ArrayList();

        /// <summary>
        /// SQL문에서 Bind Variables Collection 초기화
        /// </summary>
        /// <param name="sqlText"> SQL 문장 </param>
        /// <param name="bindVarList"> binding 정보 Collection </param>
        [Description("SQL문에서 Bind Variables를 Return합니다.")]
        public static void InitBindVarList(string sqlText, BindVarCollection bindVarList)
        {
            InitBindVarList(sqlText, bindVarList, "@");
        }

        public static void InitBindVarList(string sqlText, BindVarCollection bindVarList, string symbol)
        {
            //기존 Data Clear
            bindVarList.Clear();

            //BindVariableName List SET
            SetBindVarNameList(sqlText, symbol);

            // 이름으로 Unique하게 List작성
            foreach (string varName in bindVarNameList)
            {
                if (!bindVarList.Contains(varName))
                    bindVarList.Add(varName);
            }
        }

        private static void SetBindVarNameList(string sqlText, string symbol)
        {
            int pos = -1;
            int startIndex = 0;
            int index = startIndex;
            int cmtPos;
            bool loopExit = false;

            bindVarNameList.Clear();
            //  Bind 변수의 판단을 BindSymbol로 통일 (Oracle -> :, SqlServer -> @, DB2 -> @)
            while (((pos = sqlText.IndexOf(symbol, index)) >= 0) && !loopExit)
            {
                index = pos + 1;
                // 앞의 Character가 separator인지 여부 확인
                if (pos > 0)
                    if (!IsDelimiter(sqlText[pos - 1])) continue;
                // Comment인지 여부 확인
                string chkStmt = sqlText.Substring(startIndex, pos - startIndex);
                if ((cmtPos = chkStmt.LastIndexOf("/*")) >= 0)
                    if (chkStmt.IndexOf("*/", cmtPos + 2) < 0) continue;
                if ((cmtPos = chkStmt.LastIndexOf("--")) >= 0)
                    if (chkStmt.IndexOf("\n", cmtPos + 2) < 0) continue;
                if ((StrContains(chkStmt, "\'", 0) % 2) >= 1) continue;
                loopExit = true;
                for (int i = pos + 1; i <= sqlText.Length; i++)
                {
                    if (i == sqlText.Length)
                    {
                        string varName = sqlText.Substring(pos + 1);
                        if (varName == "")
                            varName = "Var" + (bindVarNameList.Count + 1).ToString();
                        bindVarNameList.Add(varName);
                        loopExit = true;
                        break;
                    }
                    else if (IsDelimiter(sqlText[i]))
                    {
                        string varName = sqlText.Substring(pos + 1, i - pos - 1);
                        if (varName == "")
                            varName = "Var" + (bindVarNameList.Count + 1).ToString();
                        bindVarNameList.Add(varName);
                        startIndex = i;
                        index = startIndex;
                        loopExit = false;
                        break;
                    }
                }
            }
        }

        /// <summary>
        ///  String에서 target문자열이 포함된 회수를 Return한다.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        private static int StrContains(string source, string target, int startIndex)
        {
            int count = 0, pos;
            int index = startIndex;
            while ((pos = source.IndexOf(target, index)) >= 0)
            {
                index = pos + target.Length;
                count++;
            }
            return count;
        }

        [Description("문자가 Delimiter인지 여부를 Return합니다.")]
        private static bool IsDelimiter(char ch)
        {
            return !((char.IsLetterOrDigit(ch)) || (ch == '_'));
        }

        [Description("Split 을 char가 아닌 string으로 구분합니다.")]
        public static string[] SplitByString(string testString, string split)
        {
            int offset = 0;
            int index = 0;
            int[] offsets = new int[testString.Length + 1];
            while (index < testString.Length)
            {
                int indexOf = testString.IndexOf(split, index);
                if (indexOf != -1)
                {
                    offsets[offset++] = indexOf;
                    index = (indexOf + split.Length);
                }
                else
                {
                    index = testString.Length;
                }
            }
            string[] final = new string[offset + 1];
            if (offset == 0)
            {
                final[0] = testString;
            }
            else
            {
                offset--;
                final[0] = testString.Substring(0, offsets[0]);
                for (int i = 0; i < offset; i++)
                {
                    final[i + 1] = testString.Substring(offsets[i] + split.Length, offsets[i + 1] - offsets[i] - split.Length);
                }
                final[offset + 1] = testString.Substring(offsets[offset] + split.Length);
            }
            return final;
        }

        public static int IndexOfAny(string test, string[] values)
        {
            int first = -1;
            foreach (string item in values)
            {
                int i = test.IndexOf(item);
                if (i >= 0)
                {
                    if (first >= 0)
                    {
                        if (i < first)
                        {
                            first = i;
                        }
                    }
                    else
                    {
                        first = i;
                    }
                }
            }
            return first;
        }

        public static int LastIndexOfAny(string test, string[] values)
        {
            int last = -1;
            foreach (string item in values)
            {
                int i = test.LastIndexOf(item);
                if (i >= 0)
                {
                    if (last >= 0)
                    {
                        if (i > last)
                        {
                            last = i;
                        }
                    }
                    else
                    {
                        last = i;
                    }
                }
            }
            return last;
        }

        public static string IndexItemOfAny(string test, string[] values, int index)
        {
            foreach (string item in values)
            {
                if (item == test.Substring(index, item.Length)) return item;
            }
            return "";
        }

        public static string GetSetting(string AppName, string Section, string Key, string Default)
        {
            return Interaction.GetSetting(AppName, Section, Key, Default);
        }

        public static void SaveSetting(string AppName, string Section, string Key, bool Setting)
        {
            SaveSetting(AppName, Section, Key, (Setting ? "1" : "0"));
        }

        public static void SaveSetting(string AppName, string Section, string Key, string Setting)
        {
            Interaction.SaveSetting(AppName, Section, Key, Setting);
        }

        public static string XmlTransform(XmlDocument doc, string xsltPath)
        {
            XslCompiledTransform xslDoc = new XslCompiledTransform();
            xslDoc.Load(xsltPath);
            StringWriter sw = new StringWriter();
            xslDoc.Transform(doc, null, sw);

            return sw.ToString();
        }

        public static string XmlTransform(string strXml, string xsltPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(strXml);

            XslCompiledTransform xslDoc = new XslCompiledTransform();
            xslDoc.Load(xsltPath);
            StringWriter sw = new StringWriter();
            xslDoc.Transform(doc, null, sw);

            return sw.ToString();
        }

        public static XmlDocument LoadXml(string strXmlPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(strXmlPath);

            return doc;
        }

        public static string GetXmlAttributeValue(XmlDocument doc, string nodename, string key)
        {
            try
            {
                //지정한 Element의 Attribute 의 값 Get
                XmlNodeList nodelist = doc.GetElementsByTagName(nodename);
                if (nodelist == null) return "";
                foreach (XmlNode node in nodelist)
                {
                    if (node.Name == nodename && node.Attributes[key] != null)
                        return node.Attributes[key].Value;
                }
            }
            catch { }

            return "";
        }

        public static string XmlToSave(string strXml)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(strXml);

            sb.Replace("&", "&amp;");
            sb.Replace("<", "&lt;");
            sb.Replace(">", "&gt;");
            sb.Replace("'", "&#40;");
            sb.Replace(" ", "&#160;"); //&nbsp;

            return sb.ToString();
        }

        public static string GetXmlToSave(XmlDocument xml, string frm)
        {
            string sx = "";

            try
            {
                string mypath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                string indentf = mypath + @"\indent.xsl";

                sx = XmlTransform(xml, frm);
                sx = XmlTransform(sx, indentf);
            }
            catch (XmlException er)
            {
                XMessageBox.Show(er.ToString());
            }

            return sx;
        }

        public static DateTime GetSysDate()
        {
            try
            {
                string cmdText = string.Empty;

                cmdText = "SELECT CONVERT(CHAR(10), GETDATE(), 111)";  //For SqlServer
                object data = DataAccessObject.ExecuteScalar(cmdText);

                if (data != null)
                    return DateTime.Parse(data.ToString());
                else
                    return DateTime.Today;
            }
            catch
            {
                return DateTime.Today;
            }
        }

        public static DateTime GetSysDateTime()
        {
            try
            {
                string cmdText = string.Empty;

                cmdText = "SELECT CONVERT(CHAR(10), GETDATE(), 111) + ' ' + CONVERT(CHAR(10), GETDATE(), 108) ";  //For SqlServer
                object data = DataAccessObject.ExecuteScalar(cmdText);
                if (data != null)
                    return DateTime.Parse(data.ToString());
                else
                    return DateTime.Now;
            }
            catch
            {
                return DateTime.Now;
            }
        }

        public static string GetSysDateYYYYMMDD()
        {
            return (DataAccessObject.ExecuteScalar("SELECT CONVERT(CHAR(10), GETDATE(), 112)") + "").Trim();
        }

        public static string GetSysTimeHHMMSS()
        {
            return (DataAccessObject.ExecuteScalar("SELECT REPLACE(CONVERT(CHAR(10), GETDATE(), 108),':','')") + "").Trim();
        }


        public static string Now(string deli)
        {
            return DateConv(Now(), deli);
        }

        public static DateTime Now()
        {
            return DateTime.Now;
        }

        public static string NowForDB(string deli)
        {
            return DateConv(NowForDB(), deli);
        }

        public static DateTime NowForDB()
        {
            DateTime dt = GetSysDate();
            if (dt == null) return Now();
            return dt;
        }

        public static string NowTimeForDB(string deli, string fg)
        {
            DateTime dt = GetSysDateTime();
            if (dt == null) return TimeConv(Now(), deli, fg);
            return TimeConv(dt, deli, fg);
        }

        public static long DateDiff(string interval, object date1, object date2)
        {
            return DateAndTime.DateDiff(interval, DateConv(date1), DateConv(date2), FirstDayOfWeek.Sunday, FirstWeekOfYear.Jan1);
        }

        public static TimeSpan TimeDiff(string date1, string date2)
        {
            TimeSpan tm = Convert.ToDateTime(date1) - Convert.ToDateTime(date2);
            return tm;
        }

        public static string DateAdd(string interval, double days, object date1)
        {
            return DateConv(DateAndTime.DateAdd(interval, days, DateConv(date1)));
        }

        public static DateTime DateTimeAdd(string interval, double number, object date1)
        {
            return DateAndTime.DateAdd(interval, number, date1);
        }

        [Description("yyyyMMddHHmmss 형식의 날짜를 yyyy-MM-dd HH:mm:ss 형식으로 변환합니다.")]
        public static string DateTimeConv(string dtm)
        {
            string dt = Mid(dtm, 1, 8);
            string tm = Mid(dtm, 9, 14);
            try
            {
                return DateConv(dt, "-") + " " + TimeConv(tm, ":", (tm.Length == 6 ? "LONG" : "SHORT"));
            }
            catch (Exception ex)
            {
                return dtm;
                throw ex;
            }
        }

        public static double Val(object obj)
        {
            try
            {
                return Conversion.Val(obj);
            }
            catch
            {
                return 0;
            }
        }

        public static string Format(object obj, string format)
        {
            try
            {
                if (obj is string && format.Contains("@"))
                {
                    char[] bytes = obj.ToString().ToCharArray();
                    int ii = bytes.Length;
                    for (int i = format.LastIndexOf("@"); i >= 0; i = format.LastIndexOf("@", i))
                    {
                        format = format.Remove(i, 1);
                        format = format.Insert(i, (ii > 0 ? bytes[ii - 1].ToString() : " "));
                        ii--;
                    }
                    return format;
                }
                return Strings.Format(obj, format);
            }
            catch
            {
                return "";
            }
        }

        public static string DateFormat(object date, string format)
        {
            try
            {
                if (date is string) return Convert.ToDateTime(DateConv(date)).ToString(format);
                return Convert.ToDateTime(date).ToString(format);
            }
            catch
            {
                return "";
            }
        }

        public static string TimeFormat(object time, string format)
        {
            try
            {
                if (time is string) return Convert.ToDateTime(TimeConv(time, ":")).ToString(format);
                return Convert.ToDateTime(time).ToString(format);
            }
            catch
            {
                return "";
            }
        }

        [Description("DateTime 또는 yyyymmdd 형식의 날짜를 yyyy/mm/dd 형식으로 변환합니다.")]
        public static string DateConv(object date)
        {
            return DateConv(date, "/");
        }

        [Description("DateTime 또는 yyyymmdd 형식의 날짜를 yyyy/mm/dd 형식으로 변환합니다.")]
        public static string DateConv(object date, string deli)
        {
            string tmpdate;

            try
            {
                //if (deli == string.Empty) deli = "/";
                if (date is DateTime)
                {
                    DateTime ddt = (DateTime)date;
                    tmpdate = ddt.Year.ToString("0000") + deli + ddt.Month.ToString("00") + deli + ddt.Day.ToString("00");
                }
                else
                {
                    string sdt = (string)date;
                    if (sdt.Length != 6 && sdt.Length != 8) return sdt;
                    if (sdt.Length == 6) tmpdate = sdt.Substring(0, 2) + deli + sdt.Substring(2, 2) + deli + sdt.Substring(4, 2);
                    else tmpdate = sdt.Substring(0, 4) + deli + sdt.Substring(4, 2) + deli + sdt.Substring(6, 2);
                }
                return tmpdate;
            }
            catch (Exception ex)
            {
                return Convert.ToDateTime(date).ToString("yyyy/MM/dd");
                throw ex;
            }
        }

        [Description("DateTime 또는 yyyy/mm/dd hh:mm:ss 형식의 시간을 hh:mm:ss 형식으로 변환합니다.")]
        public static string TimeConv(object date, string fg)
        {
            return TimeConv(date, ":", fg);
        }

        [Description("DateTime 또는 yyyy/mm/dd hh:mm:ss 형식의 시간을 hh:mm:ss 형식으로 변환합니다. fg = Long(hh:mm:ss)/else(hh:mm)")]
        public static string TimeConv(object date, string deli, string fg)
        {
            string tmptime;

            try
            {
                //if (deli == string.Empty) deli = ":";
                if (date is DateTime)
                {
                    DateTime ddt = (DateTime)date;
                    tmptime = ddt.Hour.ToString("00") + deli + ddt.Minute.ToString("00");
                    if (fg.ToUpper() == "LONG") tmptime += deli + ddt.Second.ToString("00");
                }
                else
                {
                    string sdt = (string)date;
                    if (sdt.Length != 4 && sdt.Length != 6) return sdt;
                    tmptime = sdt.Substring(0, 2) + deli + sdt.Substring(2, 2);
                    if (fg.ToUpper() == "LONG" || (sdt.Length == 6 && fg.ToUpper() != "SHORT")) tmptime += deli + sdt.Substring(4, 2);
                }
                return tmptime;
            }
            catch (Exception ex)
            {
                return Convert.ToDateTime(date).ToString();
                throw ex;
            }
        }

        public static string GetFirstDayOfMonth(DateTime dtmDate)
        {
            string sFirstDay_of_Month = dtmDate.ToString("yyyy-MM") + "-01";

            return DateTime.Parse(sFirstDay_of_Month).ToString("yyyy-MM-dd");
        }

        public static string GetLastDayOfMonth(DateTime dtmDate)
        {
            string sFirstDay_of_NextMonth = dtmDate.AddMonths(1).ToString("yyyy-MM") + "-01";

            return DateTime.Parse(sFirstDay_of_NextMonth).AddDays(-1).ToString("yyyy-MM-dd");
        }

        public static DateTime GetLastDay(DateTime dtmDate)
        {
            string sFirstDay_of_NextMonth = dtmDate.AddMonths(1).ToString("yyyy-MM") + "-01";
            return DateTime.Parse(sFirstDay_of_NextMonth).AddDays(-1);
        }

        public static bool IsDate(object date)
        {
            return Information.IsDate(DateConv(date));
        }

        public static string ToString(object value)
        {
            return ConvertToString(value);
        }

        public static string ConvertToString(object value)
        {
            try
            {
                string s = "";
                if (value != DBNull.Value && value != null)
                    s = Convert.ToString(value);

                return s;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static bool ToBool(object value)
        {
            return ConvertToBool(value);
        }

        public static bool ToBool(string value)
        {
            return ConvertToBool(value);
        }

        public static bool ConvertToBool(object value)
        {
            if (value == null || value == DBNull.Value) return false;
            return ConvertToBool(value.ToString());
        }

        public static bool ConvertToBool(string value)
        {
            if (value == "1") return true;
            if (value == "0") return false;
            if (value == "") return false;

            bool result;
            if (!bool.TryParse(value, out result))
                result = false;

            return result;
        }

        public static int ToInt(object value)
        {
            return ConvertToInt(value);
        }
        public static int ToInt(string value)
        {
            return ConvertToInt(value);
        }


        public static int ConvertToInt(object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            if (value.GetType().IsEnum) return Convert.ToInt32(value);
            return ConvertToInt(value.ToString());
        }

        public static int ConvertToInt(string value)
        {
            if (value == null) value = "";
            int result;
            if (!Int32.TryParse(value.Replace(",", ""), out result))
            {
                return Convert.ToInt32(ConvertToDouble(value));
            }

            return result;
        }

        public static long ToLong(object value)
        {
            return ConvertToLong(value);
        }

        public static long ToLong(string value)
        {
            return ConvertToLong(value);
        }

        public static long ConvertToLong(object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            return ConvertToLong(value.ToString());
        }

        public static long ConvertToLong(string value)
        {
            if (value == null) value = "";
            long result;
            if (!Int64.TryParse(value.Replace(",", ""), out result))
            {
                return Convert.ToInt64(ConvertToDouble(value));
            }

            return result;
        }

        public static decimal ToDecimal(object value)
        {
            return ConvertToDecimal(value);
        }

        public static decimal ToDecimal(string value)
        {
            return ConvertToDecimal(value);
        }

        public static decimal ConvertToDecimal(object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            return ConvertToDecimal(value.ToString());
        }

        public static decimal ConvertToDecimal(string value)
        {
            if (value == null) value = "";
            decimal result;
            if (!decimal.TryParse(value.Replace(",", ""), out result))
                result = 0;

            return result;
        }

        public static Double ToDouble(object value)
        {
            return ConvertToDouble(value);
        }

        public static Double ToDouble(string value)
        {
            return ConvertToDouble(value);
        }

        public static Double ConvertToDouble(object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            return ConvertToDouble(value.ToString());
        }

        public static Double ConvertToDouble(string value)
        {
            if (value == null) value = "";
            double result;
            if (!Double.TryParse(value.Replace(",", ""), out result))
                result = 0;

            return result;
        }

        public static float ToFloat(object value)
        {
            return ConvertToFloat(value);
        }

        public static float ToFloat(string value)
        {
            return ConvertToFloat(value);
        }

        public static float ConvertToFloat(object value)
        {
            if (value == null || value == DBNull.Value) return 0;
            return ConvertToFloat(value.ToString());
        }

        public static float ConvertToFloat(string value)
        {
            if (value == null) value = "";
            float result;
            if (!float.TryParse(value.Replace(",", ""), out result))
                result = 0;

            return result;
        }

        public static DateTime ConvertToDateTime(string value)
        {
            try
            {
                if (value.Length == 6 || value.Length == 8) value = DateConv(value, "/");
                DateTime date = Convert.ToDateTime(value);
                return date;
            }
            catch (Exception)
            {
                return Convert.ToDateTime(null);
            }
        }

        public static DateTime ConvertToDateTime(object value)
        {
            try
            {
                return Convert.ToDateTime(value);
            }
            catch
            {
                try
                {
                    return DateTime.ParseExact(value as string, new string[] { "yyyy/MM/dd", "yyyy-MM-dd", "yyyyMMdd" }, CultureInfo.CurrentCulture, DateTimeStyles.None);
                }
                catch
                {
                    return Convert.ToDateTime(null);
                }
            }
        }

        public static Time ConvertToTime(object value)
        {
            try
            {
                DateTime dt = ConvertToDateTime(value);
                return new Time(dt);
            }
            catch
            {
                return Time.NullValue;
            }
        }

        public static string ConvertToRtf(object value)
        {
            try
            {
                string rtfval = Convert.ToString(value);
                if (rtfval.TrimStart().StartsWith(@"{\rtf1", StringComparison.Ordinal)) return rtfval;

                RichTextBox rtf = new RichTextBox();
                rtf.Text = rtfval;
                return rtf.Rtf;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string ConvertToPlainText(object value)
        {
            try
            {
                string rtfval = Convert.ToString(value);
                if (!rtfval.TrimStart().StartsWith(@"{\rtf1", StringComparison.Ordinal)) return rtfval;

                RichTextBox rtf = new RichTextBox();
                rtf.Rtf = rtfval;
                return rtf.Text;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static Image ConvertToImage(object value)
        {
            try
            {
                if (value == DBNull.Value || value == null)
                    return null;
                else
                {
                    Byte[] byteBLOBData = new Byte[0];
                    byteBLOBData = (Byte[])(value);
                    return Image.FromStream(new MemoryStream(byteBLOBData));
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static object ConvertFromImage(Image value)
        {
            return ConvertFromImage(value, ImageFormat.Jpeg);
        }

        public static object ConvertFromImage(Image value, ImageFormat fmt)
        {
            try
            {
                if (value == null)
                    return DBNull.Value;
                else
                {     
                    MemoryStream ms = new MemoryStream();
                    Bitmap bmp = new Bitmap(value);
                    bmp.Save(ms, fmt);
                    Byte[] bytBLOBData = new Byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(bytBLOBData, 0, Convert.ToInt32(ms.Length));
                    return bytBLOBData;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static Image LoadPicture(string strPath)
        {
            try
            {
                FileStream fs = new FileStream(strPath, FileMode.Open, FileAccess.Read);
                Image img = Image.FromStream(fs);
                fs.Close();
                fs.Dispose();
                return img;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetDepth(int depth)
        {
            return Convert.ToString((char)(20 + depth));
        }

        public static DataTable ConvertStringToDataTable(string data, char coldelimiter = '\t', char rowDelimiter = '\n')
        {
            string colnm = "";
            DataTable dataTable = new DataTable();
            foreach (string row in data.Split(rowDelimiter))
            {
                DataRow dataRow = dataTable.NewRow();
                int idx = 0;
                foreach (string cell in row.Split(coldelimiter))
                {
                    colnm = "COL" + idx.ToString();
                    if (!dataTable.Columns.Contains(colnm))
                    {
                        DataColumn dataColumn = new DataColumn(colnm);
                        dataTable.Columns.Add(dataColumn);
                    }
                    dataRow[colnm] = cell;
                    idx++;
                }
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }

        public static DataTable ConvertStringToDataTable(string data, string coldelimiter = "\t", string rowDelimiter = "\r\n")
        {
            string colnm = "";
            DataTable dataTable = new DataTable();
            foreach (string row in SplitByString(data, rowDelimiter))
            {
                DataRow dataRow = dataTable.NewRow();
                int idx = 0;
                foreach (string cell in SplitByString(row, coldelimiter))
                {
                    colnm = "COL" + idx.ToString();
                    if (!dataTable.Columns.Contains(colnm))
                    {
                        DataColumn dataColumn = new DataColumn(colnm);
                        dataTable.Columns.Add(dataColumn);
                    }
                    dataRow[colnm] = cell;
                    idx++;
                }
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }

        //양력을 음력 변환
        public static DateTime ConvertToLunar(DateTime dt)
        {
            int n윤월;
            int n음력년, n음력월, n음력일;
            bool b윤달 = false;
            System.Globalization.KoreanLunisolarCalendar 음력 = new System.Globalization.KoreanLunisolarCalendar();

            n음력년 = 음력.GetYear(dt);
            n음력월 = 음력.GetMonth(dt);
            n음력일 = 음력.GetDayOfMonth(dt);
            if (음력.GetMonthsInYear(n음력년) > 12)             //1년이 12이상이면 윤달이 있음..
            {
                b윤달 = 음력.IsLeapMonth(n음력년, n음력월);     //윤월인지
                n윤월 = 음력.GetLeapMonth(n음력년);             //년도의 윤달이 몇월인지?
                if (n음력월 >= n윤월)                           //달이 윤월보다 같거나 크면 -1을 함 즉 윤8은->9 이기때문
                    n음력월--;
            }
            return new DateTime(int.Parse(n음력년.ToString()), int.Parse(n음력월.ToString()), int.Parse(n음력일.ToString()));
        }

        //음력을 양력 변환
        public static DateTime ConvertToSolar(DateTime dt)
        {
            return ConvertToSolar(dt.Year, dt.Month, dt.Day);
        }

        //음력을 양력 변환
        public static DateTime ConvertToSolar(int n음력년, int n음력월, int n음력일)
        {
            System.Globalization.KoreanLunisolarCalendar 음력 = new System.Globalization.KoreanLunisolarCalendar();

            bool b달 = 음력.IsLeapMonth(n음력년, n음력월);
            int n윤월;

            if (음력.GetMonthsInYear(n음력년) > 12)
            {
                n윤월 = 음력.GetLeapMonth(n음력년);
                if (b달)
                    n음력월++;
                if (n음력월 > n윤월)
                    n음력월++;
            }
            try
            {
                음력.ToDateTime(n음력년, n음력월, n음력일, 0, 0, 0, 0);
            }
            catch
            {
                return 음력.ToDateTime(n음력년, n음력월, 음력.GetDaysInMonth(n음력년, n음력월), 0, 0, 0, 0);//음력은 마지막 날짜가 매달 다르기 때문에 예외 뜨면 그날 맨 마지막 날로 지정
            }

            return 음력.ToDateTime(n음력년, n음력월, n음력일, 0, 0, 0, 0);
        }

        public static string RepalceBracket(string value)
        {
            try
            {
                string rtn = value.Replace("(", "").Replace(")", "");
                return rtn;
            }
            catch
            {
                return value;
            }
        }

        public static string StringToNumeric(string para)
        {
            string ss = string.Empty;
            char[] cc = para.ToCharArray();//'1','2','3'
            for (int i = 0; i < cc.Length; i++)
            {
                if (char.IsDigit(cc[i]))
                {
                    ss += cc[i];
                    //ss = string.Concat(ss, cc[i]);
                }
                else
                {
                    return ss;
                }
            }
            return ss;
        }

        public static bool IsLetterOrNumber(string para)
        {
            if (para == string.Empty) return false;
            for (int i = 0; i < para.Length; i++)
            {
                if (!char.IsLetterOrDigit(para, i)) return false;
            }
            return true;
        }

        public static bool IsLetter(string para)
        {
            if (para == string.Empty) return false;
            for (int i = 0; i < para.Length; i++)
            {
                if (!char.IsLetter(para, i)) return false;
            }
            return true;
        }

        public static bool IsNumber(string para)
        {
            if (para == string.Empty) return false;
            for (int i = 0; i < para.Length; i++)
            {
                if (!char.IsNumber(para, i)) return false;
            }
            return true;
        }

        public static bool IsRtf(object text)
        {
            try
            {
                new RichTextBox().Rtf = (string)text;
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }

        public static object CreateObject(string progId, string func, object[] args, ParameterModifier[] mods)
        {
            object rtn = null;
            try
            {
                object obj = Activator.CreateInstance(Type.GetTypeFromProgID(progId));
                //MethodInfo mi = obj.GetType().GetMethod(func);
                //if( mi != null) rtn = mi.Invoke(obj, param);
                rtn = obj.GetType().InvokeMember(func, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, obj, args, mods, null, null);
            }
            catch (Exception ex)
            {
                XMessageBox.Warning(ex.ToString());
            }

            return rtn;
        }

        public static object CreateObject(string progId)
        {
            object rtn = null;
            try
            {
                object obj = Activator.CreateInstance(Type.GetTypeFromProgID(progId));
                return obj;
            }
            catch (Exception ex)
            {
                XMessageBox.Warning(ex.ToString());
            }

            return rtn;
        }

        public static object InvokeMember(object obj, string func, object[] args, ParameterModifier[] mods)
        {
            object rtn = null;
            try
            {
                rtn = obj.GetType().InvokeMember(func, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, obj, args, mods, null, null);
            }
            catch (Exception ex)
            {
                XMessageBox.Warning(ex.ToString());
            }

            return rtn;
        }

        public static string Join(int depth, params object[] param)
        {
            return Join(Convert.ToString((char)(20 + depth)), param);
        }

        public static string Join(string deli, params object[] param)
        {
            string rtn = string.Empty;

            for (int i = 0; i < param.Length; i++)
            {
                if (i > 0) rtn += deli;
                rtn += param[i];
            }
            return rtn;
        }

        public static int GetPlen(string source, int depth)
        {
            return GetPlen(source, Convert.ToString((char)(20 + depth)));
        }

        public static int GetPlen(string source, string deli)
        {
            return StrContains(source, deli, 0) + 1;
        }

        public static string SetP(string source, int depth, int pos, string trans)
        {
            return SetP(source, Convert.ToString((char)(20 + depth)), pos, trans);
        }

        public static string SetP(string source, string deli, int pos, string trans)
        {
            string rtn = "";

            if (deli == "" || pos < 1) return "";

            int iMaxLen = GetPlen(source, deli);
            int iDeliCnt = (pos > iMaxLen ? pos - iMaxLen : 0);
            string tmpDelis = "";
            for (int i = 1; i <= iDeliCnt; i++) tmpDelis += deli;
            string tmpDeli1 = "";
            if (pos <= iMaxLen && pos > 1) tmpDeli1 = deli;
            string tmpDeli2 = "";
            if (pos < iMaxLen && pos >= 1) tmpDeli2 = deli;
            int iLastPos = (pos > iMaxLen ? pos : iMaxLen);
            rtn = GetPs(source, deli, 1, pos - 1);
            rtn += tmpDelis;
            rtn += tmpDeli1;
            rtn += trans;
            rtn += tmpDeli2;
            rtn += GetPs(source, deli, pos + 1, iLastPos);
            return rtn;
        }

        public static int GetPos(string src, int depth, string dest)
        {
            return GetPos(src, Convert.ToString((char)(20 + depth)), dest);
        }

        public static int GetPos(string src, string deli, string dest)
        {
            try
            {
                int maxp = GetPlen(src, deli);
                if (maxp < 1) return 0;
                for (int ii = 1; ii <= maxp; ii++)
                {
                    string piece = GetP(src, deli, ii);
                    if (piece == dest) return ii;
                }
                return 0;
            }
            catch { }

            return 0;
        }

        public static string GetPs(string para, int depth, int frP, int toP)
        {
            return GetPs(para, Convert.ToString((char)(20 + depth)), frP, toP);
        }

        public static string GetPs(string para, string deli, int frP, int toP)
        {
            string rtn = "";

            if (deli == "") return "";
            if (frP < 1) frP = 1;
            int len = GetPlen(para, deli);
            if (toP > len) toP = len;
            if (frP > toP) return "";

            try
            {
                int cnt = 0;
                string[] tmp = SplitByString(para, deli);
                for (int ii = frP; ii <= toP; ii++)
                {
                    if (cnt > 0) rtn = rtn + deli;
                    rtn = rtn + (tmp.Length >= ii ? tmp[ii - 1] : "");
                    cnt++;
                }
            }
            catch { }

            return rtn;
        }

        [Description("문자열을 구분자로 해당위치에서 하나만 잘라 옵니다.")]
        public static string GetP(string para, int depth, int pos)
        {
            return GetP(para, Convert.ToString((char)(20 + depth)), pos);
        }

        [Description("문자열을 구분자로 해당위치에서 하나만 잘라 옵니다.")]
        public static string GetP(string para, string deli, int pos)
        {
            string rtn = "";

            try
            {
                rtn = SplitByString(para, deli)[pos - 1];
            }
            catch { }

            return rtn;
        }

        [Description("문자열을 구분자로 순서대로 하나씩 잘라 옵니다.")]
        public static string Shift(ref string para, int depth)
        {
            return Shift(ref para, Convert.ToString((char)(20 + depth)));
        }

        [Description("문자열을 구분자로 순서대로 하나씩 잘라 옵니다.")]
        public static string Shift(ref string para, string deli)
        {
            string rtn = "";

            int indexOf = para.IndexOf(deli);
            if (indexOf != -1)
            {
                rtn = para.Substring(0, indexOf);
                para = para.Substring(indexOf + deli.Length);
            }
            else
            {
                rtn = para;
                para = "";
            }

            return rtn;
        }

        [Description("문자열을 정해진 위치에서 잘라 옵니다.(From ~ Length, start는 1 부터, Length가 생략되면 끝까지) - ●ex.cUtil.VBMid(12345, 3, 3)  => 3,4,5")]
        public static string VBMid(string para, int from, int length)
        {
            if (from < 1) from = 1;
            if (length > para.Length - from + 1) length = para.Length - from + 1;
            if (length < 0) return "";
            if (from > para.Length) return "";
            return para.Substring(from - 1, length);
        }

        [Description("문자열을 정해진 위치에서 잘라 옵니다.(From ~ Length, start는 1 부터)  ")]
        public static string VBMid(string para, int from)
        {
            return VBMid(para, from, para.Length - from + 1);
        }

        [Description("문자열을 정해진 위치에서 잘라 옵니다.(From ~ To, start는 1 부터)  ●ex. cUtil.Mid(12345, 3, 5)  //  3,4,5")]
        public static string Mid(string para, int from, int to)
        {
            if (from < 1) from = 1;
            if (to > para.Length) to = para.Length;
            if (from > to) return "";
            if (from > para.Length) return "";
            return para.Substring(from - 1, to - from + 1);
        }

        [Description("문자열을 정해진 위치에서 잘라 옵니다.(From ~ To, start는 뒤에서부터 1)")]
        public static string RMid(string para, int from, int to)
        {
            if (from < 1) from = 1;
            if (to > para.Length) to = para.Length;
            if (from > to) return "";
            if (from > para.Length) return "";
            int rto = para.Length + 1 - from;
            int rfrom = para.Length + 1 - to;
            return para.Substring(rfrom - 1, rto - rfrom + 1);
        }

        public static int HLen(string para)
        {
            byte[] s1 = Encoding.Default.GetBytes(para);
            return s1.Length;
        }

        public static string HMid(string para, int from, int to)
        {
            byte[] s1 = Encoding.Default.GetBytes(para);
            if (from < 1) from = 1;
            if (to > s1.Length) to = s1.Length;
            if (from > to) return "";
            if (from > s1.Length) return "";
            return Encoding.Default.GetString(s1, from - 1, to - from + 1);
        }

        public static string HRMid(string para, int from, int to)
        {
            byte[] s1 = Encoding.Default.GetBytes(para);
            if (from < 1) from = 1;
            if (to > s1.Length) to = s1.Length;
            if (from > to) return "";
            if (from > s1.Length) return "";
            int rto = s1.Length + 1 - from;
            int rfrom = s1.Length + 1 - to;
            return Encoding.Default.GetString(s1, rfrom - 1, rto - rfrom + 1);
        }

        public static int InStr(string str1, string str2)
        {
            return Strings.InStr(str1, str2);
        }

        public static int InStr(int start, string str1, string str2)
        {
            return Strings.InStr(start, str1, str2);
        }

        public static string SinglePiece(string Var, string Del, int cnt)
        {
            int Prt = 0;
            int Srt = 0;
            int Nxt = 0;

            if (cnt <= 0) return "";

            Nxt = (Del.Length * -1) + 1;

            for (Prt = 1; Prt <= cnt; Prt++)
            {
                Srt = Nxt + Del.Length;
                Nxt = Strings.InStr(Srt, Var, Del);

                if (Nxt == 0)
                {
                    Nxt = Var.Length + Del.Length;
                    break;
                }
            }

            if (Prt >= cnt)
            {
                return Mid(Var, Srt, Nxt - 1);
            }
            else
            {
                return "";
            }
        }


        public static string MultiPiece(string Var, string Del, int FromCnt, int ToCnt)
        {
            int Prt = 0;
            int Srt = 0;
            int Nxt = 0;
            int FromBuf = 0;

            if (FromCnt > ToCnt) return "";

            if (FromCnt < 1) FromCnt = 1;

            Nxt = (Del.Length * -1) + 1;

            for (Prt = 1; Prt <= ToCnt; Prt++)
            {
                Srt = Nxt + Del.Length;
                Nxt = Strings.InStr(Srt, Var, Del);

                if (Prt == FromCnt) FromBuf = Srt;

                if (Nxt == 0)
                {
                    Nxt = Var.Length + Del.Length;
                    break;
                }
            }

            if (FromBuf == 0) return "";
            return Mid(Var, FromBuf, Nxt - 1);
        }

        public static string SinglePieceSet(string Var, string Del, int cnt, string XCH)
        {
            int Prt = 0;
            int Srt = 0;
            int Nxt = 0;

            if (cnt == 0) return "";

            Nxt = (Del.Length * -1) + 1;

            for (Prt = 1; Prt <= cnt; Prt++)
            {
                Srt = Nxt + Del.Length;
                Nxt = Strings.InStr(Srt, Var, Del);

                if (Nxt == 0)
                {
                    Nxt = Var.Length + Del.Length;
                    break;
                }
            }

            if (Prt >= cnt)
            {
                return Left(Var, Srt - 1) + XCH + Mid(Var, Nxt, Var.Length + Del.Length - 1);
            }
            else
            {
                for (Srt = 1; Srt <= cnt - Prt; Srt++)
                {
                    Var = Var + Del;
                }

                return Var + XCH;
            }
        }

        public static string MultiPieceSet(string Var, string Del, int FromCnt, int ToCnt, string XCH)
        {
            int Prt = 0;
            int Srt = 0;
            int Nxt = 0;
            int FromBuf = 0;

            if (FromCnt > ToCnt) return "";

            if (FromCnt < 1) FromCnt = 1;

            if (Del == "")
            {
                return Left(Var, FromCnt - 1) + XCH + Mid(Var, ToCnt + 1, Var.Length);
            }

            Nxt = (Del.Length * -1) + 1;

            for (Prt = 1; Prt <= ToCnt; Prt++)
            {
                Srt = Nxt + Del.Length;
                Nxt = Strings.InStr(Srt, Var, Del);

                if (Prt == FromCnt) FromBuf = Srt;

                if (Nxt == 0)
                {
                    Nxt = Var.Length + Del.Length;
                    break;
                }
            }

            if (FromBuf > 0)
            {
                return Left(Var, FromBuf - 1) + XCH + Mid(Var, Nxt, Var.Length + Del.Length - 1);
            }
            else
            {
                for (Srt = 1; Srt <= FromCnt - Prt; Prt++)
                {
                    Var = Var + Del;
                }

                return Var + XCH;
            }
        }

        public static int L(string Var, string Del)
        {
            int Srt = 0;
            int Nxt = 0;
            int cnt = 0;

            if (Del == "") return 0;

            Nxt = (Del.Length * -1) + 1;

            do
            {
                Srt = Nxt + Del.Length;
                Nxt = Strings.InStr(Srt, Var, Del);
                cnt = cnt + 1;
            }
            while (Nxt != 0);
            return cnt;
        }

        public static int LengthByte(string Var)
        {
            int Cnt = 0;
            int NUM = 0;
            string tmp = "";

            if (Var == "") return 0;

            do
            {
                tmp = Var.Substring(Cnt, 1);
                Cnt = Cnt + 1;
                NUM = NUM + 1;

                byte[] s1 = Encoding.Default.GetBytes(tmp);
                if (s1.Length > 1) NUM = NUM + 1;
            } while (Cnt < Var.Length);

            return NUM;
        }

        public static string HExtract(string Var, string Del, int GetCnt)
        {
            string Buf = "";
            string tmp = "";
            int NUM = 0;
            int Cnt = 0;

            if (Var == "" || GetCnt < 2)
            {
                return "";
            }

            do
            {
                tmp = Var.Substring(Cnt, 1);
                Cnt = Cnt + 1;
                NUM = NUM + 1;

                byte[] s1 = Encoding.Default.GetBytes(tmp);
                if (s1.Length > 1) NUM = NUM + 1;

                if (NUM < GetCnt)
                    Buf = Buf + tmp;
                else if (NUM == GetCnt)
                {
                    NUM = 0;
                    Buf = Buf + tmp + Del;
                }
                else if (NUM > GetCnt)
                {
                    NUM = 2;
                    Buf = Buf + Del + tmp;
                }
            } while (Cnt < Var.Length);

            if (Buf.Substring(Buf.Length - 1) == Del) Buf = Buf.Substring(0, Buf.Length - 1);

            return Buf;
        }

        public static string UCase(string str)
        {
            return str.ToUpper();
        }

        public static string Left(string str, int length)
        {
            string result = "";
            for (int i = 0; i < length; i++)
            {
                if (str.Length <= i)
                    break;

                result += str[i];
            }

            return result;
        }

        public static string Right(string str, int length)
        {
            string result = "";
            for (int i = 0; i < length; i++)
            {
                if (str.Length - i <= 0)
                    break;

                result = str[str.Length - i - 1] + result;
            }

            return result;
        }

        public static string Piece(string Rec, string Deli, int PiecePos)
        {
            return GetP(Rec, Deli, PiecePos);
        }

        [Description("문자열을 정해진 위치에서 잘라 옵니다.(start는 0 부터)")]
        public static string SubString(string para, int start, int length)
        {
            if (start > para.Length - 1) return "";
            return para.Substring(start, System.Math.Min(length, para.Length - start));
        }

        public static string Tr(object para, string trans, string desti)
        {
            return Tr(para + "", trans, desti);
        }

        public static string Tr(string para, string trans, string desti)
        {
            return Replace(para, trans, desti);
        }

        public static string Replace(string para, string trans, string desti)
        {
            try
            {
                return para.Replace(trans, desti);
            }
            catch
            {
                return trans;
            }
        }

        public static string Space(int n)
        {
            return Strings.Space(n);
        }

        public static string RegexTr(string input, string pattern, string replace)
        {
            return Regex.Replace(input, pattern, replace);
        }

        public static string Chr(byte num)
        {
            return ((char)num).ToString();
        }

        public static int wcscmp(string A, string B)
        {
            return A.CompareTo(B);
        }

        public static int wcsncmp(string A, string B, int size)
        {
            return cUtil.Left(A, size).CompareTo(cUtil.Left(B, size));
        }

        public static int wcslen(string A)
        {
            return A.Length;
        }

        //같으면  FALSE    같지않으면  TRUE 
        public static bool wcscmpBool(string A, string B)
        {
            if (A.CompareTo(B) == 0)
            {
                return false;
            }
            return true;
        }

        //'*----------------------------------------------------------------------------------------
        //'*  1. 기능 : 해당 Form의 Field의 내용을 Clear
        //'*  2. 관련변수 :
        //'*  3. Parameter : objFormName ==> Form Object
        //'*                 strName ==> Field Name
        //'*  4. ex) DiscClearField(NEI0101E,"txtSKY","txtHSK",..)
        //'*         "txtSKY","txtHSK"... ==> 내용을 삭제하지 않음
        //'*----------------------------------------------------------------------------------------
        public static void DiscClearField(Control objFormName)
        {
            DiscClearField(objFormName, null);
        }

        public static void DiscClearField(Control objFormName, string[] strName)
        {
            foreach (Control c in objFormName.Controls)
            {
                DiscClearField(c, strName);
                if (cUtil.SubString(c.Name, 0, 3).ToUpper() == "TXT" || cUtil.SubString(c.Name, 0, 3).ToUpper() == "LBL")
                {
                    bool bFind = false;
                    if (strName != null)
                    {
                        foreach (string s in strName)
                        {
                            if (s == c.Name)
                            {
                                bFind = true;
                                break;
                            }
                        }
                    }
                    if (bFind == false) c.Text = "";
                }
            }
        }

        public static string GetRecordString(DataTable dt, int colDepth, int rowDepth)
        {
            return GetRecordString(dt, Convert.ToString((char)(20 + colDepth)), Convert.ToString((char)(20 + rowDepth)));
        }

        public static string GetRecordString(DataTable dt, string colDelimeter, string rowDelimeter)
        {
            string strRet = "";
            for (int ii = 0; ii < dt.Rows.Count; ii++)
            {
                for (int jj = 0; jj < dt.Columns.Count; jj++)
                {
                    strRet += dt.Rows[ii][jj] + "";
                    strRet += colDelimeter;
                }
                strRet += rowDelimeter;
            }
            return strRet;
        }

        [Description("내IP 가져오기")]
        public static string GetMyIp()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            string ip = "";
            for (int i = 0; i < host.AddressList.Length; i++)
            {
                if (host.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                {
                    ip = host.AddressList[i].ToString();
                    break;
                }
            }
            return ip;
        }

        [Description("내컴퓨터이름 가져오기")]
        public static string GetHostName()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            return host.HostName;
        }
        //
        public static string TelNumber(string num)
        {
            string tel = Convert.ToString(num);
            tel = tel.Replace("-", "").Replace(".", "");

            switch (tel.Length)
            {
                case 6:
                    return SubString(tel, 0, 2) + "-" + SubString(tel, 2, 4);
                case 7:
                    return SubString(tel, 0, 3) + "-" + SubString(tel, 3, 4);
                case 8:
                    return SubString(tel, 0, 4) + "-" + SubString(tel, 4, 4);
                case 9:
                    return SubString(tel, 0, 2) + "-" + SubString(tel, 2, 3) + "-" + SubString(tel, 5, 4);
                case 10:
                    if (tel.Substring(0, 2) == "02")
                    {
                        return SubString(tel, 0, 2) + "-" + SubString(tel, 2, 4) + "-" + SubString(tel, 6, 4);
                    }
                    else
                    {
                        return SubString(tel, 0, 3) + "-" + SubString(tel, 3, 3) + "-" + SubString(tel, 6, 4);
                    }
                case 11:
                    return SubString(tel, 0, 3) + "-" + SubString(tel, 3, 4) + "-" + SubString(tel, 7, 4);
                case 12:
                    return SubString(tel, 0, 4) + "-" + SubString(tel, 4, 4) + "-" + SubString(tel, 8, 4);
                default:
                    return tel;
            }
        }
        //
        public static long ISNULL(long pSocrce, long pchange)
        {
            if (string.IsNullOrWhiteSpace(pSocrce + "") == true)
            {
                return pchange;
            }
            return pSocrce;
        }
        //
        public static int ISNULL(int pSocrce, int pchange)
        {
            if (string.IsNullOrWhiteSpace(pSocrce + "") == true)
            {
                return pchange;
            }
            return pSocrce;
        }
        //
        public static decimal ISNULL(decimal pSocrce, decimal pchange)
        {
            if (string.IsNullOrWhiteSpace(pSocrce + "") == true)
            {
                return pchange;
            }
            return pSocrce;
        }
        //
        public static string ISNULL(string pSocrce, string pchange)
        {
            if (string.IsNullOrWhiteSpace(pSocrce + "") == true)
            {
                return "" + pchange + "";
            }
            return "" + pSocrce + "";
        }

        public static string ISNULL2Zero(string pSocrce)
        {
            if (string.IsNullOrWhiteSpace(pSocrce + "") == true)
            {
                return "0";
            }
            return pSocrce;
        }

        public static string ISNULL2Blank(string pSocrce)
        {
            if (string.IsNullOrWhiteSpace(pSocrce + "") == true)
            {
                return "";
            }
            return pSocrce;
        }

        //원하는 문자열 자름
        public static string SubStringB(byte[] parrB, int piStart, int piLenght)
        {
            return System.Text.Encoding.Default.GetString(parrB, piStart, piLenght);
        }

        //원하는 문자열 자름
        //0부터 시작. ex.12345 일경우 1~3 자르면 123 잘리는게 아니라  234 잘린다. 
        public static string SubStringB(string psText, int piStart, int piLenght)
        {
            byte[] arrB = Encoding.Default.GetBytes(psText);
            if (arrB.Length >= piStart + piLenght)
            {
                return System.Text.Encoding.Default.GetString(arrB, piStart, piLenght).Trim();
            }
            else
            {
                string srtn = psText;
                for (int ii = arrB.Length; ii < piLenght; ii++)
                {
                    srtn += " ";
                }
                return srtn;
            }
        }

        //아스키 값을 문자열로 변환
        public static string AscToChar(int piAscii)
        {
            return Encoding.ASCII.GetString(new byte[] { (byte)piAscii });
        }

        public static int Asc(string str)
        {
            return Strings.Asc(str);
        }

        public static int Asc(char chr)
        {
            return Strings.Asc(chr);
        }

        //문자열 길이 (영문1자리 한글2자리) 
        public static int LengthB(string pstr)
        {
            return System.Text.Encoding.Default.GetBytes(pstr).Length;
        }
        //
        [Description("레지스트리 값쓰기")]
        public static void SetRegistKeyValue(string pProject, string psKey, string psValue)
        {
            Microsoft.Win32.RegistryKey v_key = Microsoft.Win32.Registry.LocalMachine;
            Microsoft.Win32.RegistryKey v_software = v_key.OpenSubKey("SOFTWARE", true);
            Microsoft.Win32.RegistryKey v_metro = v_software.OpenSubKey("METRO", true);
            if (v_metro == null)
            {
                v_metro.CreateSubKey("METRO");
            }
            Microsoft.Win32.RegistryKey v_Project = v_metro.OpenSubKey(pProject, true);
            if (v_Project == null)
            {
                v_Project.CreateSubKey(pProject);
            }

            v_Project.SetValue(psKey, psValue);
            v_Project.Close();
        }

        [Description("레지스트리 값가져오기")]
        public static string GetRegistKeyValue(string pProject, string psKey, string psValue)
        {
            String sRtn = psValue;
            Microsoft.Win32.RegistryKey v_key = Microsoft.Win32.Registry.LocalMachine;
            Microsoft.Win32.RegistryKey v_software = v_key.OpenSubKey("SOFTWARE");
            Microsoft.Win32.RegistryKey v_metro = v_software.OpenSubKey("METRO");
            Microsoft.Win32.RegistryKey v_Project = v_metro.OpenSubKey(pProject);
            if (v_Project != null)
            {
                sRtn = (String)v_Project.GetValue(psKey);
                v_Project.Close();
            }
            return sRtn;
        }

        ////아이피 구하기
        //public static string GetLocalIP()
        //{
        //    string localIP = "Not available";
        //    IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        //    foreach (IPAddress ip in host.AddressList)
        //    {
        //        if (ip.AddressFamily == AddressFamily.InterNetwork)
        //        {
        //            localIP = ip.ToString();
        //            break;
        //        }
        //    }
        //    return localIP;
        //}

        /// <summary>
        /// //구분되어진 문자열 기준 문자열의 위치 값 가져옴  ex.1/2/4/5/  일경우 '/'로 구분 1=1 , 2=2 가 리턴 (1번째위치부터 시작, 0번째부터 아님)
        /// </summary>
        /// <param name="pstr"></param>
        /// <param name="psSymbol"></param>
        /// <param name="pPos"></param>
        /// <returns></returns>
        public static string GetStrByPos(string pstr, string psSymbol, int pPos)
        {
            string sRtn = "";
            string[] arrStr = pstr.Split(psSymbol.ToCharArray(), StringSplitOptions.None);
            if (arrStr.Length >= pPos)
            {
                sRtn = arrStr[--pPos];
            }
            return sRtn;
        }

        /// <summary>
        /// 문자열 정렬 -문자열로만 정보를 보여줄경우 정렬 하기위해 (ex. 툴팁)
        /// </summary>
        /// <param name="pstr"></param>
        /// <param name="piLen"></param>
        /// <param name="psAlign"></param>
        /// <returns></returns>
        public static string SetStringAlign(string pstr, int piLen, string psAlign)
        {
            int iLen = System.Text.Encoding.Default.GetBytes(pstr).Length;
            int iFill = piLen - iLen;
            if (iLen >= piLen)
            {
                return pstr;
            }
            string sFill = "";
            for (int ii = 0; ii < iFill; ii++)
            {
                sFill += " ";
            }
            switch (psAlign.ToUpper())
            {
                case "LEFT":
                case "L":
                case "왼쪽":
                case "NEAR":
                    pstr = pstr + sFill;
                    break;
                case "RIGHT":
                case "R":
                case "오른족":
                case "Far":
                    pstr = sFill + pstr;
                    break;
                case "CENTER":
                case "C":
                case "가운데":
                case "중간":
                    if (iFill % 2 != 0)//떨어지나.
                    {
                        pstr = sFill.Substring(0, sFill.Length / 2) + pstr + sFill.Substring(0, sFill.Length / 2 + 1);
                    }
                    else
                    {
                        pstr = sFill.Substring(0, sFill.Length / 2) + pstr + sFill.Substring(0, sFill.Length / 2);
                    }
                    break;
            }
            return pstr;
        }

        //public static string HMidHan(string pstr, int iByte)
        //{
        //    System.Text.Encoding sEnd = System.Text.Encoding.GetEncoding("ks_c_5601-1987");
        //    byte[] buf = sEnd.GetBytes(pstr);
        //    return sEnd.GetString(buf, 0, iByte);
        //}

        static public string SubStrHan(String str, int nStart, int nLen)
        {
            try
            {
                if (str != null && str != String.Empty)
                {
                    Encoding encoding = Encoding.GetEncoding("euc-kr");
                    byte[] abyBuf = encoding.GetBytes(str);
                    int nBuf = abyBuf.Length;

                    if (nStart < 0)
                    {
                        nStart = 0;
                    }
                    else if (nStart > nBuf)
                    {
                        nStart = nBuf;
                    }

                    if (nLen < 0)
                    {
                        nLen = 0;
                    }
                    else if (nLen > nBuf - nStart)
                    {
                        nLen = nBuf - nStart;
                    }

                    if (nStart < nBuf)
                    {
                        int nCopyStart = 0;
                        int nCopyLen = 0;

                        // 시작 위치를 결정한다.
                        if (nStart >= 1)
                        {
                            while (true)
                            {
                                if (abyBuf[nCopyStart] >= 0x80)
                                {
                                    nCopyStart++;
                                }

                                nCopyStart++;

                                if (nCopyStart >= nStart)
                                {
                                    if (nCopyStart > nStart)
                                    {
                                        nLen--;
                                    }

                                    break;
                                }
                            }
                        }

                        // 길이를 결정한다.
                        int nI = 0;

                        while (nI < nLen)
                        {
                            if (abyBuf[nCopyStart + nI] >= 0x80)
                            {
                                nI++;
                            }

                            nI++;
                        }

                        nCopyLen = (nI <= nLen) ? nI : nI - 2;

                        if (nCopyLen >= 1)
                        {
                            return encoding.GetString(abyBuf, nCopyStart, nCopyLen);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return String.Empty;
        }



        /// <summary>
        /// In 연산체크 
        /// </summary>
        /// <param name="sPara"></param>
        /// <param name="pInStr"></param>
        /// <returns></returns>
        public static bool IN(string sPara, string pInStr)
        {
            if (string.IsNullOrWhiteSpace(sPara + "") == true ||
                string.IsNullOrWhiteSpace(pInStr + "") == true)
            {
                return false;
            }
            string[] arrS = pInStr.Split(',');
            for (int ii = 0; ii < arrS.Length; ii++)
            {
                if (sPara == arrS[ii])
                {
                    return true;
                }
            }
            return false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 영문체크
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        public static bool CheckEnglish(string pstr)
        {
            bool IsCheck = true;

            Regex engRegex = new Regex(@"[a-zA-Z]");
            Boolean ismatch = engRegex.IsMatch(pstr);

            if (!ismatch)
            {
                IsCheck = false;
            }

            return IsCheck;
        }

        /// <summary>
        /// 숫자체크
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        public static bool CheckNumber(string pstr)
        {
            bool IsCheck = true;

            Regex numRegex = new Regex(@"[0-9]");
            Boolean ismatch = numRegex.IsMatch(pstr);

            if (!ismatch)
            {
                IsCheck = false;
            }

            return IsCheck;
        }

        /// <summary>
        /// 영문숫자체크
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        public static bool CheckEnglishNumber(string pstr)
        {
            bool IsCheck = true;

            Regex engRegex = new Regex(@"[a-zA-Z]");
            Boolean ismatch = engRegex.IsMatch(pstr);
            Regex numRegex = new Regex(@"[0-9]");
            Boolean ismatchNum = numRegex.IsMatch(pstr);

            if (!ismatch && !ismatchNum)
            {
                IsCheck = false;
            }
            return IsCheck;
        }

        /// <summary>
        /// 특수문자 체크
        /// </summary>
        /// <param name="letter"></param>
        /// <returns></returns>
        public static bool CheckSpcharacter(string pstr)
        {
            bool IsCheck = true;

            Regex enSp = new Regex(@"[~!@\#$%^&*\()\=+|\\/:;?""<>']");
            Boolean ismatch = enSp.IsMatch(pstr);

            if (!ismatch)
            {
                IsCheck = false;
            }

            return IsCheck;
        }

        /// <summary>
        /// 한글체크
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool CheckHangul(string pstr)
        {
            char[] arrTmp = pstr.ToCharArray();
            for (int k = 0; k < arrTmp.Length; k++)
            {
                if (arrTmp[k] >= '\xAC00' && arrTmp[k] <= '\xD7AF')
                {
                }
                else if (arrTmp[k] >= '\x3130' && arrTmp[k] <= '\x318F')
                {
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 문자 날짜타입변환시 유효한지 체크
        /// </summary>
        /// <param name="sDATE"></param>
        /// <returns></returns>
        public static bool IsyyyyMMdd(string sDATE)
        {
            try
            {
                string sD = sDATE.Replace("-", "").Replace("/", "").Replace(":", "").Replace("-", "").Replace(" ", "");
                if (sD.Length != 8) return false;
                Convert.ToDateTime(sD.Insert(6, "-").Insert(4, "-"));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 문자 시간타입변환시 휴효한지 체크
        /// </summary>
        /// <param name="sTime"></param>
        /// <returns></returns>
        public static bool IsHHmm(string sTime)
        {
            try
            {
                string sT = sTime.Replace("-", "").Replace("/", "").Replace(":", "").Replace("-", "").Replace(" ", "");
                if (sT.Length != 4) return false;
                Convert.ToDateTime(sT.Insert(2, ":"));
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 문자 시간타입변환시 유효한지 체크
        /// </summary>
        /// <param name="sTime"></param>
        /// <returns></returns>
        public static bool IsHHmmss(string sTime)
        {
            try
            {
                string sT = sTime.Replace("-", "").Replace("/", "").Replace(":", "").Replace("-", "").Replace(" ", "");
                if (sT.Length != 6) return false;
                Convert.ToDateTime(sT.Insert(4, ":").Insert(2, ":"));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Blank DBNull로 변환
        /// </summary>
        /// <param name="pstr"></param>
        /// <returns></returns>
        public static object Blank2DbNBull(string pstr)
        {
            try
            {
                if (pstr == "")
                {
                    return DBNull.Value;
                }
                else
                {
                    return pstr;
                }
                return DBNull.Value;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 문자열을 날짜타입으로 리턴 20020202 ->날짜타입
        /// </summary>
        /// <param name="pstr"></param>
        /// <returns></returns>
        public static DateTime yyyyMMddToDateTime(string pstr)
        {
            return DateTime.Parse(pstr.Insert(6, "-").Insert(4, "-"));
        }

        public static string yyyyMMdd2yyyy_MM_dd(string pstr)
        {
            pstr = pstr.Replace("/", "").Replace("-", "").Replace(":", "");
            if (pstr.Length != 8) return "";
            return pstr.Insert(6, "-").Insert(4, "-");
        }

        public static string yyyyMMdd2yyyy년MM월dd(string pstr)
        {
            pstr = pstr.Replace("/", "").Replace("-", "").Replace(":", "");
            if (pstr.Length != 8) return "";
            return pstr.Insert(6, "월").Insert(4, "년")+"일";
        }

        public static string HHMMss2_HH_mm(string pstr)
        {
            pstr = pstr.Replace(":", "").Replace("/", "").Replace("-", "");
            if (pstr.Length < 4) return "";
            return pstr.Substring(0, 4).Insert(2, ":");
        }

        public static string HHMMss2_HH시mm(string pstr)
        {
            pstr = pstr.Replace(":", "").Replace("/", "").Replace("-", "");
            if (pstr.Length < 4) return "";
            return pstr.Substring(0, 4).Insert(2, "시") +"분";
        }

        public static Process DoProcess(string psPath, string psPara, ProcessWindowStyle style = ProcessWindowStyle.Normal)
        {
            try
            {
                ProcessStartInfo pc = new ProcessStartInfo();

                //20201102 정민석 true 에서 false 로 바꿈 (공유폴드 내 EXE 실행시 출판자 명확하지 않다는 경고 메세지 뜸 방지)
                //pc.UseShellExecute = true;
                pc.UseShellExecute =false ; 

                pc.FileName = psPath;
                pc.Arguments = psPara;
                pc.WindowStyle = style;

                return Process.Start(pc);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region 2차원배열 키값 - 데이타  저장
        //2차원 배열에 1:키값 2:데이타 형태로 관리된 내역에서 데이타 가져오기
        public static void ClsStrDicParam(ref string[,] dicStr )
        {
            try
            {
                dicStr = new string[0, 0];
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //2차원 배열에 1:키값 2:데이타 형태로 관리 (string만)
        public static bool SetStrDicParam(ref string[,] dicStr, string sKey, string sValue)
        {
            try
            {
                bool blExists = false;
                for (int ii = 0; ii < (dicStr.Length / dicStr.Rank); ii++)
                {
                    if (dicStr[ii, 0] == sKey.Trim())
                    {
                        dicStr[ii, 0] = sKey.Trim();
                        dicStr[ii, 1] = sValue;
                        blExists = true;
                        break;
                    }
                }
                if (blExists == false)
                {
                    string[,] sArr = new string[(dicStr.Length / dicStr.Rank) + 1, 2];

                    for (int ii = 0; ii < (dicStr.Length / dicStr.Rank); ii++)
                    {
                        sArr[ii, 0] = dicStr[ii, 0];
                        sArr[ii, 1] = dicStr[ii, 1];
                    }
                    sArr[(sArr.Length / dicStr.Rank) - 1, 0] = sKey.Trim();
                    sArr[(sArr.Length / dicStr.Rank) - 1, 1] = sValue;
                    dicStr = sArr;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //2차원 배열에 1:키값 2:데이타 형태로 관리된 내역에서 데이타 가져오기
        public static string GetStrDicParam(string[,] dicStr, string sKey)
        {
            try
            {
                for (int ii = 0; ii < (dicStr.Length / dicStr.Rank); ii++)
                {
                    if (dicStr[ii, 0] == sKey)
                    {
                        return dicStr[ii, 1];
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion 

        public static bool IsHanWanSung(char c)
        {
            if (c >= '\xAC00' && c <= '\xD7AF')//한글완성
            {
                return true;
            }
            return false;
        }

        public static bool IsHanJaMo(char c)
        {
            if (c >= '\x3130' && c <= '\x318F') //한글자음모음
            {
                return true;
            }
            return false;
        }

    }
}
