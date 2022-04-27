using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MetroSoft.HIS.Extensions;

namespace MetroSoft.HIS
{
    /// <summary>
    /// 자주 사용되는 변수 형식간 상호 변환 함수들을 제공합니다. 
    /// 2019-01-11 
    /// 서지호
    /// </summary>
    public class cConvert
    {
        #region Convert To Int
        public static int ToInt(string value, int defaultValue)
        {
            int result;
            double temp;

            if (int.TryParse(value, out result))
            { 
                return result;
            }
            else                
            {
                if (double.TryParse(value, out temp))
                {
                    return cConvert.ToInt(temp, defaultValue);
                }
            }
            return defaultValue;
        }

        public static int ToInt(object value, int defaultValue)
        {
            int result;
            try
            {
                result = Convert.ToInt32(value);
            }
            catch
            {
                return defaultValue;
            }

            return result;
        }

        public static int ToInt(long value, int defaultValue)
        {
            return ToInt(value as object, defaultValue);
        }
        public static int ToInt(short value, int defaultValue)
        {
            return ToInt(value as object, defaultValue);
        }

        public static int ToInt(double value, int defaultValue)
        {
            return ToInt(value as object, defaultValue);
        }

        public static int ToInt(float value, int defaultValue)
        {
            return ToInt(value as object, defaultValue);
        }

        public static int ToInt(bool value, int defaultValue)
        {
            return ToInt(value as object, defaultValue);
        }

        public static int ToInt(byte value, int defaultValue)
        {
            return ToInt(value as object, defaultValue);
        }

        public static int ToInt(char value, int defaultValue)
        {
            return ToInt(value.ToString(), defaultValue);
        }

        public static int ToInt(uint value, int defaultValue)
        {
            return ToInt(value as object, defaultValue);
        }
        public static int ToInt(ulong value, int defaultValue)
        {
            return ToInt(value as object, defaultValue);
        }

        public static int ToInt(ushort value, int defaultValue)
        {
            return ToInt(value as object, defaultValue);
        }

        public static int ToInt(string value)
        {
            return ToInt(value, 0);
        }

        public static int ToInt(object value)
        {
            return ToInt(value, 0);
        }

        public static int ToInt(long value)
        {
            return ToInt(value as object, 0);
        }
        public static int ToInt(short value)
        {
            return ToInt(value as object, 0);
        }

        public static int ToInt(double value)
        {
            return ToInt(value as object,0);
        }

        public static int ToInt(float value)
        {
            return ToInt(value as object, 0);
        }

        public static int ToInt(bool value)
        {
            return ToInt(value as object, 0);
        }

        public static int ToInt(byte value)
        {
            return ToInt(value as object, 0);
        }

        public static int ToInt(char value)
        {
            return ToInt(value.ToString(), 0);
        }

        public static int ToInt(uint value)
        {
            return ToInt(value as object, 0);
        }
        public static int ToInt(ulong value)
        {
            return ToInt(value as object, 0);
        }

        public static int ToInt(ushort value)
        {
            return ToInt(value as object, 0);
        }

        #endregion

        #region Convert To Long
        public static long ToLong(string value, int defaultValue)
        {
            int result;
            double temp;

            if (int.TryParse(value, out result))
            {
                return result;
            }
            else
            {
                if (double.TryParse(value, out temp))
                {
                    return cConvert.ToLong(temp, defaultValue);
                }
            }
            return defaultValue;
        }

        public static long ToLong(object value, int defaultValue)
        {
            long result;
            try
            {
                result = Convert.ToInt64(value);
            }
            catch
            {
                return defaultValue;
            }

            return result;
        }

        public static long ToLong(long value, int defaultValue)
        {
            return ToLong(value as object, defaultValue);
        }
        public static long ToLong(short value, int defaultValue)
        {
            return ToLong(value as object, defaultValue);
        }

        public static long ToLong(double value, int defaultValue)
        {
            return ToLong(value as object, defaultValue);
        }

        public static long ToLong(float value, int defaultValue)
        {
            return ToLong(value as object, defaultValue);
        }

        public static long ToLong(bool value, int defaultValue)
        {
            return ToLong(value as object, defaultValue);
        }

        public static long ToLong(byte value, int defaultValue)
        {
            return ToLong(value as object, defaultValue);
        }

        public static long ToLong(char value, int defaultValue)
        {
            return ToLong(value.ToString(), defaultValue);
        }

        public static long ToLong(uint value, int defaultValue)
        {
            return ToLong(value as object, defaultValue);
        }
        public static long ToLong(ulong value, int defaultValue)
        {
            return ToLong(value as object, defaultValue);
        }

        public static long ToLong(ushort value, int defaultValue)
        {
            return ToLong(value as object, defaultValue);
        }

        public static long ToLong(string value)
        {
            return ToLong(value, 0);
        }

        public static long ToLong(object value)
        {
            return ToLong(value, 0);
        }

        public static long ToLong(long value)
        {
            return ToLong(value as object, 0);
        }
        public static long ToLong(short value)
        {
            return ToLong(value as object, 0);
        }

        public static long ToLong(double value)
        {
            return ToLong(value as object, 0);
        }

        public static long ToLong(float value)
        {
            return ToLong(value as object, 0);
        }

        public static long ToLong(bool value)
        {
            return ToLong(value as object, 0);
        }

        public static long ToLong(byte value)
        {
            return ToLong(value as object, 0);
        }

        public static long ToLong(char value)
        {
            return ToLong(value.ToString(), 0);
        }

        public static long ToLong(uint value)
        {
            return ToLong(value as object, 0);
        }
        public static long ToLong(ulong value)
        {
            return ToLong(value as object, 0);
        }

        public static long ToLong(ushort value)
        {
            return ToLong(value as object, 0);
        }

        #endregion

        #region Convert To Float
        public static float ToFloat(string value, float defaultValue)
        {
            float result;
            double temp;

            if (float.TryParse(value, out result))
            { 
                return result;
            }
            else
            {
                if(double.TryParse(value, out temp))
                {
                    return cConvert.ToFloat(temp);
                }
            }

            return defaultValue;
        }

        public static float ToFloat(object value, float defaultValue)
        {
            float result;
            try
            {
                result = Convert.ToSingle(value);
            }
            catch
            {
                return defaultValue;
            }

            return result;
        }

        public static float ToFloat(long value, float defaultValue)
        {
            return ToFloat(value as object, defaultValue);
        }
        public static float ToFloat(short value, float defaultValue)
        {
            return ToFloat(value as object, defaultValue);
        }

        public static float ToFloat(double value, float defaultValue)
        {
            return ToFloat(value as object, defaultValue);
        }

        public static float ToFloat(float value, float defaultValue)
        {
            return ToFloat(value as object, defaultValue);
        }

        public static float ToFloat(bool value, float defaultValue)
        {
            return ToFloat(value as object, defaultValue);
        }

        public static float ToFloat(byte value, float defaultValue)
        {
            return ToFloat(value as object, defaultValue);
        }

        public static float ToFloat(char value, float defaultValue)
        {
            return ToFloat(value.ToString(), defaultValue);
        }

        public static float ToFloat(uint value, float defaultValue)
        {
            return ToFloat(value as object, defaultValue);
        }
        public static float ToFloat(ulong value, float defaultValue)
        {
            return ToFloat(value as object, defaultValue);
        }

        public static float ToFloat(ushort value, float defaultValue)
        {
            return ToFloat(value as object, defaultValue);
        }

        public static float ToFloat(object value)
        {
            return ToFloat(value, 0);
        }

        public static float ToFloat(string value)
        {
            return ToFloat(value, 0);
        }

        public static float ToFloat(long value)
        {
            return ToFloat(value as object, 0);
        }
        public static float ToFloat(short value)
        {
            return ToFloat(value as object, 0);
        }

        public static float ToFloat(double value)
        {
            return ToFloat(value as object, 0);
        }

        public static float ToFloat(float value)
        {
            return ToFloat(value as object, 0);
        }

        public static float ToFloat(bool value)
        {
            return ToFloat(value as object, 0);
        }

        public static float ToFloat(byte value)
        {
            return ToFloat(value as object, 0);
        }

        public static float ToFloat(char value)
        {
            return ToFloat(value.ToString(), 0);
        }

        public static float ToFloat(uint value)
        {
            return ToFloat(value as object, 0);
        }
        public static float ToFloat(ulong value)
        {
            return ToFloat(value as object, 0);
        }

        public static float ToFloat(ushort value)
        {
            return ToFloat(value as object, 0);
        }

        #endregion

        #region Convert To Double
        public static double ToDouble(string value, double defaultValue)
        {
            double result;

            if (double.TryParse(value, out result))
                return result;

            return defaultValue;
        }

        public static double ToDouble(object value, double defaultValue)
        {
            double result;
            try
            {
                result = Convert.ToSingle(value);
            }
            catch
            {
                return defaultValue;
            }

            return result;
        }

        public static double ToDouble(long value, double defaultValue)
        {
            return ToDouble(value as object, defaultValue);
        }
        public static double ToDouble(short value, double defaultValue)
        {
            return ToDouble(value as object, defaultValue);
        }

        public static double ToDouble(float value, double defaultValue)
        {
            return ToDouble(value as object, defaultValue);
        }

        public static double ToDouble(bool value, double defaultValue)
        {
            return ToDouble(value as object, defaultValue);
        }

        public static double ToDouble(byte value, double defaultValue)
        {
            return ToDouble(value as object, defaultValue);
        }

        public static double ToDouble(char value, double defaultValue)
        {
            return ToDouble(value.ToString(), defaultValue);
        }

        public static double ToDouble(uint value, double defaultValue)
        {
            return ToDouble(value as object, defaultValue);
        }
        public static double ToDouble(ulong value, double defaultValue)
        {
            return ToDouble(value as object, defaultValue);
        }

        public static double ToDouble(ushort value, double defaultValue)
        {
            return ToDouble(value as object, defaultValue);
        }

        public static double ToDouble(DateTime value, double defaultValue)
        {
            try
            {
                return value.ToOADate();
            }
            catch
            {
                return defaultValue;
            }
        }

        public static double ToDouble(object value)
        {
            return ToDouble(value, 0);
        }

        public static double ToDouble(string value)
        {
            return ToDouble(value, 0);
        }

        public static double ToDouble(long value)
        {
            return ToDouble(value as object, 0);
        }
        public static double ToDouble(short value)
        {
            return ToDouble(value as object, 0);
        }

        public static double ToDouble(float value)
        {
            return ToDouble(value as object, 0);
        }

        public static double ToDouble(bool value)
        {
            return ToDouble(value as object, 0);
        }

        public static double ToDouble(byte value)
        {
            return ToDouble(value as object, 0);
        }

        public static double ToDouble(char value)
        {
            return ToDouble(value.ToString(), 0);
        }

        public static double ToDouble(uint value)
        {
            return ToDouble(value as object, 0);
        }
        public static double ToDouble(ulong value)
        {
            return ToDouble(value as object, 0);
        }

        public static double ToDouble(ushort value)
        {
            return ToDouble(value as object, 0);
        }

        public static double ToDouble(DateTime value)
        {
            return ToDouble(value, 0);
        }
        #endregion

        #region Convert To String
        public static string ToString(object value, string defaultValue)
        {
            string result;
            try
            {
                result = Convert.ToString(value);
            }
            catch
            {
                return defaultValue;
            }

            return result;
        }

        public static string ToString(long value, string defaultValue)
        {
            return ToString(value as object, defaultValue);
        }
        public static string ToString(short value, string defaultValue)
        {
            return ToString(value as object, defaultValue);
        }

        public static string ToString(float value, string defaultValue)
        {
            return ToString(value as object, defaultValue);
        }

        public static string ToString(double value, string defaultValue)
        {
            return ToString(value as object, defaultValue);
        }

        public static string ToString(bool value, string defaultValue)
        {
            return ToString(value as object, defaultValue);
        }

        public static string ToString(byte value, string defaultValue)
        {
            return ToString(value as object, defaultValue);
        }

        public static string ToString(char value, string defaultValue)
        {
            return ToString(value.ToString(), defaultValue);
        }

        public static string ToString(uint value, string defaultValue)
        {
            return ToString(value as object, defaultValue);
        }
        public static string ToString(ulong value, string defaultValue)
        {
            return ToString(value as object, defaultValue);
        }

        public static string ToString(ushort value, string defaultValue)
        {
            return ToString(value as object, defaultValue);
        }

        public static string ToString(DateTime dt, string format, string defaultValue)
        {
            try
            {
                if (dt.Equals(DateTime.MinValue))
                    return string.Empty;

                return dt.ToString(format, DateTimeFormatInfo.CurrentInfo);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static string ToString(Time time, string format, string defaultValue)
        {
            try
            {
                if (time.Equals(Time.NullValue))
                    return string.Empty;

                return time.ToString(format);
            }
            catch
            {
                return defaultValue;
            }
        }

        public static string ToString(object value)
        {
            return ToString(value, string.Empty);
        }

        public static string ToString(long value)
        {
            return ToString(value as object, string.Empty);
        }
        public static string ToString(short value)
        {
            return ToString(value as object, string.Empty);
        }

        public static string ToString(float value)
        {
            return ToString(value as object, string.Empty);
        }

        public static string ToString(double value)
        {
            return ToString(value as object, string.Empty);
        }

        public static string ToString(bool value)
        {
            return ToString(value as object, string.Empty);
        }

        public static string ToString(byte value)
        {
            return ToString(value as object, string.Empty);
        }

        public static string ToString(char value)
        {
            return ToString(value.ToString(), string.Empty);
        }

        public static string ToString(uint value)
        {
            return ToString(value as object, string.Empty);
        }
        public static string ToString(ulong value)
        {
            return ToString(value as object, string.Empty);
        }

        public static string ToString(ushort value)
        {
            return ToString(value as object, string.Empty);
        }

        public static string ToString(DateTime value)
        {
            return ToString(value, "yyyy-MM-dd hh:mm:ss");
        }

        public static string ToString(DateTime value, string format)
        {
            return ToString(value, format, string.Empty);
        }

        public static string ToString(Time value)
        {
            return ToString(value, "hhMMss");
        }

        public static string ToString(Time value, string format)
        {
            return ToString(value, format, string.Empty);
        }
        #endregion

        #region Convert To Boolean
        public static bool ToBoolean(object value, bool defaultValue)
        {
            bool result;
            try
            {
                result = Convert.ToBoolean(value);
            }
            catch
            {
                return defaultValue;
            }

            return result;
        }

        public static bool ToBoolean(long value, bool defaultValue)
        {
            return ToBoolean(value as object, defaultValue);
        }
        public static bool ToBoolean(short value, bool defaultValue)
        {
            return ToBoolean(value as object, defaultValue);
        }

        public static bool ToBoolean(float value, bool defaultValue)
        {
            return ToBoolean(value as object, defaultValue);
        }

        public static bool ToBoolean(double value, bool defaultValue)
        {
            return ToBoolean(value as object, defaultValue);
        }

        public static bool ToBoolean(bool value, bool defaultValue)
        {
            return ToBoolean(value as object, defaultValue);
        }

        public static bool ToBoolean(byte value, bool defaultValue)
        {
            return ToBoolean(value as object, defaultValue);
        }

        public static bool ToBoolean(char value, bool defaultValue)
        {
            return ToBoolean(value as object, defaultValue);
        }

        public static bool ToBoolean(uint value, bool defaultValue)
        {
            return ToBoolean(value as object, defaultValue);
        }
        public static bool ToBoolean(ulong value, bool defaultValue)
        {
            return ToBoolean(value as object, defaultValue);
        }

        public static bool ToBoolean(ushort value, bool defaultValue)
        {
            return ToBoolean(value as object, defaultValue);
        }

        public static bool ToBoolean(string value, bool defaultValue)
        {
            return ToBoolean(value as object, defaultValue);
        }

        public static bool ToBoolean(object value)
        {
            return ToBoolean(value, true);
        }
        public static bool ToBoolean(long value)
        {
            return ToBoolean(value as object, true);
        }
        public static bool ToBoolean(short value)
        {
            return ToBoolean(value as object, true);
        }

        public static bool ToBoolean(float value)
        {
            return ToBoolean(value as object, true);
        }

        public static bool ToBoolean(double value)
        {
            return ToBoolean(value as object, true);
        }

        public static bool ToBoolean(bool value)
        {
            return ToBoolean(value as object, true);
        }

        public static bool ToBoolean(byte value)
        {
            return ToBoolean(value as object, true);
        }

        public static bool ToBoolean(char value)
        {
            return ToBoolean(value as object, true);
        }

        public static bool ToBoolean(uint value)
        {
            return ToBoolean(value as object, true);
        }
        public static bool ToBoolean(ulong value)
        {
            return ToBoolean(value as object, true);
        }

        public static bool ToBoolean(ushort value)
        {
            return ToBoolean(value as object, true);
        }

        public static bool ToBoolean(string value)
        {
            return ToBoolean(value as object, true);
        }
        #endregion

        #region Convert To DateTime
        public static DateTime ToDateTime(string value, DateTime defaultValue)
        {
            DateTime result;

            if (DateTime.TryParse(value, out result))
                return result;
            else
            {
                if(DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out result))
                    return result;
            }

            return defaultValue;
        }

        public static DateTime ToDateTime(string value, string format, DateTime defaultValue)
        {
            DateTime result;

            if (DateTime.TryParseExact(value, format, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out result))
                return result;

            return defaultValue;
        }

        public static DateTime ToDateTime(object value, DateTime defaultValue)
        {
            DateTime result;
            try
            {
                if (value is string)
                    result = cConvert.ToDateTime((string)value, defaultValue);
                else
                    result = Convert.ToDateTime(value);
            }
            catch
            {
                return defaultValue;
            }

            return result;
        }

        public static DateTime ToDateTime(object value, string format, DateTime defaultValue)
        {
            DateTime result;
            try
            {
                if (value is string)
                    result = cConvert.ToDateTime((string)value, format, defaultValue);
                else
                    result = Convert.ToDateTime(value);
            }
            catch
            {
                return defaultValue;
            }

            return result;
        }

        public static DateTime ToDateTime(object value)
        {
            return ToDateTime(value, DateTime.MinValue);
        }

        public static DateTime ToDateTime(object value, string format)
        {
            return ToDateTime(value, format, DateTime.MinValue);
        }

        public static DateTime ToDateTime(string value)
        {
            return ToDateTime(value, DateTime.MinValue);
        }

        public static DateTime ToDateTime(string value, string format)
        {
            return ToDateTime(value, format, DateTime.MinValue);
        }
        #endregion

        #region Convert To Time
        public static Time ToTime(string value, string[] formats, Time defaultValue)
        {
            Time result;
            if (Time.TryParse(value, formats, null, out result))
                return result;

            return defaultValue;
        }
        public static Time ToTime(string value, string format, Time defaultValue)
        {
            Time result;
            if (Time.TryParse(value, format, null, out result))
                return result;

            return defaultValue;
        }

        public static Time ToTime(string value, Time defaultValue)
        {
            return ToTime(value, "HHmmss", defaultValue);
        }

        public static Time ToTime(DateTime value, Time defaultValue)
        {
            try
            {
                DateTime dt = (DateTime)value;
                return new Time(dt.Hour, dt.Minute, dt.Second, dt.Millisecond);
            }
            catch
            {
                return defaultValue;
            }
        }
        
        public static Time ToTime(string value, string format)
        {
            return ToTime(value, format, Time.NullValue);
        }
        public static Time ToTime(object value, string[] formats)
        {
            Time result;
            foreach (string f in formats)
            {
                try
                {
                    result = ToTime(value, f);
                }
                catch
                {
                    result = Time.NullValue;
                }

                if (result != Time.NullValue)
                    return result;
            }
            throw new InvalidCastException("Cannot convert to Time.");
        }

        public static Time ToTime(object value, string format)
        {
            if (value is string)
            {
                return ToTime((string)value, format, Time.NullValue);
            }
            else if (value is DateTime)
            {
                return ToTime((DateTime)value, Time.NullValue);
            }
            else if (cConvert.isNullOrEmpty(value))
                return Time.NullValue;
            else
                throw new InvalidCastException("Cannot convert to Time.");
        }

        public static Time ToTime(string value)
        {
            return ToTime(value, Time.NullValue);
        }

        public static Time ToTime(object value)
        {
            return ToTime(value, "HHmmss");
        }

        public static bool isNullOrEmpty(object obj)
        {
            return (obj == null)
                || ((obj != null) && obj.Equals(DBNull.Value))
                || ((obj is string) && ((string)obj).Length == 0)
                || ((obj is DateTime) && ((DateTime)obj).Equals(DateTime.MinValue))
                || ((obj is Time) && ((Time)obj).Equals(Time.NullValue));
        }
        #endregion
    }
}
