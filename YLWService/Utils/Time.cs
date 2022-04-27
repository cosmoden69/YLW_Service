using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace MetroSoft.HIS
{

    public enum TimeComparePrecision
    {
        Hour,
        Minute,
        Second,
        Millisecond
    }

    [DebuggerDisplay("Time = {ToString(\"HH:mm:ss\")}")]
    public struct Time : IComparable, IFormattable, IConvertible, ISerializable, IComparable<Time>, IEquatable<Time>
    {
        #region Fields
        private int _hours;
        private int _minutes;
        private int _seconds;
        private int _milliseconds;
        #endregion

        #region Constants
        public static Time Midnight
        {
            get
            {
                return new Time(0, 0, 0);
            }
        }

        public static Time Noon
        {
            get
            {
                return new Time(12, 0, 0);
            }
        }

        public static Time NullValue
        {
            get { return new Time(-1, -1, -1, -1); }
        }
        #endregion

        #region Properties
        public int Hours
        {
            get { return _hours; }
            set { SetHours(value); }
        }

        public int Minutes
        {
            get { return _minutes; }
            set { SetMinutes(value); }
        }

        public int Seconds
        {
            get { return _seconds; }
            set { SetSeconds(value); }
        }

        public int Milliseconds
        {
            get { return _milliseconds; }
            set { SetMilliseconds(value); }
        }

        #endregion

        #region Constructors
        public Time(string s, string format)
            : this(s, format, null)
        { }

        public Time(string s, string format, IFormatProvider provider = null)
        {
            if (provider == null) provider = CultureInfo.CurrentCulture;

            try
            {
                _hours = -1;
                _minutes = -1;
                _seconds = -1;
                _milliseconds = -1;

                DateTime dt = DateTime.ParseExact(s, format, provider);

                _hours = dt.Hour;
                _minutes = dt.Minute;
                _seconds = dt.Second;
                _milliseconds = dt.Millisecond;
            }
            catch
            {
                _hours = -1;
                _minutes = -1;
                _seconds = -1;
                _milliseconds = -1;

                throw new InvalidCastException("Cannot convert to Time from string.");
            }
        }

        public Time(int hour, int minute, int second)
        {
            _hours = 0;
            _minutes = 0;
            _seconds = 0;
            _milliseconds = 0;

            SetTime(hour, minute, second, 0);
        }

        public Time(int hour, int minute, int second, int millisecond)
        {
            _hours = 0;
            _minutes = 0;
            _seconds = 0;
            _milliseconds = 0;

            SetTime(hour, minute, second, millisecond);
        }

        public Time(DateTime time)
        {
            _hours = time.Hour;
            _minutes = time.Minute;
            _seconds = time.Second;
            _milliseconds = time.Millisecond;
        }

        public Time(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");

            _hours = (int)info.GetValue("Hours", typeof(int));
            _minutes = (int)info.GetValue("Minutes", typeof(int));
            _seconds = (int)info.GetValue("Seconds", typeof(int));
            _milliseconds = (int)info.GetValue("Milliseconds", typeof(int));
        }
        #endregion

        #region ISerializable
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new System.ArgumentNullException("info");
            info.AddValue("Hours", this.Hours);
            info.AddValue("Minutes", this.Minutes);
            info.AddValue("Seconds", this.Seconds);
            info.AddValue("Milliseconds", this.Milliseconds);
        }

        public override int GetHashCode()
        {
            return 0;
        }
        
        public TypeCode GetTypeCode()
        {
            return TypeCode.DateTime;
        }
        #endregion

        #region Set functions
        private void SetTime(int hour, int minute, int second, int millisecond)
        {
            Hours = hour;
            Minutes = minute;
            Seconds = second;
            Milliseconds = millisecond;
        }

        private void SetHours(int hours)
        {
            _hours = hours % 24;
        }

        private void SetMinutes(int minutes)
        {
            _minutes = minutes % 60;
        }

        private void SetSeconds(int seconds)
        {
            _seconds = seconds % 60;
        }

        private void SetMilliseconds(int milliseconds)
        {
            _milliseconds = milliseconds % 1000;
        }
        #endregion

        #region Add Time functions
        public Time AddHours(double value)
        {
            if (this.Equals(Time.NullValue))
                throw new InvalidOperationException("Cannot add value to NullValue Time.");

            int delta = (int)Math.Truncate(value);
            Time result = new Time(Hours, Minutes, Seconds, Milliseconds);
            result.SetHours(this.Hours + (delta % 24) + (24 * ((delta % 24) < 0 ? 1 : 0)));

            return result;
        }        
        public Time AddMinutes(double value)
        {
            if (this.Equals(Time.NullValue))
                throw new InvalidOperationException("Cannot add value to NullValue Time.");

            int delta = (int)Math.Truncate(value);

            int minuteDelta = delta % 60;
            int hourDelta = (delta) / 60 + (minuteDelta + Minutes) / 60;
            
            Time result = new Time(Hours, Minutes, Seconds, Milliseconds);
            result = result.AddHours(hourDelta - (1 * (this.Minutes + minuteDelta < 0 ? 1 : 0)));
            result.SetMinutes(this.Minutes + (minuteDelta % 60) + (60 * ((minuteDelta % 60) < 0 ? 1 : 0)));                

            return result;
        }
        public Time AddSeconds(double value)
        {
            if (this.Equals(Time.NullValue))
                throw new InvalidOperationException("Cannot add value to NullValue Time.");

            int delta = (int)Math.Truncate(value);
            int secondDelta = delta % 60;
            int minuteDelta = (delta / 60) + (secondDelta + Seconds) / 60;            

            Time result = new Time(Hours, Minutes, Seconds, Milliseconds);
            result = result.AddMinutes(minuteDelta - (1 * (this.Seconds + secondDelta < 0 ? 1 : 0)));
            result.SetSeconds(this.Seconds + (secondDelta % 60) + (60 * ((secondDelta % 60) < 0 ? 1 : 0)));
            
            return result;
        }
        public Time AddMilliseconds(double value)
        {
            if (this.Equals(Time.NullValue))
                throw new InvalidOperationException("Cannot add value to NullValue Time.");

            int delta = (int)Math.Truncate(value);
            int millisecondDelta = delta % 1000;
            int secondDelta = (delta / 1000) + ((millisecondDelta + Milliseconds) / 1000);

            Time result = new Time(Hours, Minutes, Seconds, Milliseconds);
            result = result.AddSeconds(secondDelta - (1 * (this.Milliseconds + millisecondDelta < 0 ? 1 : 0)));
            result.SetMilliseconds(this.Milliseconds + (millisecondDelta % 1000) + (1000 * ((millisecondDelta % 1000) < 0 ? 1 : 0)));

            return result;
        }
        #endregion

        #region Operators
        public static Time operator +(Time t, TimeSpan s)
        {
            return t.AddMilliseconds(s.TotalMilliseconds);
        }
        public static TimeSpan operator -(Time t1, Time t2)
        {
            Time tt1, tt2;
            int signed = 1;
            int hDelta, mDelta, sDelta, msDelta;

            if (t1 < t2)
            {
                tt1 = t2;
                tt2 = t1;
                signed = -1;
            }
            else
            {
                tt1 = t1;
                tt2 = t2;
            }

            hDelta = tt1.Hours - tt2.Hours;
            mDelta = tt1.Minutes - tt2.Minutes;
            sDelta = tt1.Seconds - tt2.Seconds;
            msDelta = t1.Milliseconds - t2.Milliseconds;

            return new TimeSpan(0, hDelta * signed, mDelta * signed, sDelta * signed, msDelta * signed);
        }
        public static Time operator -(Time t, TimeSpan s)
        {
            return t.AddMilliseconds(0 - s.TotalMilliseconds);
        }
        public static bool operator ==(Time t1, Time t2)
        {
            return t1.Equals(t2);
        }
        public static bool operator !=(Time t1, Time t2)
        {
            return !t1.Equals(t2);
        }
        public static bool operator <(Time t1, Time t2)
        {
            return (t1.CompareTo(t2) < 0);
        }
        public static bool operator >(Time t1, Time t2)
        {
            return (t1.CompareTo(t2) > 0);
        }
        public static bool operator <=(Time t1, Time t2)
        {
            return (t1.CompareTo(t2) <= 0);
        }
        public static bool operator >=(Time t1, Time t2)
        {
            return (t1.CompareTo(t2) >= 0);
        }

        #endregion

        #region Compare functions

        public int CompareTo(object obj)
        {
            if (obj is Time)
            {
                return CompareTo((Time)obj);
            }
            throw new InvalidOperationException(String.Format("Time instances Cannot compare with {1}.", obj.GetType()));
        }

        public int CompareTo(Time other)
        {
            if (this.Hours > other.Hours)
                return 1;
            else if (this.Hours < other.Hours)
                return -1;
            else
            {
                if (this.Minutes > other.Minutes)
                    return 1;
                else if (this.Minutes < other.Minutes)
                    return -1;
                else
                {
                    if (this.Seconds > other.Seconds)
                        return 1;
                    else if (this.Seconds < other.Seconds)
                        return -1;
                    else
                    {
                        if (this.Milliseconds > other.Milliseconds)
                            return 1;
                        else if (this.Milliseconds < other.Milliseconds)
                            return -1;
                        else
                            return 0;
                    }
                }
            }
        }

        public bool Equals(Time other)
        {
            return Equals(other, TimeComparePrecision.Millisecond);
        }

        public bool Equals(Time other, TimeComparePrecision precision)
        {
            switch(precision)
            {
                case TimeComparePrecision.Hour:
                    return (this.Hours == other.Hours);
                case TimeComparePrecision.Minute:
                    return (this.Hours == other.Hours && this.Minutes == other.Minutes);
                case TimeComparePrecision.Second:
                    return (this.Hours == other.Hours && this.Minutes == other.Minutes && this.Seconds == other.Seconds);
                default:
                    return (this.Hours == other.Hours && this.Minutes == other.Minutes && this.Seconds == other.Seconds && this.Milliseconds == other.Milliseconds);
            }
        }

        public override bool Equals(object o)
        {
            if (o is Time)
            {
                return Equals((Time)o);
            }

            return false;
        }

        #endregion
        
        #region DateTime <-> String

        public DateTime ToDateTime(IFormatProvider provider)
        {
            return new DateTime(0, 0, 0, this.Hours, this.Minutes, this.Seconds, this.Milliseconds);
        }

        public override string ToString()
        {
            return this.ToString("T", CultureInfo.CurrentCulture);
        }

        public string ToString(string format)
        {
            return this.ToString(format, CultureInfo.CurrentCulture);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (this.Equals(Time.NullValue))
                return "";

            if (String.IsNullOrEmpty(format)) format = "T";
            if (formatProvider == null) formatProvider = CultureInfo.CurrentCulture;

            if (format.ToUpper() != "D"
                && format.ToUpper() != "F"
                && format.ToUpper() != "G"
                && format.ToUpper() != "M"
                && format.ToUpper() != "O"
                && format.ToUpper() != "R"
                && format.ToUpper() != "S"
                && format.ToUpper() != "U"
                && format.ToUpper() != "Y")
            {
                if (format.ToUpper() == "T"
                || format.Contains("h")
                || format.Contains("m")
                || format.Contains("s")
                || format.Contains("f")
                || format.Contains("t"))
                {
                    return new DateTime(1, 1, 3, Hours, Minutes, Seconds, Milliseconds).ToString(format, formatProvider);
                }
            }

            throw new FormatException(String.Format("The {0} format string is not supported.", format));
        }

        public static Time Parse(string s, string format, IFormatProvider provider)
        {
            return new Time(s, format, provider);
        }

        public static bool TryParse(string s, string format, IFormatProvider provider, out Time result)
        {
            if (provider == null) provider = CultureInfo.CurrentCulture;
            
            try
            {
                DateTime dt = DateTime.Now;

                if (DateTime.TryParseExact(s, format, provider, DateTimeStyles.AssumeLocal, out dt))
                {
                    result._hours = dt.Hour;
                    result._minutes = dt.Minute;
                    result._seconds = dt.Second;
                    result._milliseconds = dt.Millisecond;

                    return true;
                }

                result = Time.NullValue;
                return false;
            }
            catch
            {
                result = Time.NullValue;
                return false;
            }
        }

        public static Time Parse(string s, string[] formats, IFormatProvider provider)
        {
            foreach(string f in formats)
            {
                try
                {
                    return new Time(s, f, provider);
                }
                catch
                { }
            }

            throw new FormatException("Cannot parse string to Time.");
        }

        public static bool TryParse(string s, string[] formats, IFormatProvider provider, out Time result)
        {
            foreach (string f in formats)
            {
                try
                {
                    if (TryParse(s, f, provider, out result))
                        return true;
                }
                catch
                { }
            }

            result = Time.NullValue;
            return false;
        }

        public string ToString(IFormatProvider provider)
        {
            if (this.Equals(Time.NullValue))
                return "";

            return ToString("T", provider);
        }

        #endregion
        
        #region Convert to ~~~
        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert Time to boolean.");
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert Time to byte.");
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert Time to char.");
        }
        
        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert Time to decimal.");
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert Time to double.");
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert Time to Int16.");
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert Time to Int32.");
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert Time to Int64.");
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert Time to SByte.");
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert Time to Single.");
        }
        
        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(DateTime))
            {
                return new DateTime(0, 0, 0, Hours, Minutes, Seconds, Milliseconds);
            }

            throw new InvalidCastException(String.Format("Cannot convert Time to {0}", conversionType.Name));
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert Time to UInt16.");
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert Time to UInt32.");
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            throw new InvalidCastException("Cannot convert Time to UInt64.");
        }
        #endregion
    }
}
