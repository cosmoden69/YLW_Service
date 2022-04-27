using System;
using System.Data;

namespace MetroSoft.HIS.Utils
{
    public class SqlUtil
    {
        /// <summary>
        /// C#에서 사용되는 Data Type을 호환되는 가장 가까운 SqlDbType 형식으로 반환합니다.
        /// </summary>
        /// <param name="type">SqlDbType으로 변환하고자 하는 C# 내의 Type입니다.</param>
        /// <returns>해당 Type과 가장 호환성이 높은 SqlDbType Enum 값입니다.</returns>
        public static SqlDbType ToSqlDbType(Type type)
        {
            if (type == typeof(Int32)
                || type == typeof(UInt32))
                return SqlDbType.Int;
            else if (type == typeof(Int16)
                || type == typeof(UInt16))
                return SqlDbType.SmallInt;
            else if (type == typeof(Byte))
                return SqlDbType.TinyInt;
            else if (type == typeof(Int64)
                || type == typeof(UInt64))
                return SqlDbType.BigInt;
            else if (type == typeof(Boolean))
                return SqlDbType.Bit;
            else if (type == typeof(String))
                return SqlDbType.VarChar;
            else if (type == typeof(Char))
                return SqlDbType.Char;
            else if (type == typeof(DateTime))
                return SqlDbType.DateTime;
            else if (type == typeof(Time))
                return SqlDbType.Time;
            else if (type == typeof(byte[]))
                return SqlDbType.VarBinary;
            else if (type == typeof(Guid))
                return SqlDbType.UniqueIdentifier;
            else if (type == typeof(Double))
                return SqlDbType.Float;
            else if (type == typeof(Single))
                return SqlDbType.Real;
            else if (type == typeof(Decimal))
                return SqlDbType.Decimal;
            else if (type == typeof(Object))
                return SqlDbType.Variant;
            else
                return SqlDbType.VarChar;
        }

        public static bool CanAssignLength(SqlDbType type)
        {
            switch(type)
            {
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.VarChar:
                case SqlDbType.NVarChar:                
                    return true;
                default:
                    return false;
            }
        }
        /// <summary>
        /// DB TYPE 문자열을 SqlDbType enum 형식으로 변환하여 줍니다.
        /// </summary>
        /// <param name="typeStr">MS-SQL DB의 Column Type 문자열입니다.</param>
        /// <returns>일치하는 SqlDbType enum 값입니다.</returns>
        public static SqlDbType ToSqlDbType(string typeStr)
        {
            string t = typeStr.ToUpper();

            if (t == "BIGINT")
                return SqlDbType.BigInt;
            else if (t == "NUMERIC")
                return SqlDbType.Decimal;
            else if (t == "BIT")
                return SqlDbType.Bit;
            else if (t == "SMALLINT")
                return SqlDbType.SmallInt;
            else if (t == "DECIMAL")
                return SqlDbType.Decimal;
            else if (t == "SMALLMONEY")
                return SqlDbType.SmallMoney;
            else if (t == "INT")
                return SqlDbType.Int;
            else if (t == "TINYINT")
                return SqlDbType.TinyInt;
            else if (t == "MONEY")
                return SqlDbType.Money;
            else if (t == "FLOAT")
                return SqlDbType.Float;
            else if (t == "REAL")
                return SqlDbType.Real;
            else if (t == "DATE")
                return SqlDbType.Date;
            else if (t == "DATETIMEOFFSET")
                return SqlDbType.DateTimeOffset;
            else if (t == "DATETIME2")
                return SqlDbType.DateTime2;
            else if (t == "SMALLDATETIME")
                return SqlDbType.SmallDateTime;
            else if (t == "DATETIME")
                return SqlDbType.DateTime;
            else if (t == "TIME")
                return SqlDbType.Time;
            else if (t == "CHAR")
                return SqlDbType.Char;
            else if (t == "VARCHAR")
                return SqlDbType.VarChar;
            else if (t == "TEXT")
                return SqlDbType.Text;
            else if (t == "NCHAR")
                return SqlDbType.NChar;
            else if (t == "NVARCHAR")
                return SqlDbType.NVarChar;
            else if (t == "NTEXT")
                return SqlDbType.NText;
            else if (t == "BINARY")
                return SqlDbType.Binary;
            else if (t == "VARBINARY")
                return SqlDbType.VarBinary;
            else if (t == "IMAGE")
                return SqlDbType.Image;
            else
                return SqlDbType.VarChar;
        }

        /// <summary>
        /// DB TYPE 문자열을 호환되는 C# 내의 type 형식으로 변환하여 줍니다.
        /// </summary>
        /// <param name="typeStr">MS-SQL DB의 Column Type 문자열입니다.</param>
        /// <returns>호환되는 C# type 형식입니다.</returns>
        public static Type ToType(string typeStr)
        {
            string t = typeStr.ToUpper();

            switch(t)
            {
                case "BIGINT":                
                case "SMALLINT":
                case "INT":
                case "TINYINT":
                    return typeof(int);

                case "NUMERIC":
                case "DECIMAL":
                case "SMALLMONEY":
                case "MONEY":
                case "FLOAT":
                case "REAL":
                    return typeof(double);

                case "DATE":                
                case "DATETIME2":
                case "SMALLDATETIME":
                case "DATETIME":
                    return typeof(DateTime);

                case "DATETIMEOFFSET":
                    return typeof(TimeSpan);

                case "TIME":
                    return typeof(Time);

                case "CHAR":
                case "VARCHAR":
                case "TEXT":
                case "NCHAR":
                case "NVARCHAR":
                case "NTEXT":
                    return typeof(string);

                case "BINARY":
                case "VARBINARY":
                case "IMAGE":
                    return typeof(byte[]);

                case "BIT":
                    return typeof(bool);

                default:
                    return typeof(string);
            }
        }

        public static Type ToType(SqlDbType dbType)
        {
            switch(dbType)
            {
                case SqlDbType.BigInt:
                case SqlDbType.Int:
                case SqlDbType.SmallInt:
                case SqlDbType.TinyInt:
                    return typeof(int);

                case SqlDbType.Binary:
                case SqlDbType.Image:
                case SqlDbType.VarBinary:
                    return typeof(byte[]);

                case SqlDbType.Bit:
                    return typeof(bool);

                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                    return typeof(string);

                case SqlDbType.DateTime:
                case SqlDbType.SmallDateTime:
                case SqlDbType.Timestamp:                
                case SqlDbType.Date:                
                case SqlDbType.DateTime2:
                case SqlDbType.DateTimeOffset:
                    return typeof(DateTime);

                case SqlDbType.Time:
                    return typeof(Time);

                case SqlDbType.Decimal:
                case SqlDbType.Float:
                case SqlDbType.Money:
                case SqlDbType.Real:
                case SqlDbType.SmallMoney:
                    return typeof(double);

                case SqlDbType.UniqueIdentifier:
                    return typeof(Guid);
                
                case SqlDbType.Variant:
                    return typeof(object);

                case SqlDbType.Xml:
                case SqlDbType.Udt:
                case SqlDbType.Structured:
                default:
                    return null;
            }
        }
    }
}
