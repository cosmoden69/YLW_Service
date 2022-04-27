using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;

namespace MetroSoft.HIS
{
    public class DataTransactionManager : IDisposable
    {
        private static DataTransactionManager current = null;
        
        private SqlConnection _conn;
        private TransactionScope _tran;

        public SqlConnection Connection
        {
            get { return _conn; }
        }

        public bool TransactionBegun { get => _tran != null; }

        private DataTransactionManager(SqlConnection conn)
        {   _conn = conn;
            _tran = null;            
        }

        /// <summary>
        /// 트랜잭션을 생성하여 반환합니다. 
        /// </summary>
        /// <returns></returns>
        public static DataTransactionManager GetNew()
        {
            try
            {
                current = new DataTransactionManager(
                        DataAccessObject.GetNewConnection());

                return current;
            }
            catch //(Exception ex)
            {
                return null;
            }
        }

        public static DataTransactionManager GetCurrent()
        {
            return current;
        }

        /// <summary>
        /// 트랜잭션을 시작합니다. 이 구문 아래에 있는 모든 DB Access 구문은 반드시 이 TransactionManager의 Connection을 사용하여 실행되어야 합니다.
        /// 아닐 경우 오류가 발생합니다.
        /// </summary>
        /// <returns></returns>
        public bool BeginTransaction()
        {
            try
            {
                //DB가 무한 Lock 이 걸리는 것을 방지하기 위해 15초로 트랜젝션 최대 실행시간을 제한합니다. 
                //1. 디버깅 할시에는 클라이언트에서 BeginTransaction 을 필히 주석처리후 진행하세요 (안그럼 오류납니다.)
                _tran = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 0, 15));
                return true;
            }
            catch
            {
                if (_tran != null)
                    _tran.Dispose();

                return false;
            }
        }

        /// <summary>
        /// 트랜잭션을 시작합니다. 이 구문 아래에 있는 모든 DB Access 구문은 반드시 이 TransactionManager의 Connection을 사용하여 실행되어야 합니다.
        /// 아닐 경우 오류가 발생합니다.
        /// </summary>
        /// <returns></returns>
        public bool BeginTransaction( int iHours , int iminutes   ,  int iseconds  )
        {
            try
            {
                //DB가 무한 Lock 이 걸리는 것을 방지하기 위해 15초로 트랜젝션 최대 실행시간을 제한합니다. 
                //1. 디버깅 할시에는 클라이언트에서 BeginTransaction 을 필히 주석처리후 진행하세요 (안그럼 오류납니다.)
                //2. 트랜잭션 시간을넘깁니다.없을경우 기본은 15초입니다.
                //   2-1.특정 작업시 15초 작업되지 않습니다.  2-2.특정 작업시 팝업 제공 및 정보 입력하면 15초 모자랍니다.
                //   2-2 오버라이딩 시킨 이유 : 인자값 옵셔널로 변강처리 하니 기존 begintrasn... 오류남. 
                _tran = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(iHours, iminutes, iseconds));

                return true;
            }
            catch
            {
                if (_tran != null)
                    _tran.Dispose();

                return false;
            }
        }



        public bool Commit()
        {
            if (_tran != null)
            {
                _tran.Complete();

                _tran.Dispose();
                _tran = null;

                current = null;
                return true;
            }
            else
                throw new TransactionException("Invalid TransactionScope instance.");
        }

        public bool Rollback()
        {
            if (_tran != null)
            {
                _tran.Dispose();
                _tran = null;

                current = null;
                return true;
            }
            else
                throw new TransactionException("Invalid TransactionScope instance.");
        }

        public void Dispose()
        {
            if (_tran != null)
            {
                _tran.Dispose();

                if (current == this)
                    current = null;
            }

            if (_conn != null)
            {
                _conn.Close();
                _conn.Dispose();
            }
        }
    }
}
