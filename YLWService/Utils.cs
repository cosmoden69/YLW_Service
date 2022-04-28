using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Globalization;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.Xsl;
using Microsoft.Win32;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YLWService
{
    public class Utils
    {
        private const UInt32 WM_CLOSE = 0x0010;

        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        public const UInt32 SWP_NOSIZE = 0x0001;
        public const UInt32 SWP_NOMOVE = 0x0002;
        public const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;

        [DllImport("user32.dll")]
        static extern bool AllowSetForegroundWindow(int dwProcessId);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int BringWindowToTop(IntPtr hwnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        public static void CloseWindow(IntPtr hwnd)
        {
            SendMessage(hwnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }

        [DllImport("kernel32", SetLastError = true)]
        public static extern bool CloseHandle(IntPtr objectHandle);
            
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

        public static string Join(int depth, params string[] param)
        {
            return Join(Convert.ToString((char)(20 + depth)), param);
        }

        public static string Join(string deli, params string[] param)
        {
            string rtn = string.Empty;

            for (int i = 0; i < param.Length; i++)
            {
                if (i > 0) rtn += deli;
                rtn += param[i];
            }
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

        public static int GetPlen(string source, int depth)
        {
            return GetPlen(source, Convert.ToString((char)(20 + depth)));
        }

        public static int GetPlen(string source, string deli)
        {
            return StrContains(source, deli, 0) + 1;
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

        [Description("문자열을 정해진 위치에서 잘라 옵니다.(From ~ To, start는 1 부터)")]
        public static string Mid(string para, int from, int to)
        {
            if (from < 1) from = 1;
            if (to > para.Length) to = para.Length;
            if (from > to) return "";
            if (from > para.Length) return "";
            return para.Substring(from - 1, to - from + 1);
        }

        [Description("문자열을 정해진 위치에서 잘라 옵니다.(start는 0 부터)")]
        public static string SubString(string para, int start, int length)
        {
            if (start > para.Length - 1) return "";
            return para.Substring(start, System.Math.Min(length, para.Length - start));
        }

        [Description("왼쪽에 공백문자 채우기")]
        public static string PadLeft(object para, int width, char pad = ' ')
        {
            return (para + "").PadLeft(width, pad);
        }

        [Description("오른쪽 공백문자 채우기")]
        public static string PadRight(object para, int width, char pad = ' ')
        {
            return (para + "").PadRight(width, pad);
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

        [Description("내공인IP 가져오기")]
        public static string GetExternalIPAddress()
        {
            string externalip = new WebClient().DownloadString("http://ipinfo.io/ip").Trim();  //http://icanhazip.com

            if (String.IsNullOrWhiteSpace(externalip))
            {
                externalip = GetMyIp();  //null경우 Get Internal IP를 가져오게 한다.
            }

            return externalip;
        }

        [Description("내컴퓨터이름 가져오기")]
        public static string GetHostName()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            return host.HostName;
        }

        public static DateTime Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);

            while (AfterWards >= ThisMoment)
            {
                System.Windows.Forms.Application.DoEvents();
                ThisMoment = DateTime.Now;
            }

            return DateTime.Now;
        }

        public static string TelNumber(string num)
        {
            string tel = Convert.ToString(num.Trim());
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
                DateTime rdt = default(DateTime);
                if (date is string) rdt = Convert.ToDateTime(DateConv(date));
                else rdt = Convert.ToDateTime(date);
                if (rdt == default(DateTime)) return "";
                return rdt.ToString(format);
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

        public static string ConvertToRtf(object value)
        {
            try
            {
                string rtfval = Convert.ToString(value);
                if (rtfval.TrimStart().StartsWith(@"{\rtf1", StringComparison.Ordinal)) return rtfval;

                RichTextBox rtf = new RichTextBox();
                rtf.Font = new Font("맑은 고딕", 10);
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
                rtf.Font = new Font("맑은 고딕", 10);
                rtf.Rtf = rtfval;
                return rtf.Text;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string ToMultiline(object obj)
        {
            string txt = Utils.ConvertToString(obj);
            return txt.Replace("\r\n", "\n").Replace("\n", "\r\n");
        }

        public static string AddComma(object value)
        {
            try
            {
                string rtn = Convert.ToString(value);
                if (rtn == "") return "";
                rtn = String.Format("{0:#,0}", ConvertToDouble(rtn));
                return rtn;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string Adjuster(object value)
        {
            try
            {
                string rtn = Convert.ToString(value);
                if (rtn == "" || rtn.Length > 3) return rtn;
                rtn = rtn.Aggregate(string.Empty, (c, i) => c + i + "   ");
                return rtn;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static string Round(object value, int dec)
        {
            try
            {
                return ConvertToString(Math.Round(ToDecimal(value), dec));
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static Image stringToImage(string inputString)
        {
            try
            {
                if (inputString.Length % 4 != 0)  //이미지가 4의 배수이므로 잘못된 이미지인 경우 잘라낸다
                {
                    inputString = inputString.Substring(0, inputString.Length - (inputString.Length % 4));
                }
                byte[] imageBytes = Convert.FromBase64String(inputString);
                MemoryStream ms = new MemoryStream(imageBytes);

                Image image = Image.FromStream(ms, true, true);

                return image;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static string ImageToString(Image value)
        {
            try
            {
                if (value == null)
                    return "";
                else
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Bitmap bmp = new Bitmap(value);
                        bmp.Save(ms, value.RawFormat);
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string ClassToJsonstring(object cls)
        {
            return new JavaScriptSerializer().Serialize(cls);
        }

        public static string JObjectToJsonstring(object jobj)
        {
            return JsonConvert.SerializeObject(jobj);
        }

        public static string Serialize<T>(T dataToSerialize)
        {
            try
            {
                var stringwriter = new System.IO.StringWriter();
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(stringwriter, dataToSerialize);
                return stringwriter.ToString();
            }
            catch
            {
                throw;
            }
        }

        public static T Deserialize<T>(string xmlText)
        {
            try
            {
                var stringReader = new System.IO.StringReader(xmlText);
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(stringReader);
            }
            catch
            {
                throw;
            }
        }

        public static string GetSQL(string sqlText, IDbDataParameter[] parameters)
        {
            int pos = -1;
            int startIndex = 0;
            int index = startIndex;
            int cmtPos;
            bool loopExit = false;
            string symbol = "@";

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
                        string varName = sqlText.Substring(pos);
                        IDbDataParameter p;
                        try
                        {
                            p = parameters.First(x => x.ParameterName == varName);
                        }
                        catch
                        {
                            loopExit = true;
                            break;
                        }
                        string replace = "'" + p.Value + "'";
                        sqlText = sqlText.Remove(pos);
                        sqlText = sqlText.Insert(pos, replace);
                        loopExit = true;
                        break;
                    }
                    else if (IsDelimiter(sqlText[i]))
                    {
                        string varName = sqlText.Substring(pos, i - pos);
                        IDbDataParameter p;
                        try
                        {
                            p = parameters.First(x => x.ParameterName == varName);
                        }
                        catch
                        {
                            loopExit = false;
                            break;
                        }
                        string replace = "'" + p.Value + "'";
                        sqlText = sqlText.Remove(pos, i - pos);
                        sqlText = sqlText.Insert(pos, replace);
                        startIndex = pos + replace.Length;
                        index = startIndex;
                        loopExit = false;
                        break;
                    }
                }
            }
            //if p.DbType == DbType.Int16

            return sqlText;
        }

        public static void SetCombo(ComboBox cboObj, DataTable pdt, string strValueMember, string strDisplayMember, bool bAddnull)
        {
            try
            {
                if (cboObj.DataSource != null) cboObj.DataSource = null;
                cboObj.Items.Clear();

                if (pdt == null || pdt.Rows.Count < 1)
                {
                    return;
                }
                if (bAddnull)
                {
                    DataRow drTmp = pdt.NewRow();
                    drTmp[strValueMember] = DBNull.Value;
                    drTmp[strDisplayMember] = "";
                    pdt.Rows.InsertAt(drTmp, 0);
                }
                cboObj.DataSource = pdt;
                cboObj.ValueMember = strValueMember;
                cboObj.DisplayMember = strDisplayMember;
                cboObj.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public static string GetComboSelectedValue(ComboBox comboBox, string codeMember)
        {
            try
            {
                int index = comboBox.SelectedIndex;
                if (index == -1) return "";
                string strRet = ((DataTable)comboBox.DataSource).Rows[index][codeMember].ToString();
                return strRet;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void SetComboSelectedValue(ComboBox comboBox, string value, string codeMember)
        {
            try
            {
                Boolean bFind = false;
                for (int i = 0; i < comboBox.Items.Count; i++)
                {
                    string strCode = ((DataTable)comboBox.DataSource).Rows[i][codeMember].ToString();
                    if (strCode == value)
                    {
                        comboBox.SelectedIndex = i;
                        bFind = true;
                        break;
                    }
                }
                if (bFind == false)
                {
                    comboBox.SelectedIndex = -1;
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void ClearFolder(string path, string searchPattern = "*.*", SearchOption opt = SearchOption.TopDirectoryOnly)
        {
            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                DirectoryInfo dir = new DirectoryInfo(path);
                var files = dir.GetFiles(searchPattern, opt);
                foreach (var file in files)
                {
                    file.Attributes = FileAttributes.Normal;
                    file.Delete();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool ZipExtract(string fileName, string path)
        {
            try
            {
                ClearFolder(path, "*.*", SearchOption.AllDirectories);
                Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(fileName);
                zip.ExtractAll(path, Ionic.Zip.ExtractExistingFileAction.OverwriteSilently);
                zip.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool ZipAddDirectory(string fileName, string path)
        {
            try
            {
                File.Delete(fileName);
                Ionic.Zip.ZipFile zip = new Ionic.Zip.ZipFile(Encoding.UTF8);
                zip.AddDirectory(path);
                zip.Save(fileName);
                zip.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool FileMoveTo(string sourceFile, string destPath)
        {
            try
            {
                string filename = Path.GetFileName(sourceFile);
                string destFile = Path.Combine(destPath, filename);
                if (!Directory.Exists(destPath)) Directory.CreateDirectory(destPath);
                if (File.Exists(destFile)) File.Delete(destFile);
                File.Move(sourceFile, destFile);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool DeleteFile(string file)
        {
            try
            {
                if (File.Exists(file)) File.Delete(file);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static XmlDocument ToXmlDocument(XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            using (var xmlReader = xDocument.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        public static XDocument ToXDocument(XmlDocument xmlDocument)
        {
            using (var nodeReader = new XmlNodeReader(xmlDocument))
            {
                nodeReader.MoveToContent();
                return XDocument.Load(nodeReader);
            }
        }

        public static bool IsHtml(string str)
        {
            string str2 = str.Replace("m²", "m2").Replace("㎡", "m2");
            return (str2 != HttpUtility.HtmlEncode(str2));
        }

        public static bool IsHtml2(string str)
        {
            System.Text.RegularExpressions.Regex tagRegex = new System.Text.RegularExpressions.Regex(@"<\s*([^ >]+)[^>]*>.*?<\s*/\s*\1\s*>");
            return tagRegex.IsMatch(str);
        }
        public static bool IsRtf(string str)
        {
            return str.TrimStart().StartsWith(@"{\rtf1", StringComparison.Ordinal);
        }

        public static void ActivateApp(string processName)
        {
            Process[] p = Process.GetProcessesByName(processName);

            // Activate the first application we find with this name
            if (p.Count() > 0)
            {
                try
                {
                    ShowWindowAsync(p[0].MainWindowHandle, SW_SHOWNORMAL);
                    AllowSetForegroundWindow(p[0].Id);
                    SetForegroundWindow(p[0].MainWindowHandle);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public static void BringToFront(IntPtr handle)
        {
            try
            {
                SetForegroundWindow(handle);
                SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
                SetWindowPos(handle, HWND_NOTOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void TopMost(IntPtr handle, bool fg = true)
        {
            try
            {
                SetForegroundWindow(handle);
                if (fg == true) SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
                else SetWindowPos(handle, HWND_NOTOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void DoubleBuffered(DataGridView dgv, bool setting)
        {
            Type dgvType = dgv.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
                  BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(dgv, setting, null);
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

        public static string FtpDownload(string id, string pwd, string filepath, string downloadFile)
        {
            try
            {
                FtpWebRequest req = (FtpWebRequest)WebRequest.Create(new Uri(filepath));
                req.Method = WebRequestMethods.Ftp.DownloadFile;
                req.Credentials = new NetworkCredential(id, pwd);
                req.UseBinary = true;

                using (FtpWebResponse resp = (FtpWebResponse)req.GetResponse())
                {
                    // FTP 결과 스트림
                    Stream stream = resp.GetResponseStream();

                    byte[] buffer = new byte[2048];
                    FileStream fs = new FileStream(downloadFile, FileMode.Create);
                    int ReadCount = stream.Read(buffer, 0, buffer.Length);
                    while (ReadCount > 0)
                    {
                        fs.Write(buffer, 0, ReadCount);
                        ReadCount = stream.Read(buffer, 0, buffer.Length);
                    }
                    fs.Close();
                    stream.Close();
                    return resp.StatusDescription;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string FtpUpload(string id, string pwd, string filepath, string uploadFile)
        {
            try
            {
                FtpWebRequest req = (FtpWebRequest)WebRequest.Create(new Uri(filepath));
                req.Method = WebRequestMethods.Ftp.UploadFile;
                req.Credentials = new NetworkCredential(id, pwd);
                req.UseBinary = true;
                req.UsePassive = true;

                byte[] data = File.ReadAllBytes(uploadFile);
                req.ContentLength = data.Length;
                Stream stream = req.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Close();

                // FTP Upload 실행
                using (FtpWebResponse resp = (FtpWebResponse)req.GetResponse())
                {
                    return resp.StatusDescription;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string FtpDelete(string id, string pwd, string filepath)
        {
            try
            {
                FtpWebRequest req = (FtpWebRequest)WebRequest.Create(new Uri(filepath));
                req.Method = WebRequestMethods.Ftp.DeleteFile;
                req.Credentials = new NetworkCredential(id, pwd);

                // FTP Delete 실행
                using (FtpWebResponse resp = (FtpWebResponse)req.GetResponse())
                {
                    return resp.StatusDescription;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public static class Alert
    {
        static string mypath = Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);  // Application.StartupPath;

        /// <summary>
        /// Shows a client-side JavaScript alert in the browser.
        /// </summary>
        /// <param name="message">The message to appear in the alert.</param>
        public static void Show(string message)
        {
            Show(message, "");
        }

        public static void Show(string message, string title)
        {
            WriteLog("Alert.Show", message);

            // Cleans the message to allow single quotation marks
            string cleanMessage = message.Replace("'", "\\'");
            string script = "<script type=\"text/javascript\">alert('" + cleanMessage + "');</script>";

            // Gets the executing web page
            System.Web.UI.Page page = HttpContext.Current.CurrentHandler as System.Web.UI.Page;

            // Checks if the handler is a Page and that the script isn't allready on the Page
            if (page != null && !page.ClientScript.IsClientScriptBlockRegistered("alert"))
            {
                page.ClientScript.RegisterClientScriptBlock(typeof(Alert), "alert", script);
            }
        }

        public static void WriteLog(string module, string msg)
        {
            try
            {
                // 폴더가 있는지 검사하고 없으면 만든다.
                String dir = mypath + "/log/" + DateTime.Now.ToString("yyyy") + "/" + DateTime.Now.ToString("MM");
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dir);
                if (!di.Exists)
                {
                    di.Create();
                }

                // Write the string to a file.
                string str = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " > " + module + " : " + msg;
                System.IO.StreamWriter file = new System.IO.StreamWriter(dir + @"\MonitorLog_" + DateTime.Now.ToString("yyyyMMdd") + ".txt", true);
                file.WriteLine(str);
                file.Close();
            }
            catch
            {
            }
        }

        public static void WriteErr(string module, string msg)
        {
            try
            {
                // 폴더가 있는지 검사하고 없으면 만든다.
                String dir = mypath + "/log/" + DateTime.Now.ToString("yyyy") + "/" + DateTime.Now.ToString("MM");
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dir);
                if (!di.Exists)
                {
                    di.Create();
                }

                // Write the string to a file.
                string str = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " > " + module + " : " + msg;
                System.IO.StreamWriter file = new System.IO.StreamWriter(dir + @"\MonitorErr_" + DateTime.Now.ToString("yyyyMMdd") + ".txt", true);
                file.WriteLine(str);
                file.Close();
            }
            catch
            {
            }
        }

        public static void WriteHist(string module, string msg)
        {
            try
            {
                // 폴더가 있는지 검사하고 없으면 만든다.
                String dir = mypath + "/log/" + DateTime.Now.ToString("yyyy") + "/" + DateTime.Now.ToString("MM");
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dir);
                if (!di.Exists)
                {
                    di.Create();
                }

                // Write the string to a file.
                string str = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss") + " > " + module + " : " + msg;
                System.IO.StreamWriter file = new System.IO.StreamWriter(dir + @"\MonitorHist_" + DateTime.Now.ToString("yyyyMMdd") + ".txt", true);
                file.WriteLine(str);
                file.Close();
            }
            catch
            {
            }
        }
    }
}
