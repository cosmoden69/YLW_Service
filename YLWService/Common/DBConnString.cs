/*
 * Created by SharpDevelop.
 * User: hiworld
 * Date: 2011-07-31
 * Time: 오후 8:33
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace MetroSoft.HIS
{
    public class DbConnString
    {
        public static string ServerID = "";           //Server 명
        public static string DataBaseID = "";         //데이터베이스명
        public static string UserID = "";             //사용자명
        public static string UserPassWord = "";	      //사용자비밀번호

        public static string ConnectString()
        {
            string lsConnString = string.Empty;
//            lsConnString = "Server=" + DbConnString.ServerID + ";";
//            lsConnString = lsConnString + "Database=" + DbConnString.DataBaseID + ";";
//            lsConnString = lsConnString + "UID=" + DbConnString.UserID + ";";
//            lsConnString = lsConnString + "PWD=" + DbConnString.UserPassWord;
            lsConnString = "Data Source=" + DbConnString.ServerID + ";";
            lsConnString = lsConnString + "Initial CATALOG=" + DbConnString.DataBaseID + ";";
            lsConnString = lsConnString + "User ID=" + DbConnString.UserID + ";";
            lsConnString = lsConnString + "Password=" + DbConnString.UserPassWord;

            return lsConnString;
        }
    }

}
