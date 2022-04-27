using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Security;
using System.Windows.Forms;
using System.Xml;

using MetroSoft.HIS.Utils;

namespace MetroSoft.HIS
{
    public class DataAccessObject
    {
        internal static string connectionString = "";


        static DataAccessObject()
        {
            XmlNode conns = ConfigurationManager.Global["connectionStrings"];
            string key;
            if (conns != null)
            {
                foreach (XmlElement conn in conns.ChildNodes)
                {
                    key = conn.GetAttribute("Name");
                    connectionString = conn.GetAttribute("ConnectString");
                    bool isEncryption = (conn.GetAttribute("Encrypt") == "On" ? true : false);
                    ////연결문자열이 암호화되지 않았으면 암호화 실행
                    //if (!isEncryption)
                    //{
                    //    string encstr = cEncryptionUtil.EncryptString(connectionString);
                    //    ConfigurationManager.Global.SetConfigStringNode(conns, "connection", "ConnectString", encstr);
                    //    ConfigurationManager.Global.SetConfigStringNode(conns, "connection", "Encrypt", "On");
                    //}
                    //else connectionString = cEncryptionUtil.DecryptString(connectionString).Trim('\0');
                    //암호화 해제
                    if (isEncryption)
                    {
                        connectionString = cEncryptionUtil.DecryptString(connectionString).Trim('\0');
                        ConfigurationManager.Global.SetConfigStringNode(conns, "connection", "ConnectString", connectionString);
                        ConfigurationManager.Global.SetConfigStringNode(conns, "connection", "Encrypt", "Off");
                    }
                }
            }
        }

        static string GetConnectionString()
        {
            return connectionString;
        }

        public static IDbDataParameter[] SqlParamsMapping(string sqlText)
        {
            return SqlParamsMapping(sqlText, "@");
        }


        public static IDbDataParameter[] SqlParamsMapping(string sqlText, string symbol)
        {
            int pos = -1;
            int startIndex = 0;
            int index = startIndex;
            int cmtPos;
            bool loopExit = false;

            List<IDbDataParameter> bindVarNameList = new List<IDbDataParameter>();

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
                        bindVarNameList.Add(new SqlParameter() { ParameterName = symbol + varName, SourceColumn = varName });
                        loopExit = true;
                        break;
                    }
                    else if (IsDelimiter(sqlText[i]))
                    {
                        string varName = sqlText.Substring(pos + 1, i - pos - 1);
                        if (varName == "")
                            varName = "Var" + (bindVarNameList.Count + 1).ToString();
                        bindVarNameList.Add(new SqlParameter() { ParameterName = symbol + varName, SourceColumn = varName });
                        startIndex = i;
                        index = startIndex;
                        loopExit = false;
                        break;
                    }
                }
            }

            return bindVarNameList.ToArray();
        }

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

        private static bool IsDelimiter(char ch)
        {
            return !((char.IsLetterOrDigit(ch)) || (ch == '_'));
        }

        #region 커넥션 생성
        public static SqlConnection GetConnection()
        {
            //새로운 커넥션이 열릴경우 전자인증처리시 방금 입력한 데이타에 대한 HDID 못구해옴
            if (_conn != null)
            {
                return _conn;
            }
            _conn = new SqlConnection(GetConnectionString());
            //SqlConnection conn = new SqlConnection(GetConnectionString());

            //conn.Credential = GetCredential();

            return _conn;

        }

        public static SqlConnection GetNewConnection()
        {
            return new SqlConnection(GetConnectionString());
        }

        //20200114 정민석 추가 (connect Pool 초과 오류) (프로그램에서 처리용)
        public static SqlConnection _conn = null;
        public static void _GetConnection()
        {
            _conn = new SqlConnection(GetConnectionString());
        }

        //벡그라운드 처리용 커넥션 (다른 스레드처리용)
        public static SqlConnection _connD = null;
        public static void _GetConnectionD()
        {
            if (_connD == null)
            {
                _connD = new SqlConnection(GetConnectionString());
            }
        }

        //벡그라운드 처리용 커넥션 (다른 스레드처리용)
        public static SqlConnection _connMsg = null;
        public static void _GetConnectionMsg()
        {
            if (_connMsg == null)
            {
                _connMsg = new SqlConnection(GetConnectionString());
            }
        }

        #endregion

        #region ExecuteDataTable

        /// <summary>
        /// CommandText와 CommandType 을 모두 받아서 Query 결과를 DataTable로 리턴합니다. 
        /// 이 함수는 Connection 을 외부에서 받아서 사용합니다. Transaction 의 중간에 질의가 필요할 때 사용하세요.
        /// </summary>
        /// <param name="conn">Sql Connection 인스턴스</param>
        /// <param name="tran">conn으로부터 생성한 Transaction 인스턴스</param>
        /// <param name="cmdText">커맨드 SQL 질의문</param>  
        /// <param name="cmdType">커맨드 SQL 질의의 종류.</param>
        /// <param name="parameters">SQL 질의문의 파라메터 목록</param>
        /// <returns>SQL 질의의 결과 DataTable</returns>
        public static DataTable ExecuteDataTable(SqlConnection conn, SqlTransaction tran, string cmdText, CommandType cmdType, IDbDataParameter[] parameters)
        {
            SqlCommand cmd = new SqlCommand();

            DataTransactionManager dtm = DataTransactionManager.GetCurrent();
            if (dtm == null)
            {
                conn = (conn == null ? _conn : conn); //20200114 정민석 추가 (connect Pool 초과 오류)
                if (conn == null)
                    conn = GetConnection();
            }
            else
            {
                if (dtm.TransactionBegun)
                    conn = DataTransactionManager.GetCurrent().Connection;
                else
                {
                    conn = GetConnection();
                }
            }

            //cmdText = cmdText.Replace(Environment.NewLine, " ").Replace("\r", " ").Replace("\t", " "); //주석에 오류 남.
            if (parameters != null)
            {
                cmdText = cmdText.Replace("\t ", " ");
                cmdText = cmdText.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
                cmdText = cmdText.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
                cmdText = cmdText.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
                cmdText = cmdText.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
                cmdText = cmdText.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            }

            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;

            if (tran != null)
                cmd.Transaction = tran;

            if (parameters != null)
            {
                //중복파라메타 제거
                Dictionary<string, bool> dicDup = new Dictionary<string, bool>();
                IDbDataParameter[] paraAdjust = new IDbDataParameter[0];
                for (int ii = 0; ii < parameters.Length; ii++)
                {
                    if (dicDup.ContainsKey(parameters[ii].ParameterName) == false)
                    {
                        Array.Resize(ref paraAdjust, paraAdjust.Length + 1);
                        paraAdjust[paraAdjust.Length - 1] = parameters[ii];
                        dicDup.Add(parameters[ii].ParameterName, true);
                    }
                }
                parameters = paraAdjust;

                cmd.Parameters.AddRange(parameters);
            }


            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            try
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();


                adapter.Fill(dt);
            }
            catch (SqlException ex)
            {
#if DEBUG
                string sSQL = GetSQL(cmdText, parameters);

                cFile.WriteLog("", cmdText);
#endif
                throw ex;
            }
            catch
            {
                throw;
            }
            finally
            {
                //파라메타 재사용.
                cmd.Parameters.Clear(); //sqlparameter이(가) 이미 다른 sqlparametercollection에 들어 있습니다
                cmd.Dispose();
                adapter.Dispose();

            }

            //컬럼대문자로 통일.
            for (int ii = 0; ii < dt.Columns.Count; ii++)
            {
                dt.Columns[ii].ColumnName = dt.Columns[ii].ColumnName.ToUpper();
            }
            return dt;
        }



        /// <summary>
        /// CommandText와 CommandType 을 모두 받아서 Query 결과를 DataTable로 리턴합니다. 
        /// 이 함수는 Connection 을 외부에서 받아서 사용합니다. TransactionScope 등으로 트랜잭션을 별도 관리시 사용하세요.
        /// </summary>
        /// <param name="conn">Sql Connection 인스턴스</param>        
        /// <param name="cmdText">커맨드 SQL 질의문</param>  
        /// <param name="cmdType">커맨드 SQL 질의의 종류.</param>
        /// <param name="parameters">SQL 질의문의 파라메터 목록</param>
        /// <returns>SQL 질의의 결과 DataTable</returns>
        public static DataTable ExecuteDataTable(SqlConnection conn, string cmdText, CommandType cmdType, IDbDataParameter[] parameters)
        {
            return ExecuteDataTable(
                conn
                , null
                , cmdText
                , cmdType
                , parameters);
        }


        /// <summary>
        /// CommandText와 CommandType 을 모두 받아서 Query 결과를 DataTable로 리턴합니다. 
        /// 이 함수는 Connection 을 직접 생성합니다. 따라서 Transaction 의 중간에서 사용할 수 없습니다. 
        /// </summary>
        /// <param name="cmdText">커맨드 SQL 질의문</param>
        /// <param name="cmdType">SQL 질의의 종류</param>
        /// <param name="parameters">SQL 질의문의 파라메터 목록</param>
        /// <returns>SQL 질의의 결과 DataTable</returns>
        public static DataTable ExecuteDataTable(string cmdText, CommandType cmdType, IDbDataParameter[] parameters)
        {
            return ExecuteDataTable(
                    null
                    , cmdText
                    , cmdType
                    , parameters);
        }

        /// <summary>
        /// CommandText를 입력 받아서 Query 결과를 DataTable로 리턴합니다. 
        /// 이 함수는 Connection, Transaction 을 외부에서 받아서 사용합니다. Transaction 을 직접 관리할 때 사용하세요.
        /// 이 함수는 CommandType 을 무조건 Text 형식으로 설정합니다. Stored Procedure 문에 사용하지 마십시오.
        /// </summary>
        /// <param name="conn">Sql Connection 인스턴스</param>
        /// <param name="tran">conn으로부터 생성한 Transaction 인스턴스</param>
        /// <param name="cmdText">커맨드 SQL 질의문</param>        
        /// <param name="parameters">SQL 질의문의 파라메터 목록</param>
        /// <returns>SQL 질의의 결과 DataTable</returns>
        public static DataTable ExecuteDataTable(SqlConnection conn, SqlTransaction tran, string cmdText, IDbDataParameter[] parameters)
        {
            return ExecuteDataTable(
                conn
                , tran
                , cmdText
                , CommandType.Text
                , parameters);
        }

        /// <summary>
        /// CommandText를 입력 받아서 Query 결과를 DataTable로 리턴합니다. 
        /// 이 함수는 Connection만 외부에서 받아서 사용합니다. TransactionScope 등으로 별도 트랜잭션 관리시 사용하세요.
        /// 만약 conn 에 null 을 넣을 경우 자체적으로 생성한 Connection 을 사용합니다.
        /// 이 함수는 CommandType 을 무조건 Text 형식으로 설정합니다. Stored Procedure 문에 사용하지 마십시오.
        /// </summary>
        /// <param name="conn">Sql Connection 인스턴스</param>        
        /// <param name="cmdText">커맨드 SQL 질의문</param>        
        /// <param name="parameters">SQL 질의문의 파라메터 목록</param>
        /// <returns>SQL 질의의 결과 DataTable</returns>
        public static DataTable ExecuteDataTable(SqlConnection conn, string cmdText, IDbDataParameter[] parameters)
        {
            return ExecuteDataTable(
                conn
                , null
                , cmdText
                , CommandType.Text
                , parameters);
        }

        /// <summary>
        /// CommandText를 받아서 Query 결과를 DataTable로 리턴합니다. 
        /// 이 함수는 Connection 을 직접 생성합니다. 따라서 Transaction 의 중간에서 사용할 수 없습니다. 
        /// 이 함수는 CommandType 을 무조건 Text 형식으로 설정합니다. Stored Procedure 문에 사용하지 마십시오.
        /// </summary>
        /// <param name="cmdText">커맨드 SQL 질의문</param>        
        /// <param name="parameters">SQL 질의문의 파라메터 목록</param>
        /// <returns>SQL 질의의 결과 DataTable</returns>
        public static DataTable ExecuteDataTable(string cmdText, IDbDataParameter[] parameters)
        {

            return ExecuteDataTable(
                null
                , cmdText
                , parameters);
        }

        /// <summary>
        /// CommandText를 받아서 Query 결과를 DataTable로 리턴합니다. 
        /// 이 함수는 Connection, Transaction 을 외부에서 받아서 사용합니다. Transaction을 직접 관리시 사용하세요.
        /// 이 함수는 CommandType 을 무조건 Text 형식으로 설정합니다. Stored Procedure 문에 사용하지 마십시오.
        /// 이 함수는 Parameter를 받지 않습니다. ADHOC 용...
        /// </summary>
        /// <param name="conn">Sql Connection 인스턴스</param>
        /// <param name="tran">conn으로부터 생성한 Transaction 인스턴스</param>
        /// <param name="cmdText">커맨드 SQL 질의문</param>                
        /// <returns>SQL 질의의 결과 DataTable</returns>
        public static DataTable ExecuteDataTable(SqlConnection conn, SqlTransaction tran, string cmdText)
        {
            return ExecuteDataTable(
                conn
                , tran
                , cmdText
                , CommandType.Text
                , null);
        }

        /// <summary>
        /// CommandText를 받아서 Query 결과를 DataTable로 리턴합니다. 
        /// 이 함수는 Connection을 외부에서 받아서 사용합니다. TransactionScope 등으로 트랜잭션을 별도 관리시 사용하세요.
        /// 이 함수는 CommandType 을 무조건 Text 형식으로 설정합니다. Stored Procedure 문에 사용하지 마십시오.
        /// 이 함수는 Parameter를 받지 않습니다. ADHOC 용...
        /// </summary>
        /// <param name="conn">Sql Connection 인스턴스</param>        
        /// <param name="cmdText">커맨드 SQL 질의문</param>                
        /// <returns>SQL 질의의 결과 DataTable</returns>
        public static DataTable ExecuteDataTable(SqlConnection conn, string cmdText)
        {
            return ExecuteDataTable(
                conn
                , cmdText
                , null);
        }

        /// <summary>
        /// CommandText를 받아서 Query 결과를 DataTable로 리턴합니다. 
        /// 이 함수는 Connection 을 직접 생성합니다. 따라서 Transaction 의 중간에서 사용할 수 없습니다. 
        /// 이 함수는 CommandType 을 무조건 Text 형식으로 설정합니다. Stored Procedure 문에 사용하지 마십시오.
        /// 이 함수는 Parameter를 받지 않습니다. ADHOC 용...
        /// </summary>
        /// <param name="cmdText">커맨드 SQL 질의문</param>                
        /// <returns>SQL 질의의 결과 DataTable</returns>
        public static DataTable ExecuteDataTable(string cmdText)
        {
            return ExecuteDataTable(
                    null
                    , cmdText);
        }

        public static cRecordset GetRecordset(string cmdText, IDbDataParameter[] parameters)
        {

            return new cRecordset(ExecuteDataTable(
                null
                , cmdText
                , parameters));
        }

        public static cRecordset GetRecordset(string cmdText)
        {
            return new cRecordset(ExecuteDataTable(
                    null
                    , cmdText));
        }
        #endregion

        #region ExecuteDataSet

        /// <summary>
        /// CommandText와 CommandType 을 모두 받아서 Query 결과를 DataSet로 리턴합니다. 
        /// 이 함수는 Connection 을 외부에서 받아서 사용합니다. Transaction 의 중간에 질의가 필요할 때 사용하세요.
        /// </summary>
        /// <param name="conn">Sql Connection 인스턴스</param>
        /// <param name="tran">conn으로부터 생성한 Transaction 인스턴스</param>
        /// <param name="cmdText">커맨드 SQL 질의문</param>  
        /// <param name="cmdType">커맨드 SQL 질의의 종류.</param>
        /// <param name="parameters">SQL 질의문의 파라메터 목록</param>
        /// <returns>SQL 질의의 결과 DataSet</returns>
        public static DataSet ExecuteDataSet(SqlConnection conn, SqlTransaction tran, string cmdText, CommandType cmdType, IDbDataParameter[] parameters)
        {
            DataTransactionManager dtm = DataTransactionManager.GetCurrent();
            if (dtm == null)
            {
                conn = (conn == null ? _conn : conn); //20200114 정민석 추가 (connect Pool 초과 오류)
                if (conn == null)
                    conn = GetConnection();
            }
            else
            {
                if (dtm.TransactionBegun)
                    conn = DataTransactionManager.GetCurrent().Connection;
                else
                {
                    conn = GetConnection();
                }
            }

            SqlCommand cmd = new SqlCommand();

            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;

            if (tran != null)
                cmd.Transaction = tran;

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet dt = new DataSet();

            try
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                adapter.Fill(dt);
            }
            catch
            {
                throw;
            }
            finally
            {
                cmd.Dispose();
                adapter.Dispose();
            }

            return dt;
        }

        /// <summary>
        /// CommandText와 CommandType 을 모두 받아서 Query 결과를 DataSet로 리턴합니다. 
        /// 이 함수는 Connection 을 외부에서 받아서 사용합니다. TransactionScope 등으로 트랜잭션을 별도 관리시 사용하세요.
        /// </summary>
        /// <param name="conn">Sql Connection 인스턴스</param>        
        /// <param name="cmdText">커맨드 SQL 질의문</param>  
        /// <param name="cmdType">커맨드 SQL 질의의 종류.</param>
        /// <param name="parameters">SQL 질의문의 파라메터 목록</param>
        /// <returns>SQL 질의의 결과 DataSet</returns>
        public static DataSet ExecuteDataSet(SqlConnection conn, string cmdText, CommandType cmdType, IDbDataParameter[] parameters)
        {
            return ExecuteDataSet(
                conn
                , null
                , cmdText
                , cmdType
                , parameters);
        }


        /// <summary>
        /// CommandText와 CommandType 을 모두 받아서 Query 결과를 DataSet로 리턴합니다. 
        /// 이 함수는 Connection 을 직접 생성합니다. 따라서 Transaction 의 중간에서 사용할 수 없습니다. 
        /// </summary>
        /// <param name="cmdText">커맨드 SQL 질의문</param>
        /// <param name="cmdType">SQL 질의의 종류</param>
        /// <param name="parameters">SQL 질의문의 파라메터 목록</param>
        /// <returns>SQL 질의의 결과 DataSet</returns>
        public static DataSet ExecuteDataSet(string cmdText, CommandType cmdType, IDbDataParameter[] parameters)
        {
            return ExecuteDataSet(
                    null
                    , cmdText
                    , cmdType
                    , parameters);
        }

        /// <summary>
        /// CommandText를 입력 받아서 Query 결과를 DataSet로 리턴합니다. 
        /// 이 함수는 Connection, Transaction 을 외부에서 받아서 사용합니다. Transaction 을 직접 관리할 때 사용하세요.
        /// 이 함수는 CommandType 을 무조건 Text 형식으로 설정합니다. Stored Procedure 문에 사용하지 마십시오.
        /// </summary>
        /// <param name="conn">Sql Connection 인스턴스</param>
        /// <param name="tran">conn으로부터 생성한 Transaction 인스턴스</param>
        /// <param name="cmdText">커맨드 SQL 질의문</param>        
        /// <param name="parameters">SQL 질의문의 파라메터 목록</param>
        /// <returns>SQL 질의의 결과 DataSet</returns>
        public static DataSet ExecuteDataSet(SqlConnection conn, SqlTransaction tran, string cmdText, IDbDataParameter[] parameters)
        {
            return ExecuteDataSet(
                conn
                , tran
                , cmdText
                , CommandType.Text
                , parameters);
        }

        /// <summary>
        /// CommandText를 입력 받아서 Query 결과를 DataSet로 리턴합니다. 
        /// 이 함수는 Connection만 외부에서 받아서 사용합니다. TransactionScope 등으로 별도 트랜잭션 관리시 사용하세요.
        /// 이 함수는 CommandType 을 무조건 Text 형식으로 설정합니다. Stored Procedure 문에 사용하지 마십시오.
        /// </summary>
        /// <param name="conn">Sql Connection 인스턴스</param>        
        /// <param name="cmdText">커맨드 SQL 질의문</param>        
        /// <param name="parameters">SQL 질의문의 파라메터 목록</param>
        /// <returns>SQL 질의의 결과 DataSet</returns>
        public static DataSet ExecuteDataSet(SqlConnection conn, string cmdText, IDbDataParameter[] parameters)
        {
            return ExecuteDataSet(
                conn
                , null
                , cmdText
                , CommandType.Text
                , parameters);
        }

        /// <summary>
        /// CommandText를 받아서 Query 결과를 DataSet로 리턴합니다. 
        /// 이 함수는 Connection 을 직접 생성합니다. 따라서 Transaction 의 중간에서 사용할 수 없습니다. 
        /// 이 함수는 CommandType 을 무조건 Text 형식으로 설정합니다. Stored Procedure 문에 사용하지 마십시오.
        /// </summary>
        /// <param name="cmdText">커맨드 SQL 질의문</param>        
        /// <param name="parameters">SQL 질의문의 파라메터 목록</param>
        /// <returns>SQL 질의의 결과 DataSet</returns>
        public static DataSet ExecuteDataSet(string cmdText, IDbDataParameter[] parameters)
        {
            return ExecuteDataSet(
                    null
                    , cmdText
                    , parameters);
        }

        /// <summary>
        /// CommandText를 받아서 Query 결과를 DataSet로 리턴합니다. 
        /// 이 함수는 Connection, Transaction 을 외부에서 받아서 사용합니다. Transaction을 직접 관리시 사용하세요.
        /// 이 함수는 CommandType 을 무조건 Text 형식으로 설정합니다. Stored Procedure 문에 사용하지 마십시오.
        /// 이 함수는 Parameter를 받지 않습니다. ADHOC 용...
        /// </summary>
        /// <param name="conn">Sql Connection 인스턴스</param>
        /// <param name="tran">conn으로부터 생성한 Transaction 인스턴스</param>
        /// <param name="cmdText">커맨드 SQL 질의문</param>                
        /// <returns>SQL 질의의 결과 DataSet</returns>
        public static DataSet ExecuteDataSet(SqlConnection conn, SqlTransaction tran, string cmdText)
        {
            return ExecuteDataSet(
                conn
                , tran
                , cmdText
                , CommandType.Text
                , null);
        }

        /// <summary>
        /// CommandText를 받아서 Query 결과를 DataSet로 리턴합니다. 
        /// 이 함수는 Connection을 외부에서 받아서 사용합니다. TransactionScope 등으로 트랜잭션을 별도 관리시 사용하세요.
        /// 이 함수는 CommandType 을 무조건 Text 형식으로 설정합니다. Stored Procedure 문에 사용하지 마십시오.
        /// 이 함수는 Parameter를 받지 않습니다. ADHOC 용...
        /// </summary>
        /// <param name="conn">Sql Connection 인스턴스</param>        
        /// <param name="cmdText">커맨드 SQL 질의문</param>                
        /// <returns>SQL 질의의 결과 DataSet</returns>
        public static DataSet ExecuteDataSet(SqlConnection conn, string cmdText)
        {
            return ExecuteDataSet(
                conn
                , cmdText
                , null);
        }

        /// <summary>
        /// CommandText를 받아서 Query 결과를 DataSet로 리턴합니다. 
        /// 이 함수는 Connection 을 직접 생성합니다. 따라서 Transaction 의 중간에서 사용할 수 없습니다. 
        /// 이 함수는 CommandType 을 무조건 Text 형식으로 설정합니다. Stored Procedure 문에 사용하지 마십시오.
        /// 이 함수는 Parameter를 받지 않습니다. ADHOC 용...
        /// </summary>
        /// <param name="cmdText">커맨드 SQL 질의문</param>                
        /// <returns>SQL 질의의 결과 DataSet</returns>
        public static DataSet ExecuteDataSet(string cmdText)
        {
            return ExecuteDataSet(
                    null
                    , cmdText);
        }
        #endregion

        #region ExecuteNonQuery

        /// <summary>
        /// CommandText, CommantType, Parameter 를 받아 결과가 없는 SQL 질의를 실행합니다. 
        /// 이 함수는 Connection과 Transaction 객체를 외부로부터 받아와서 실행됩니다. Transaction 을 직접 관리하고자 할 때 사용하세요.
        /// </summary>
        /// <param name="conn">DB 연결 객체</param>
        /// <param name="tran">DB 연결시 트랜잭션을 관리할 SqlTransaction 개체</param>
        /// <param name="cmdText">SQL 질의 문자열</param>
        /// <param name="cmdType">SQL 질의 종류</param>
        /// <param name="parameters">SQL 질의의 파라메터</param>
        /// <returns>SQL 질의의 영향을 받은 행의 갯수</returns>
        public static int ExecuteNonQuery(SqlConnection conn, SqlTransaction tran, string cmdText, CommandType cmdType, IDbDataParameter[] parameters)
        {
            DataTransactionManager dtm = DataTransactionManager.GetCurrent();
            if (dtm == null)
            {
                conn = (conn == null ? _conn : conn); //20200114 정민석 추가 (connect Pool 초과 오류)
                if (conn == null)
                    conn = GetConnection();
            }
            else
            {
                if (dtm.TransactionBegun)
                    conn = DataTransactionManager.GetCurrent().Connection;
                else
                {
                    conn = GetConnection();
                }
            }

            if (parameters != null)
            {
                //cmdText = cmdText.Replace(Environment.NewLine, " ").Replace("\r", " ").Replace("\t", " "); //주석에 오류 남.
                cmdText = cmdText.Replace("\t ", " ");
                cmdText = cmdText.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
                cmdText = cmdText.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
                cmdText = cmdText.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
                cmdText = cmdText.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
                cmdText = cmdText.Replace("  ", " ").Replace("  ", " ").Replace("  ", " ");
            }


            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;
            cmd.Connection = conn;

            if (tran != null)
            {
                cmd.Transaction = tran;
            }

            if (parameters != null)
            {
                //중복파라메타 제거
                Dictionary<string, bool> dicDup = new Dictionary<string, bool>();
                IDbDataParameter[] paraAdjust = new IDbDataParameter[0];
                for (int ii = 0; ii < parameters.Length; ii++)
                {
                    if (dicDup.ContainsKey(parameters[ii].ParameterName) == false)
                    {
                        Array.Resize(ref paraAdjust, paraAdjust.Length + 1);
                        paraAdjust[paraAdjust.Length - 1] = parameters[ii];
                        dicDup.Add(parameters[ii].ParameterName, true);
                    }
                }
                parameters = paraAdjust;
                cmd.Parameters.AddRange(parameters);
            }


            int result = 0;

            try
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                result = cmd.ExecuteNonQuery();
            }
            catch (SqlException ex)
            {
                string sSQL = GetSQL(cmdText, parameters);
                throw ex;
            }
            catch
            {
                string sSQL = GetSQL(cmdText, parameters);
                throw;
            }
            finally
            {
                cmd.Parameters.Clear(); //sqlparameter이(가) 이미 다른 sqlparametercollection에 들어 있습니다
                cmd.Dispose();
            }

            return result;
        }

        /// <summary>
        /// CommandText, CommantType, Parameter 를 받아 결과가 없는 SQL 질의를 실행합니다. 
        /// 이 함수는 Connection 객체를 외부로부터 받아와 실행됩니다. TransactionScope 등을 통하여 Transaction 관리시 사용하세요.
        /// </summary>
        /// <param name="conn">DB 연결 객체</param>        
        /// <param name="cmdText">SQL 질의 문자열</param>
        /// <param name="cmdType">SQL 질의 종류</param>
        /// <param name="parameters">SQL 질의의 파라메터</param>
        /// <returns>SQL 질의의 영향을 받은 행의 갯수</returns>
        public static int ExecuteNonQuery(SqlConnection conn, string cmdText, CommandType cmdType, IDbDataParameter[] parameters)
        {
            return ExecuteNonQuery(
                conn
                , null
                , cmdText
                , cmdType
                , parameters);
        }

        /// <summary>
        /// CommandText, CommantType, Parameter 를 받아 결과가 없는 SQL 질의를 실행합니다. 
        /// 이 함수는 Connection 을 직접 생성하여 실행됩니다. 따라서 Transaction 처리에 사용할 수 없습니다. 
        /// </summary>
        /// <param name="conn">DB 연결 객체</param>
        /// <param name="cmdText">SQL 질의 문자열</param>
        /// <param name="cmdType">SQL 질의 종류</param>
        /// <param name="parameters">SQL 질의의 파라메터</param>
        /// <returns>SQL 질의의 영향을 받은 행의 갯수</returns>
        public static int ExecuteNonQuery(string cmdText, CommandType cmdType, IDbDataParameter[] parameters)
        {
            return ExecuteNonQuery(
                    null
                    , cmdText
                    , cmdType
                    , parameters);
        }

        /// <summary>
        /// CommandText, Parameter 를 받아 결과가 없는 SQL 질의를 실행합니다. 
        /// 이 함수는 Connection과 Transaction 객체를 외부로부터 받아와서 실행됩니다. Transaction 을 직접 관리하고자 할 때 사용하세요.
        /// 이 함수는 무조건 Command Type 이 Text이므로 Stored Procedure 에는 사용하지 마세요.
        /// </summary>
        /// <param name="conn">DB 연결 객체</param>
        /// <param name="tran">DB 연결시 트랜잭션을 관리할 SqlTransaction 개체</param>
        /// <param name="cmdText">SQL 질의 문자열</param>        
        /// <param name="parameters">SQL 질의의 파라메터</param>
        /// <returns>SQL 질의의 영향을 받은 행의 갯수</returns>
        public static int ExecuteNonQuery(SqlConnection conn, SqlTransaction tran, string cmdText, IDbDataParameter[] parameters)
        {
            return ExecuteNonQuery(
                conn
                , tran
                , cmdText
                , CommandType.Text
                , parameters);
        }

        /// <summary>
        /// CommandText, Parameter 를 받아 결과가 없는 SQL 질의를 실행합니다. 
        /// 이 함수는 Connection과 Transaction 객체를 외부로부터 받아와서 실행됩니다. Transaction 을 직접 관리하고자 할 때 사용하세요.
        /// 이 함수는 무조건 Command Type 이 Text이므로 Stored Procedure 에는 사용하지 마세요.
        /// </summary>
        /// <param name="conn">DB 연결 객체</param>
        /// <param name="tran">DB 연결시 트랜잭션을 관리할 SqlTransaction 개체</param>
        /// <param name="cmdText">SQL 질의 문자열</param>        
        /// <param name="parameters">SQL 질의의 파라메터</param>
        /// <returns>SQL 질의의 영향을 받은 행의 갯수</returns>
        public static int ExecuteNonQuery(SqlConnection conn, string cmdText, IDbDataParameter[] parameters)
        {
            return ExecuteNonQuery(
                conn
                , null
                , cmdText
                , CommandType.Text
                , parameters);
        }

        /// <summary>
        /// CommandText, Parameter 를 받아 결과가 없는 SQL 질의를 실행합니다. 
        /// 이 함수는 Connection을 직접 생성하여 실행됩니다. Transaction 의 내부에서 사용할 수 없습니다. 
        /// 이 함수는 무조건 Command Type 이 Text이므로 Stored Procedure 에는 사용하지 마세요.
        /// </summary>        
        /// <param name="cmdText">SQL 질의 문자열</param>        
        /// <param name="parameters">SQL 질의의 파라메터</param>
        /// <returns>SQL 질의의 영향을 받은 행의 갯수</returns>
        public static int ExecuteNonQuery(string cmdText, IDbDataParameter[] parameters)
        {
            return ExecuteNonQuery(
                null
                , null
                , cmdText
                , CommandType.Text
                , parameters);
        }

        /// <summary>
        /// CommandText 를 받아 결과가 없는 SQL 질의를 실행합니다. 
        /// 이 함수는 Connection과 Transaction 객체를 외부로부터 받아와서 실행됩니다. Transaction 을 직접 관리하고자 할 때 사용하세요.
        /// 이 함수는 무조건 Command Type 이 Text이므로 Stored Procedure 에는 사용하지 마세요.
        /// 이 함수는 Parameter 를 받지 않습니다. ADHOC 용..
        /// </summary>
        /// <param name="conn">DB 연결 객체</param>
        /// <param name="tran">DB 연결시 트랜잭션을 관리할 SqlTransaction 개체</param>
        /// <param name="cmdText">SQL 질의 문자열</param>                
        /// <returns>SQL 질의의 영향을 받은 행의 갯수</returns>
        public static int ExecuteNonQuery(SqlConnection conn, SqlTransaction tran, string cmdText)
        {
            return ExecuteNonQuery(
                conn
                , tran
                , cmdText
                , CommandType.Text
                , null);
        }

        /// <summary>
        /// CommandText 를 받아 결과가 없는 SQL 질의를 실행합니다. 
        /// 이 함수는 Connection객체를 외부로부터 받아와서 실행됩니다. TransactionScope 등을 통하여 Transaction 관리시 사용하세요.
        /// 이 함수는 무조건 Command Type 이 Text이므로 Stored Procedure 에는 사용하지 마세요.
        /// 이 함수는 Parameter 를 받지 않습니다. ADHOC 용..
        /// </summary>
        /// <param name="conn">DB 연결 객체</param>        
        /// <param name="cmdText">SQL 질의 문자열</param>                
        /// <returns>SQL 질의의 영향을 받은 행의 갯수</returns>
        public static int ExecuteNonQuery(SqlConnection conn, string cmdText)
        {
            return ExecuteNonQuery(
                conn
                , null
                , cmdText
                , CommandType.Text
                , null);
        }

        /// <summary>
        /// CommandText 를 받아 결과가 없는 SQL 질의를 실행합니다. 
        /// 이 함수는 Connectio 직접 생성하여 실행됩니다. Transaction 의 내부에서 사용할 수 없습니다.  
        /// 이 함수는 무조건 Command Type 이 Text이므로 Stored Procedure 에는 사용하지 마세요.
        /// 이 함수는 Parameter 를 받지 않습니다. ADHOC 용..
        /// </summary>        
        /// <param name="cmdText">SQL 질의 문자열</param>                
        /// <returns>SQL 질의의 영향을 받은 행의 갯수</returns>
        public static int ExecuteNonQuery(string cmdText)
        {
            return ExecuteNonQuery(
                null
                , null
                , cmdText
                , CommandType.Text
                , null);
        }
        #endregion

        #region ExecuteScalar
        public static object ExecuteScalar(SqlConnection conn, SqlTransaction tran, string cmdText, CommandType cmdType, IDbDataParameter[] parameters)
        {
            object result = null;

            DataTransactionManager dtm = DataTransactionManager.GetCurrent();
            if (dtm == null)
            {
                conn = (conn == null ? _conn : conn); //20200114 정민석 추가 (connect Pool 초과 오류)
                if (conn == null)
                    conn = GetConnection();
            }
            else
            {
                if (dtm.TransactionBegun)
                    conn = DataTransactionManager.GetCurrent().Connection;
                else
                {
                    conn = GetConnection();
                }
            }

            SqlCommand cmd = new SqlCommand();
            cmd.CommandText = cmdText;
            cmd.CommandType = cmdType;
            cmd.Connection = conn;

            if (tran != null)
                cmd.Transaction = tran;

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            try
            {
                if (conn.State != ConnectionState.Open)
                    conn.Open();

                result = cmd.ExecuteScalar();
            }
            catch
            {
                throw;
            }
            finally
            {
                cmd.Dispose();
            }

            return result;
        }

        public static object ExecuteScalar(SqlConnection conn, string cmdText, CommandType cmdType, IDbDataParameter[] parameters)
        {
            return ExecuteScalar(
                conn
                , null
                , cmdText
                , cmdType
                , parameters);
        }

        public static object ExecuteScalar(string cmdText, CommandType cmdType, IDbDataParameter[] parameters)
        {
            return ExecuteScalar(
                    null
                    , null
                    , cmdText
                    , cmdType
                    , parameters);
        }

        public static object ExecuteScalar(SqlConnection conn, SqlTransaction tran, string cmdText, IDbDataParameter[] parameters)
        {
            return ExecuteScalar(
                conn
                , tran
                , cmdText
                , CommandType.Text
                , parameters);
        }

        public static object ExecuteScalar(SqlConnection conn, string cmdText, IDbDataParameter[] parameters)
        {
            return ExecuteScalar(
                conn
                , null
                , cmdText
                , parameters);
        }

        public static object ExecuteScalar(string cmdText, IDbDataParameter[] parameters)
        {
            return ExecuteScalar(
                    null
                    , null
                    , cmdText
                    , parameters);
        }

        public static object ExecuteScalar(SqlConnection conn, SqlTransaction tran, string cmdText)
        {
            return ExecuteScalar(
                conn
                , tran
                , cmdText
                , null);
        }

        public static object ExecuteScalar(SqlConnection conn, string cmdText)
        {
            return ExecuteScalar(
                conn
                , cmdText
                , null);
        }

        public static object ExecuteScalar(string cmdText)
        {
            return ExecuteScalar(
                    null
                    , cmdText);
        }

        #endregion

        #region ExecuteUpdate
        public static int ExecuteUpdate(SqlConnection conn, object data,
            string cmdTextInsert, string cmdTextUpdate, string cmdTextDelete,
            CommandType cmdTypeInsert, CommandType cmdTypeUpdate, CommandType cmdTypeDelete,
            IDbDataParameter[] paramInsert, IDbDataParameter[] paramUpdate, IDbDataParameter[] paramDelete)
        {
            DataTransactionManager dtm = DataTransactionManager.GetCurrent();
            if (dtm == null)
            {
                conn = (conn == null ? _conn : conn); //20200114 정민석 추가 (connect Pool 초과 오류)
                if (conn == null)
                    conn = GetConnection();
            }
            else
            {
                if (dtm.TransactionBegun)
                    conn = DataTransactionManager.GetCurrent().Connection;
                else
                {
                    conn = GetConnection();
                }
            }

            SqlCommand cmdInsert = new SqlCommand();
            SqlCommand cmdUpdate = new SqlCommand();
            SqlCommand cmdDelete = new SqlCommand();

            cmdInsert.CommandText = cmdTextInsert;
            cmdUpdate.CommandText = cmdTextUpdate;
            cmdDelete.CommandText = cmdTextDelete;

            cmdInsert.CommandType = cmdTypeInsert;
            cmdUpdate.CommandType = cmdTypeUpdate;
            cmdDelete.CommandType = cmdTypeDelete;

            if (paramInsert != null)
                cmdInsert.Parameters.AddRange(paramInsert);
            if (paramUpdate != null)
                cmdUpdate.Parameters.AddRange(paramUpdate);
            if (paramDelete != null)
                cmdDelete.Parameters.AddRange(paramDelete);

            try
            {
                return ExecuteUpdate(conn, data, cmdInsert, cmdUpdate, cmdDelete);
            }
            finally
            {
                cmdInsert.Dispose();
                cmdUpdate.Dispose();
                cmdDelete.Dispose();
            }
        }

        public static int ExecuteUpdate(object data, SqlCommand cmdInsert, SqlCommand cmdUpdate, SqlCommand cmdDelete)
        {
            return ExecuteUpdate(null, data, cmdInsert, cmdUpdate, cmdDelete);
        }

        public static int ExecuteUpdate(SqlConnection conn, object data, SqlCommand cmdInsert, SqlCommand cmdUpdate, SqlCommand cmdDelete)
        {
            if (conn == null)
                conn = GetConnection();

            if (cmdInsert != null)
                cmdInsert.Connection = conn;
            if (cmdUpdate != null)
                cmdUpdate.Connection = conn;
            if (cmdDelete != null)
                cmdDelete.Connection = conn;

            SqlDataAdapter adapter = GetDataAdapter(cmdInsert, cmdUpdate, cmdDelete);

            try
            {
                if (data is DataRow[])
                    return adapter.Update(data as DataRow[]);
                else if (data is DataTable)
                    return adapter.Update(data as DataTable);
                else
                    throw new ArgumentException("Type of data must be DataRow[] or DataTable.");
            }
            finally
            {
                adapter.Dispose();
            }
        }

        private static SqlDataAdapter GetDataAdapter(SqlCommand cmdInsert, SqlCommand cmdUpdate, SqlCommand cmdDelete)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();

            adapter.InsertCommand = cmdInsert;
            adapter.UpdateCommand = cmdUpdate;
            adapter.DeleteCommand = cmdDelete;

            return adapter;
        }
        #endregion



        /// <summary>
        /// Insert시 Array처리 [일정한 조건이 있습니다.조건에 맞게 호출해야 합니다.]
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="dicList"></param>
        /// <returns></returns>
        public static int ExecuteInsArray(string cmdText, Dictionary<int, List<IDbDataParameter>> pdicPara)
        {
            string[] arSQL = cmdText.ToUpper().Split(new string[] { "VALUES" }, StringSplitOptions.None);
            string[] arrSelect = arSQL[0].ToUpper().Split(new string[] { ")" }, StringSplitOptions.None);
            arrSelect = arrSelect[0].ToUpper().Split(new string[] { "(" }, StringSplitOptions.None)[1].Split(',');

            string sValueS = "";
            string sParaV = "";
            foreach (KeyValuePair<int, List<IDbDataParameter>> kv in pdicPara)
            {
                string sValue = "";
                string sParaName = "";
                List<IDbDataParameter> psPara = kv.Value;
                for (int jj = 0; jj < arrSelect.Length; jj++)
                {
                    sParaName = arrSelect[jj].ToUpper().Replace(Environment.NewLine, "").Trim().Replace(" ", "").Replace("\t", "").Replace("\r", "");

                    if (sValue != "")
                    {
                        sValue += ",";
                    }
                    sParaV = GetStrParamValue(psPara, sParaName);
                    if (sParaV == "")
                    {
                        sValue += " NULL  ";
                    }
                    else
                    {
                        sValue += " '" + sParaV + "' ";
                    }
                }
                if (sValueS != "")
                {
                    sValueS += Environment.NewLine;
                    sValueS += " , ";
                }
                sValueS += " (" + sValue + ") ";
            }


            string sSQL = arSQL[0] + " VALUES " + sValueS;
            return DataAccessObject.ExecuteNonQuery(sSQL);
        }

        public static int ExecuteInsArray(string cmdText, List<List<IDbDataParameter>> pListPara)
        {
            string[] arSQL = cmdText.ToUpper().Split(new string[] { "VALUES" }, StringSplitOptions.None);
            string[] arrSelect = arSQL[0].ToUpper().Split(new string[] { ")" }, StringSplitOptions.None);
            arrSelect = arrSelect[0].ToUpper().Split(new string[] { "(" }, StringSplitOptions.None)[1].Split(',');

            string sValueS = "";
            string sParaV = "";

            for (int ii = 0; ii < pListPara.Count; ii++) //lstArrPOrd  전체 처방 목록 
            {
                string sValue = "";
                string sParaName = "";
                List<IDbDataParameter> psPara = pListPara[ii];
                for (int jj = 0; jj < arrSelect.Length; jj++)
                {
                    sParaName = arrSelect[jj].ToUpper().Replace(Environment.NewLine, "").Trim().Replace(" ", "").Replace("\t", "").Replace("\r", "");

                    if (sValue != "")
                    {
                        sValue += ",";
                    }
                    sParaV = GetStrParamValue(psPara, sParaName);
                    if (sParaV == "")
                    {
                        sValue += " NULL  ";
                    }
                    else
                    {
                        sValue += " '" + sParaV + "' ";
                    }
                }
                if (sValueS != "")
                {
                    sValueS += Environment.NewLine;
                    sValueS += " , ";
                }
                sValueS += " (" + sValue + ") ";
            }


            string sSQL = arSQL[0] + " VALUES " + sValueS;
            return DataAccessObject.ExecuteNonQuery(sSQL);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region 바인딩 값 체크
        /// <summary>
        /// 쿼리문 + 바인딩된 변수를 통해 쿼리 문장을 생성 리턴 한다.
        /// </summary>
        /// <param name="cmdText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        //public static string GetSQL(string cmdText, IDbDataParameter[] parameters)
        //{
        //    string sSQL = cmdText;

        //    if (parameters != null)
        //    {
        //        var newList = parameters.OrderBy(x => x.ParameterName).ToList();

        //        for (int ii = 0; ii < newList.Count; ii++)
        //        {
        //            sSQL = sSQL.Replace(newList[ii].ParameterName, "'" + newList[ii].Value + "'");
        //        }
        //    }

        //    return sSQL;
        //}
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

        /// <summary>
        /// 바인딩 변수에 값을 리턴한다. Object 타입
        /// </summary>
        /// <param name="psPara"></param>
        /// <param name="psParaName"></param>
        /// <returns></returns>
        public static object GetObjParamValue(List<IDbDataParameter> psPara, string psParaName)
        {
            if (psParaName.StartsWith("@") == false)
            {
                psParaName = "@" + psParaName;
            }
            object oRtn = "";
            foreach (SqlParameter para in psPara)
            {
                if (para.ParameterName == psParaName)
                {
                    oRtn = para.Value;
                    break;
                }
            }
            return oRtn;
        }

        /// <summary>
        /// 바인딩 변수에 값을 리턴한다. String 타입
        /// </summary>
        /// <param name="psPara"></param>
        /// <param name="psParaName"></param>
        /// <returns></returns>
        public static string GetStrParamValue(List<IDbDataParameter> psPara, string psParaName)
        {
            psParaName = psParaName.Trim().ToUpper();
            psParaName = psParaName.Replace("@", "");
            //if (psParaName.StartsWith("@") == false)
            //{
            psParaName = "@" + psParaName;
            //}
            string sRtn = "";
            foreach (SqlParameter para in psPara)
            {
                if (para.ParameterName == psParaName)
                {
                    sRtn = para.Value + "";
                    break;
                }
            }
            return sRtn;
        }

        /// <summary>
        /// 중복되는 파라메타를 Distinct 한다. (먼저나오는 파라메타 살리고 후에나오는 파라메타 없앤다.)
        /// </summary>
        /// <param name="plPara"></param>
        private static void SetParamDistinct(List<IDbDataParameter> plPara)
        {
            //중복파라메타 제거
            Dictionary<string, bool> dicDup = new Dictionary<string, bool>();
            List<IDbDataParameter> lstPara = new List<IDbDataParameter>();
            lstPara.Clear();

            for (int ii = 0; ii < plPara.Count; ii++)
            {
                if (dicDup.ContainsKey((plPara[ii]).ParameterName) == false)
                {
                    lstPara.Add(plPara[ii]);
                    dicDup.Add((plPara[ii]).ParameterName, true);
                }
            }

            plPara = lstPara;
        }

        #endregion


        /// <summary>
        /// 파라메타에 값넣기.(파라메타 A라는 곳에 A라는 값을 셋팅할 경우 기존 파라메타가 후의 값으로 변경됨)
        /// </summary>
        /// <param name="psPara"></param>
        /// <param name="psParaName"></param>
        /// <param name="psValue"></param>
        public static void SetParam(List<IDbDataParameter> psPara, string psParaName, object psValue)
        {
            if (psParaName.StartsWith("@") == false)
            {
                psParaName = "@" + psParaName;
            }
            foreach (SqlParameter para in psPara)
            {
                if (para.ParameterName == psParaName)
                {
                    para.Value = psValue;
                    return;
                }
            }

            psPara.Add(new SqlParameter(psParaName, psValue));

        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region 공통 옵션 테이블

        public static string OptTa88(string psMst1CD, string psMst2CD, string psMst3CD, string psField, string psDefault)
        {

            string sSQL = "";
            sSQL += @"   SELECT " + psField + @" 
                          FROM TA88 A88  (nolock) 
                         WHERE 1 = 1   
                           AND A88.MST1CD = '" + psMst1CD + @"' 
                           AND A88.MST2CD = '" + psMst2CD + @"' 
                           AND A88.MST3CD = '" + psMst3CD + "' ";

            DataTable dtRtn = ExecuteDataTable(sSQL);
            if (dtRtn.Rows.Count > 0)
            {
                psDefault = dtRtn.Rows[0][psField] + "";
            }

            return psDefault;
        }


        /// <summary>
        /// TA88데이타를 가져온다
        /// </summary>
        /// <param name="psMst1CD"></param>
        /// <param name="psMst2CD"></param>
        /// <param name="psMst3CD"></param>
        /// <param name="psSelect"></param>
        /// <param name="psFilter"></param>
        /// <returns></returns>
        public static DataTable GetTA88yKey(string psMst1CD, string psMst2CD, string psMst3CD, string psSelect, string psFilter)
        {
            psSelect = psSelect.Replace(" ", "").ToUpper();
            if (psSelect == "")
            {
                psSelect = " *";
            }
            psSelect = psSelect.Replace(";", ",");

            string sSQL = "";
            sSQL += @"   SELECT " + psSelect + @" 
                          FROM TA88 A88  (nolock) 
                         WHERE 1 = 1 ";
            if (psMst1CD != "")
            {
                sSQL += @"   AND A88.MST1CD = '" + psMst1CD + "' ";
            }
            if (psMst2CD != "")
            {
                sSQL += @"   AND A88.MST2CD = '" + psMst2CD + "' ";
            }
            if (psMst3CD != "")
            {
                sSQL += @"   AND A88.MST3CD = '" + psMst3CD + "' ";
            }
            sSQL += @"   ORDER BY A88.MST1CD
                                , A88.MST2CD
                                , A88.MST3CD";


            DataTable dtRtn = ExecuteDataTable(sSQL);

            if (psFilter != "")
            {
                DataRow[] arr = dtRtn.Select(psFilter);

                if (arr.Length > 0)
                {
                    dtRtn = dtRtn.Clone();
                    for (int ii = 0; ii < arr.Length; ii++)
                    {
                        dtRtn.Rows.Add(arr[ii].ItemArray);
                    }
                    //dtRtn = arr.CopyToDataTable();  //이게 안보임
                }
                else
                {
                    dtRtn = dtRtn.Clone();
                }
            }

            return dtRtn;
        }

        /// <summary>
        /// TA88데이타를 UNION으로 가져온다
        /// </summary>
        /// <param name="psWhere"></param>
        /// <param name="psSelect"></param>
        /// <param name="psFilter"></param>
        /// <returns></returns>
        public static DataTable GetTA88UByKey(string psWhere, string psSelect, string psFilter)
        {
            string sSQL = "";

            psWhere = psWhere.Replace(" ", "").ToUpper();  //"1,2/3,2/";
            psSelect = psSelect.Replace(" ", "").ToUpper();
            if (psSelect == "")
            {
                psSelect = " *";
            }
            psSelect = psSelect.Replace(";", ",");

            string[] arrsWhere = psWhere.Split(';'); //쿼리갯수 
            if (arrsWhere.Length < 1)
            {
                return null;
            }

            for (int ii = 0; ii < arrsWhere.Length; ii++) //쿼리갯수까지
            {
                if (arrsWhere[ii] != "") //마지막에 문자열 처리 : 추가했다면 빠짐
                {
                    string[] arrDetail = arrsWhere[ii].Trim().Split(','); //쿼리할 필드갯수

                    bool blChk = false;
                    for (int jj = 0; jj < arrDetail.Length; jj++)
                    {
                        if (arrDetail[jj] != "")
                        {
                            blChk = true;
                            break;
                        }
                    }

                    if (blChk == true) //필드값 있다면 실제쿼리함.
                    {
                        if (sSQL != "")
                        {
                            sSQL += @"  UNION ALL";
                        }
                        sSQL += @"   SELECT " + psSelect + @" 
                                       FROM TA88 A88 (nolock) 
                                      WHERE 1 = 1 ";
                        if (arrDetail[0] != "")
                        {
                            sSQL += @"   AND A88.MST1CD = '" + arrDetail[0] + "' ";
                        }
                        if (arrDetail[1] != "")
                        {
                            sSQL += @"   AND A88.MST2CD = '" + arrDetail[1] + "' ";
                        }
                        if (arrDetail[2] != "")
                        {
                            sSQL += @"   AND A88.MST3CD = '" + arrDetail[2] + "' ";
                        }
                    }
                }
            }
            sSQL += @"   ORDER BY A88.MST1CD
                                , A88.MST2CD
                                , A88.MST3CD";


            DataTable dtRtn = ExecuteDataTable(sSQL);

            if (psFilter != "")
            {
                DataRow[] arr = dtRtn.Select(psFilter);

                if (arr.Length > 0)
                {
                    dtRtn = dtRtn.Clone();
                    for (int ii = 0; ii < arr.Length; ii++)
                    {
                        dtRtn.Rows.Add(arr[ii].ItemArray);
                    }
                    //dtRtn = arr.CopyToDataTable();  //이게 안보임
                }
                else
                {
                    dtRtn = dtRtn.Clone();
                }
            }

            return dtRtn;
        }

        public static string OptTa972(string psPrjCD, string psFrmnm, string psSeq, string psField, string psDefault)
        {

            string sSQL = "";
            sSQL += @"   SELECT " + psField + @" 
                          FROM TA972 A972  (nolock) 
                         WHERE 1 = 1   
                           AND A972.PRJCD = '" + psPrjCD + @"' 
                           AND A972.FRMNM = '" + psFrmnm + @"' 
                           AND A972.SEQ = '" + psSeq + "' ";

            DataTable dtRtn = ExecuteDataTable(sSQL);
            if (dtRtn.Rows.Count > 0)
            {
                psDefault = dtRtn.Rows[0][psField] + "";
            }

            return psDefault;
        }
 

        /// <summary>
        /// TA972데이타를 가져온다.  
        /// </summary>
        /// <param name="psPrjCD"></param>
        /// <param name="psFrmnm"></param>
        /// <param name="psSeq"></param>
        /// <param name="psSelect"></param>
        /// <param name="psFilter"></param>
        /// <returns></returns>
        public static DataTable GetTA972ByKey(string psPrjCD, string psFrmnm, string psSeq, string psSelect, string psFilter)
        {
            psSelect = psSelect.Replace(" ", "").ToUpper();
            if (psSelect == "")
            {
                psSelect = " *";
            }
            psSelect = psSelect.Replace(";", ",");

            string sSQL = "";
            sSQL += @"   SELECT " + psSelect + @" 
                          FROM TA972 A972  (nolock) 
                         WHERE 1 = 1 ";
            if (psPrjCD != "")
            {
                sSQL += @"   AND A972.PRJCD = '" + psPrjCD + "' ";
            }
            if (psFrmnm != "")
            {
                sSQL += @"   AND A972.FRMNM = '" + psFrmnm + "' ";
            }
            if (psSeq != "")
            {
                sSQL += @"   AND A972.SEQ = '" + psSeq + "' ";
            }
            sSQL += @"   ORDER BY A972.PRJCD
                                , A972.FRMNM
                                , A972.SEQ";


            DataTable dtRtn = ExecuteDataTable(sSQL);

            if (psFilter != "")
            {
                DataRow[] arr = dtRtn.Select(psFilter);

                if (arr.Length > 0)
                {
                    dtRtn = dtRtn.Clone();
                    for (int ii = 0; ii < arr.Length; ii++)
                    {
                        dtRtn.Rows.Add(arr[ii].ItemArray);
                    }
                    //dtRtn = arr.CopyToDataTable();  //이게 안보임
                }
                else
                {
                    dtRtn = dtRtn.Clone();
                }
            }

            return dtRtn;
        }

        /// <summary>
        /// TA972데이타를 UNION기준 가져온다.   Where => ex)"1,2;3,2;";
        /// </summary>
        /// <param name="psWhere"></param>
        /// <param name="psSelect"></param>
        /// <param name="psFilter"></param>
        /// <returns></returns>
        public static DataTable GetTA972UByKey(string psWhere, string psSelect, string psFilter)
        {
            string sSQL = "";

            psWhere = psWhere.Replace(" ", "").ToUpper();  //"1,2/3,2/";
            psSelect = psSelect.Replace(" ", "").ToUpper();
            if (psSelect == "")
            {
                psSelect = " *";
            }
            psSelect = psSelect.Replace(";", ",");

            string[] arrsWhere = psWhere.Split(';'); //쿼리갯수 
            if (arrsWhere.Length < 1)
            {
                return null;
            }

            for (int ii = 0; ii < arrsWhere.Length; ii++) //쿼리갯수까지
            {
                if (arrsWhere[ii] != "") //마지막에 문자열 처리 : 추가했다면 빠짐
                {
                    string[] arrDetail = arrsWhere[ii].Trim().Split(','); //쿼리할 필드갯수

                    bool blChk = false;
                    for (int jj = 0; jj < arrDetail.Length; jj++)
                    {
                        if (arrDetail[jj] != "")
                        {
                            blChk = true;
                            break;
                        }
                    }

                    if (blChk == true) //필드값 있다면 실제쿼리함.
                    {
                        if (sSQL != "")
                        {
                            sSQL += @"  UNION ALL";
                        }
                        sSQL += @"   SELECT " + psSelect + @" 
                                       FROM TA972 A972  (nolock) 
                                      WHERE 1 = 1 ";
                        if (arrDetail[0] != "")
                        {
                            sSQL += @"   AND A972.PRJCD = '" + arrDetail[0] + "' ";
                        }
                        if (arrDetail[1] != "")
                        {
                            sSQL += @"   AND A972.FRMNM = '" + arrDetail[1] + "' ";
                        }
                        if (arrDetail[2] != "")
                        {
                            sSQL += @"   AND A972.SEQ = '" + arrDetail[2] + "' ";
                        }
                    }
                }
            }
            sSQL += @"   ORDER BY A972.PRJCD
                                , A972.FRMNM
                                , A972.SEQ";


            DataTable dtRtn = ExecuteDataTable(sSQL);

            if (psFilter != "")
            {
                DataRow[] arr = dtRtn.Select(psFilter);

                if (arr.Length > 0)
                {
                    dtRtn = dtRtn.Clone();
                    for (int ii = 0; ii < arr.Length; ii++)
                    {
                        dtRtn.Rows.Add(arr[ii].ItemArray);
                    }
                    //dtRtn = arr.CopyToDataTable();  //이게 안보임
                }
                else
                {
                    dtRtn = dtRtn.Clone();
                }
            }

            return dtRtn;
        }

        /// <summary>
        /// TA31데이타를 가져온다.
        /// </summary>
        /// <param name="psMst1CD"></param>
        /// <param name="psMst2Cd"></param>
        /// <param name="psSelect"></param>
        /// <param name="psFilter"></param>
        /// <returns></returns>
        public static DataTable GetTA31ByKey(string psMst1CD, string psMst2Cd, string psSelect, string psFilter)
        {
            psSelect = psSelect.Replace(" ", "").ToUpper();
            if (psSelect == "")
            {
                psSelect = " *";
            }
            psSelect = psSelect.Replace(";", ",");

            string sSQL = "";
            sSQL += @"   SELECT " + psSelect + @" 
                              , MST2CD + '-'  + CDNM V_DATA
                           FROM TA31 A31  (nolock) 
                          WHERE 1 = 1 ";
            if (psMst1CD != "")
            {
                sSQL += @"   AND A31.MST1CD = '" + psMst1CD + "' ";
            }
            if (psMst2Cd != "")
            {
                sSQL += @"   AND A31.MST2CD = '" + psMst2Cd + "' ";
            }
            sSQL += @"   ORDER BY A31.MST1CD
                                , A31.MST2CD ";


            DataTable dtRtn = ExecuteDataTable(sSQL);

            if (psFilter != "")
            {
                DataRow[] arr = dtRtn.Select(psFilter);

                if (arr.Length > 0)
                {
                    dtRtn = dtRtn.Clone();
                    for (int ii = 0; ii < arr.Length; ii++)
                    {
                        dtRtn.Rows.Add(arr[ii].ItemArray);
                    }
                    //dtRtn = arr.CopyToDataTable();  //이게 안보임
                }
                else
                {
                    dtRtn = dtRtn.Clone();
                }
            }

            return dtRtn;
        }

        /// <summary>
        /// TA31데이타를 union으로 가져온다.  Where => ex)"1,2;3,2;";
        /// </summary>
        /// <param name="psWhere"></param>
        /// <param name="psSelect"></param>
        /// <param name="psFilter"></param>
        /// <returns></returns>
        public static DataTable GetTA31UByKey(string psWhere, string psSelect, string psFilter)
        {
            string sSQL = "";

            psWhere = psWhere.Replace(" ", "").ToUpper();  //"1,2;3,2;";
            psSelect = psSelect.Replace(" ", "").ToUpper();
            if (psSelect == "")
            {
                psSelect = " *";
            }
            psSelect = psSelect.Replace(";", ",");

            string[] arrsWhere = psWhere.Split(';'); //쿼리갯수 
            if (arrsWhere.Length < 1)
            {
                return null;
            }

            for (int ii = 0; ii < arrsWhere.Length; ii++) //쿼리갯수까지
            {
                if (arrsWhere[ii] != "") //마지막에 문자열 처리 : 추가했다면 빠짐
                {
                    string[] arrDetail = arrsWhere[ii].Trim().Split(','); //쿼리할 필드갯수

                    bool blChk = false;
                    for (int jj = 0; jj < arrDetail.Length; jj++)
                    {
                        if (arrDetail[jj] != "")
                        {
                            blChk = true;
                            break;
                        }
                    }

                    if (blChk == true) //필드값 있다면 실제쿼리함.
                    {
                        if (sSQL != "")
                        {
                            sSQL += @"  UNION ALL";
                        }
                        sSQL += @"   SELECT " + psSelect + @" 
                                       FROM TA31 A31  (nolock) 
                                      WHERE 1 = 1 ";
                        if (arrDetail[0] != "")
                        {
                            sSQL += @"   AND A31.MST1CD = '" + arrDetail[0] + "' ";
                        }
                        if (arrDetail[1] != "")
                        {
                            sSQL += @"   AND A31.MST2CD = '" + arrDetail[1] + "' ";
                        }
                    }
                }
            }
            sSQL += @"   ORDER BY MST1CD
                                , MST2CD ";


            DataTable dtRtn = ExecuteDataTable(sSQL);

            if (psFilter != "")
            {
                DataRow[] arr = dtRtn.Select(psFilter);

                if (arr.Length > 0)
                {
                    dtRtn = dtRtn.Clone();
                    for (int ii = 0; ii < arr.Length; ii++)
                    {
                        dtRtn.Rows.Add(arr[ii].ItemArray);
                    }
                    //dtRtn = arr.CopyToDataTable();  //이게 안보임
                }
                else
                {
                    dtRtn = dtRtn.Clone();
                }
            }

            return dtRtn;
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// TU89데이타를 가져온다.
        /// </summary>
        /// <param name="psMst1CD"></param>
        /// <param name="psMst2Cd"></param>
        /// <param name="psSelect"></param>
        /// <param name="psFilter"></param>
        /// <returns></returns>
        public static DataTable GetTU89ByKey(string psMst1CD, string psMst2Cd, string psMst3Cd, string psSelect, string psFilter)
        {
            psSelect = psSelect.Replace(" ", "").ToUpper();
            if (psSelect == "")
            {
                psSelect = " *";
            }
            psSelect = psSelect.Replace(";", ",");

            string sSQL = "";
            sSQL += @"   SELECT " + psSelect + @" 
                              , MST3CD + '-'  + FLD1 V_DATA
                           FROM TU89 U89  (nolock) 
                          WHERE 1 = 1 ";
            if (psMst1CD != "")
            {
                sSQL += @"   AND U89.MST1CD = '" + psMst1CD + "' ";
            }
            if (psMst2Cd != "")
            {
                sSQL += @"   AND U89.MST2CD = '" + psMst2Cd + "' ";
            }
            if (psMst3Cd != "")
            {
                sSQL += @"   AND U89.MST3CD = '" + psMst3Cd + "' ";
            }
            sSQL += @"   ORDER BY U89.MST1CD
                                , U89.MST2CD
                                , U89.MST3CD ";


            DataTable dtRtn = ExecuteDataTable(sSQL);

            if (psFilter != "")
            {
                DataRow[] arr = dtRtn.Select(psFilter);

                if (arr.Length > 0)
                {
                    dtRtn = dtRtn.Clone();
                    for (int ii = 0; ii < arr.Length; ii++)
                    {
                        dtRtn.Rows.Add(arr[ii].ItemArray);
                    }
                    //dtRtn = arr.CopyToDataTable();  //이게 안보임
                }
                else
                {
                    dtRtn = dtRtn.Clone();
                }
            }

            return dtRtn;
        }

        /// <summary>
        /// TU89데이타를 union으로 가져온다.  Where => ex)"1,2;3,2;";
        /// </summary>
        /// <param name="psWhere"></param>
        /// <param name="psSelect"></param>
        /// <param name="psFilter"></param>
        /// <returns></returns>
        public static DataTable GetTU89UByKey(string psWhere, string psSelect, string psFilter)
        {
            string sSQL = "";

            psWhere = psWhere.Replace(" ", "").ToUpper();  //"1,2;3,2;";
            psSelect = psSelect.Replace(" ", "").ToUpper();
            if (psSelect == "")
            {
                psSelect = " *";
            }
            psSelect = psSelect.Replace(";", ",");

            string[] arrsWhere = psWhere.Split(';'); //쿼리갯수 
            if (arrsWhere.Length < 1)
            {
                return null;
            }

            for (int ii = 0; ii < arrsWhere.Length; ii++) //쿼리갯수까지
            {
                if (arrsWhere[ii] != "") //마지막에 문자열 처리 : 추가했다면 빠짐
                {
                    string[] arrDetail = arrsWhere[ii].Trim().Split(','); //쿼리할 필드갯수

                    bool blChk = false;
                    for (int jj = 0; jj < arrDetail.Length; jj++)
                    {
                        if (arrDetail[jj] != "")
                        {
                            blChk = true;
                            break;
                        }
                    }

                    if (blChk == true) //필드값 있다면 실제쿼리함.
                    {
                        if (sSQL != "")
                        {
                            sSQL += @"  UNION ALL";
                        }
                        sSQL += @"   SELECT " + psSelect + @" 
                                       FROM TU89 U89  (nolock) 
                                      WHERE 1 = 1 ";
                        if (arrDetail[0] != "")
                        {
                            sSQL += @"   AND U89.MST1CD = '" + arrDetail[0] + "' ";
                        }
                        if (arrDetail[1] != "")
                        {
                            sSQL += @"   AND U89.MST2CD = '" + arrDetail[1] + "' ";
                        }
                        if (arrDetail[2] != "")
                        {
                            sSQL += @"   AND U89.MST3CD = '" + arrDetail[2] + "' ";
                        }
                    }
                }
            }
            sSQL += @"   ORDER BY MST1CD
                                , MST2CD
                                , MST3CD ";


            DataTable dtRtn = ExecuteDataTable(sSQL);

            if (psFilter != "")
            {
                DataRow[] arr = dtRtn.Select(psFilter);

                if (arr.Length > 0)
                {
                    dtRtn = dtRtn.Clone();
                    for (int ii = 0; ii < arr.Length; ii++)
                    {
                        dtRtn.Rows.Add(arr[ii].ItemArray);
                    }
                    //dtRtn = arr.CopyToDataTable();  //이게 안보임
                }
                else
                {
                    dtRtn = dtRtn.Clone();
                }
            }

            return dtRtn;
        }



        /// <summary>
        /// TU88데이타를 가져온다.
        /// </summary>
        /// <param name="psMst1CD"></param>
        /// <param name="psMst2Cd"></param>
        /// <param name="psSelect"></param>
        /// <param name="psFilter"></param>
        /// <returns></returns>
        public static DataTable GetTU88ByKey(string psMst1CD, string psMst2Cd, string psMst3Cd, string psSelect, string psFilter)
        {
            psSelect = psSelect.Replace(" ", "").ToUpper();
            if (psSelect == "")
            {
                psSelect = " *";
            }
            psSelect = psSelect.Replace(";", ",");

            string sSQL = "";
            sSQL += @"   SELECT " + psSelect + @" 
                              , MST2CD + '-'  + FLD1 V_DATA
                           FROM TU88 U88  (nolock) 
                          WHERE 1 = 1 ";
            if (psMst1CD != "")
            {
                sSQL += @"   AND U88.MST1CD = '" + psMst1CD + "' ";
            }
            if (psMst2Cd != "")
            {
                sSQL += @"   AND U88.MST2CD = '" + psMst2Cd + "' ";
            }

            sSQL += @"   ORDER BY U88.MST1CD
                                , U88.MST2CD";


            DataTable dtRtn = ExecuteDataTable(sSQL);

            if (psFilter != "")
            {
                DataRow[] arr = dtRtn.Select(psFilter);

                if (arr.Length > 0)
                {
                    dtRtn = dtRtn.Clone();
                    for (int ii = 0; ii < arr.Length; ii++)
                    {
                        dtRtn.Rows.Add(arr[ii].ItemArray);
                    }
                    //dtRtn = arr.CopyToDataTable();  //이게 안보임
                }
                else
                {
                    dtRtn = dtRtn.Clone();
                }
            }

            return dtRtn;
        }

        /// <summary>
        /// TU88데이타를 union으로 가져온다.  Where => ex)"1,2;3,2;";
        /// </summary>
        /// <param name="psWhere"></param>
        /// <param name="psSelect"></param>
        /// <param name="psFilter"></param>
        /// <returns></returns>
        public static DataTable GetTU88UByKey(string psWhere, string psSelect, string psFilter)
        {
            string sSQL = "";

            psWhere = psWhere.Replace(" ", "").ToUpper();  //"1,2;3,2;";
            psSelect = psSelect.Replace(" ", "").ToUpper();
            if (psSelect == "")
            {
                psSelect = " *";
            }
            psSelect = psSelect.Replace(";", ",");

            string[] arrsWhere = psWhere.Split(';'); //쿼리갯수 
            if (arrsWhere.Length < 1)
            {
                return null;
            }

            for (int ii = 0; ii < arrsWhere.Length; ii++) //쿼리갯수까지
            {
                if (arrsWhere[ii] != "") //마지막에 문자열 처리 : 추가했다면 빠짐
                {
                    string[] arrDetail = arrsWhere[ii].Trim().Split(','); //쿼리할 필드갯수

                    bool blChk = false;
                    for (int jj = 0; jj < arrDetail.Length; jj++)
                    {
                        if (arrDetail[jj] != "")
                        {
                            blChk = true;
                            break;
                        }
                    }

                    if (blChk == true) //필드값 있다면 실제쿼리함.
                    {
                        if (sSQL != "")
                        {
                            sSQL += @"  UNION ALL";
                        }
                        sSQL += @"   SELECT " + psSelect + @" 
                                       FROM TU88 A  (nolock) 
                                      WHERE 1 = 1 ";
                        if (arrDetail[0] != "")
                        {
                            sSQL += @"   AND A.MST1CD = '" + arrDetail[0] + "' ";
                        }
                        if (arrDetail[1] != "")
                        {
                            sSQL += @"   AND A.MST2CD = '" + arrDetail[1] + "' ";
                        }
                    }
                }
            }
            sSQL += @"   ORDER BY MST1CD
                                , MST2CD ";


            DataTable dtRtn = ExecuteDataTable(sSQL);

            if (psFilter != "")
            {
                DataRow[] arr = dtRtn.Select(psFilter);

                if (arr.Length > 0)
                {
                    dtRtn = dtRtn.Clone();
                    for (int ii = 0; ii < arr.Length; ii++)
                    {
                        dtRtn.Rows.Add(arr[ii].ItemArray);
                    }
                    //dtRtn = arr.CopyToDataTable();  //이게 안보임
                }
                else
                {
                    dtRtn = dtRtn.Clone();
                }
            }

            return dtRtn;
        }

        #endregion


        /// <summary>
        /// TU19 데이타 가져오기 
        /// </summary>
        /// <param name="psMst1CD"></param>
        /// <param name="psMst2Cd"></param>
        /// <param name="psMst3Cd"></param>
        /// <param name="psSelect"></param>
        /// <param name="psFilter"></param>
        /// <returns></returns>
        public static DataTable GetTU19ByKey(string psMst1CD, string psMst2Cd, string psMst3Cd, string psSelect, string psFilter)
        {
            psSelect = psSelect.Replace(" ", "").ToUpper();
            if (psSelect == "")
            {
                psSelect = " *";
            }
            psSelect = psSelect.Replace(";", ",");

            string sSQL = "";
            sSQL += @"   SELECT " + psSelect + @" 
                           FROM TU19 A  (nolock) 
                          WHERE 1 = 1 ";
            if (psMst1CD != "")
            {
                sSQL += @"   AND A.DPTCD = '" + psMst1CD + "' ";
            }
            if (psMst2Cd != "")
            {
                sSQL += @"   AND A.ODIVCD = '" + psMst2Cd + "' ";
            }
            if (psMst3Cd != "")
            {
                sSQL += @"   AND A.OCD = '" + psMst3Cd + "' ";
            }

            sSQL += @"   ORDER BY A.DPTCD
                                , A.ODIVCD ";

            DataTable dtRtn = ExecuteDataTable(sSQL);

            if (psFilter != "")
            {
                DataRow[] arr = dtRtn.Select(psFilter);

                if (arr.Length > 0)
                {
                    dtRtn = dtRtn.Clone();
                    for (int ii = 0; ii < arr.Length; ii++)
                    {
                        dtRtn.Rows.Add(arr[ii].ItemArray);
                    }
                    //dtRtn = arr.CopyToDataTable();  //이게 안보임
                }
                else
                {
                    dtRtn = dtRtn.Clone();
                }
            }

            return dtRtn;
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static DataTable GetTA35ByKey(string psMST1CD, string psMST2CD, string psMST3CD, string psSelect, string psFilter)
        {
            psSelect = psSelect.Replace(" ", "").ToUpper();
            if (psSelect == "")
            {
                psSelect = " *";
            }
            psSelect = psSelect.Replace(";", ",");

            string sSQL = "";
            sSQL += @"   SELECT " + psSelect + @" 
                          FROM TA35 A  (nolock) 
                         WHERE 1 = 1 ";
            if (psMST1CD != "")
            {
                sSQL += @"   AND A.MST1CD = '" + psMST1CD + "' ";
            }
            if (psMST2CD != "")
            {
                sSQL += @"   AND A.MST2CD = '" + psMST2CD + "' ";
            }
            if (psMST3CD != "")
            {
                sSQL += @"   AND A.MST3CD = '" + psMST3CD + "' ";
            }
            sSQL += @"   ORDER BY A.MST1CD
                                , A.MST2CD
                                , A.MST3CD";


            DataTable dtRtn = ExecuteDataTable(sSQL);

            if (psFilter != "")
            {
                DataRow[] arr = dtRtn.Select(psFilter);

                if (arr.Length > 0)
                {
                    dtRtn = dtRtn.Clone();
                    for (int ii = 0; ii < arr.Length; ii++)
                    {
                        dtRtn.Rows.Add(arr[ii].ItemArray);
                    }
                    //dtRtn = arr.CopyToDataTable();  //이게 안보임
                }
                else
                {
                    dtRtn = dtRtn.Clone();
                }
            }

            return dtRtn;
        }

        public static DataTable GetTA35ByKey(string psWhere, string psSelect, string psFilter)
        {
            string sSQL = "";

            psWhere = psWhere.Replace(" ", "").ToUpper();  //"1,2;3,2;";
            psSelect = psSelect.Replace(" ", "").ToUpper();
            if (psSelect == "")
            {
                psSelect = " *";
            }
            psSelect = psSelect.Replace(";", ",");

            string[] arrsWhere = psWhere.Split(';'); //쿼리갯수 
            if (arrsWhere.Length < 1)
            {
                return null;
            }

            for (int ii = 0; ii < arrsWhere.Length; ii++) //쿼리갯수까지
            {
                if (arrsWhere[ii] != "") //마지막에 문자열 처리 : 추가했다면 빠짐
                {
                    string[] arrDetail = arrsWhere[ii].Trim().Split(','); //쿼리할 필드갯수

                    bool blChk = false;
                    for (int jj = 0; jj < arrDetail.Length; jj++)
                    {
                        if (arrDetail[jj] != "")
                        {
                            blChk = true;
                            break;
                        }
                    }

                    if (blChk == true) //필드값 있다면 실제쿼리함.
                    {
                        if (sSQL != "")
                        {
                            sSQL += @"  UNION ALL";
                        }
                        sSQL += @"   SELECT " + psSelect + @" 
                                       FROM TA35 A  (nolock) 
                                      WHERE 1 = 1 ";
                        if (arrDetail[0] != "")
                        {
                            sSQL += @"   AND A.MST1CD = '" + arrDetail[0] + "' ";
                        }
                        if (arrDetail[1] != "")
                        {
                            sSQL += @"   AND A.MST2CD = '" + arrDetail[1] + "' ";
                        }
                        if (arrDetail[2] != "")
                        {
                            sSQL += @"   AND A.MST3CD = '" + arrDetail[2] + "' ";
                        }
                    }
                }
            }
            sSQL += @"   ORDER BY MST1CD
                                , MST2CD
                                , MST3CD ";
            DataTable dtRtn = ExecuteDataTable(sSQL);

            if (psFilter != "")
            {
                DataRow[] arr = dtRtn.Select(psFilter);

                if (arr.Length > 0)
                {
                    dtRtn = dtRtn.Clone();
                    for (int ii = 0; ii < arr.Length; ii++)
                    {
                        dtRtn.Rows.Add(arr[ii].ItemArray);
                    }
                    //dtRtn = arr.CopyToDataTable();  //이게 안보임
                }
                else
                {
                    dtRtn = dtRtn.Clone();
                }
            }

            return dtRtn;



        }
    }
}
