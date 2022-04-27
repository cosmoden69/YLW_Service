using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MetroSoft.HIS;
using YLWService.Extensions;

namespace YLWService
{
    public class YLWDBService
    {
        public static YlwSecurityJson SecurityJson { get { return securityJson; } set { securityJson = value; } }

        static string configFileName = "YLWService.Config";  //환경파일
        static Encoding baseEncoding = Encoding.GetEncoding("ks_c_5601-1987");
        static XmlDocument configXml = new XmlDocument();  //config File Xml 관리 Document

        static string ApiDomain = "http://metrokstudio.ksystemace.com/Angkor.ylw.Common.HttpExecute/RestOutsideService.svc/GetServiceMethodSQLWFJson";
        static string ApiDomainPost = "http://metrokstudio.ksystemace.com/Angkor.ylw.Common.HttpExecute/RestOutsideService.svc/OpenApi";
        static string encryptionType = "0";
        static string timeOut = "30";
        static string inPath = "in";
        static string outPath = "out";
        static string responsePath = "re";
        static string sendfilePath = "send";
        static string getfilePath = "get";
        static string startupPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); //Application.StartupPath;
        static YlwSecurityJson securityJson = new YlwSecurityJson();

        static YLWDBService()
        {
            //config Xml Load
            string fileName = GetStartupPath() + @"\" + configFileName;
            SetConfigFile(fileName);
        }

        public static void SetConfigFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Debug.WriteLine("Config file(" + fileName + ") does not exist");
                throw new Exception("Config file(" + fileName + ") does not exist");
            }

            try
            {
                TextReader textReader = new StreamReader(fileName);
                XmlReader xmlReader = new XmlTextReader(textReader);
                configXml.Load(xmlReader);
                xmlReader.Close();
                textReader.Close();
            }
            catch (Exception xe)
            {
                Debug.WriteLine("Config file(" + configFileName + ") load error[" + xe.Message + "]");
                throw new Exception("Config file(" + configFileName + ") load error[" + xe.Message + "]");
            }

            // Encoding Type Set (Default로 SET)
            baseEncoding = Encoding.GetEncoding("ks_c_5601-1987");

            XmlNodeList nodeList = configXml.SelectNodes("configuration");

            foreach (XmlNode node in nodeList)
            {
                ApiDomain = GetNodeText((XmlElement)node, "ApiDomain", ApiDomain);
                ApiDomainPost = GetNodeText((XmlElement)node, "ApiDomainPost", ApiDomainPost);
                encryptionType = GetNodeText((XmlElement)node, "encryptionType", encryptionType);
                timeOut = GetNodeText((XmlElement)node, "timeOut", timeOut);
                //string jsonStr = JsonConvert.SerializeXmlNode(node["SecurityJson"], Newtonsoft.Json.Formatting.None, true);
                string cmpSeq = GetNodeText(node["SecurityJson"], "companySeq");
                if (cmpSeq != "") securityJson.companySeq = Utils.ToInt(cmpSeq);
                securityJson.certId = GetNodeText(node["SecurityJson"], "certId");
                securityJson.certKey = GetNodeText(node["SecurityJson"], "certKey");
                securityJson.dsnOper = GetNodeText(node["SecurityJson"], "dsnOper");
                securityJson.dsn = GetNodeText(node["SecurityJson"], "dsn");
                securityJson.hostComputername = Utils.GetHostName();
                securityJson.hostIPAddress = Utils.GetMyIp();
                securityJson.userId = GetNodeText(node["SecurityJson"], "userId");
                securityJson.userPwd = GetNodeText(node["SecurityJson"], "userPwd");
                inPath = GetNodeText((XmlElement)node, "inPath", inPath);
                outPath = GetNodeText((XmlElement)node, "outPath", outPath);
                responsePath = GetNodeText((XmlElement)node, "responsePath", responsePath);
                sendfilePath = GetNodeText((XmlElement)node, "sendfilePath", sendfilePath);
                getfilePath = GetNodeText((XmlElement)node, "getfilePath", getfilePath);
                break;
            }
        }

        public static string GetInPath()
        {
            return GetStartupPath() + @"\" + inPath;
        }

        public static string GetOutPath()
        {
            return GetStartupPath() + @"\" + outPath;
        }

        public static string GetSendfilePath()
        {
            return GetStartupPath() + @"\" + sendfilePath;
        }

        public static string GetGetfilePath()
        {
            return GetStartupPath() + @"\" + getfilePath;
        }

        public static string GetStartupPath()
        {
            return startupPath;
        }

        private static string GetNodeText(XmlElement node, string nodeName, string defaultValue = "")
        {
            try
            {
                if (node[nodeName] != null)
                {
                    string rtnValue = node[nodeName].InnerText;
                    string enc = node[nodeName].GetAttribute("Encrypt");
                    //연결문자열이 암호화되지 않았으면 암호화 실행
                    if (enc == "Off")
                    {
                        string encstr = EncryptionUtil.EncryptString(rtnValue);
                        node[nodeName].InnerText = encstr;
                        node[nodeName].SetAttribute("Encrypt", "On");
                        SaveConfigFile();
                    }
                    else if (enc == "On") rtnValue = EncryptionUtil.DecryptString(rtnValue).Trim('\0');
                    return rtnValue;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return defaultValue;
        }

        private static void SaveConfigFile()
        {
            try
            {
                //Save Config File
                using (TextWriter tw = new StreamWriter(GetStartupPath() + @"\" + configFileName, false, baseEncoding))
                    configXml.Save(tw);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataSet CallYlwServiceCallPost(YlwSecurityJson security, DataSet ds)
        {
            string dataJson = ds.ToJsonString();

            // Parameter 추가 구문.
            var URL = new Uri(ApiDomainPost + "/" + security.serviceId + "/" + security.methodId);

            JObject postObject = new JObject();
            JObject secObj = new JObject();
            secObj.Add("certId", security.certId);
            secObj.Add("certKey", security.certKey);
            secObj.Add("dsnOper", security.dsnOper);
            secObj.Add("companySeq", security.companySeq);
            secObj.Add("securityType", security.securityType);
            secObj.Add("userId", security.userId);
            secObj.Add("data", dataJson);
            postObject.Add("ROOT", secObj);

            var postData = Utils.JObjectToJsonstring(postObject);
            var data = UTF8Encoding.UTF8.GetBytes(postData);

            DataSet dsr = ExecuteQuery(ds, security.serviceId, security.methodId);

            return dsr;
        }

        //GET 방식
        public static YlwDataSet CallYlwServiceCall(YlwSecurityJson security, DataSet ds)
        {
            string strReturn = string.Empty;

            string securityJson = Utils.ClassToJsonstring(security);
            securityJson = securityJson.Replace("\"", "");   //securityJson 에 " 가 있으면 영림원에러 발생됨.
            string dataJson = ds.ToJsonString();

            // Parameter 추가 구문.
            var URL = new Uri(ApiDomain);
            URL = URL.AddQuery("securityJson", securityJson);
            URL = URL.AddQuery("dataJson", dataJson);
            URL = URL.AddQuery("encryptionType", encryptionType);
            URL = URL.AddQuery("timeOut", timeOut);

            // 서비스 콜.
            var webrequest = (HttpWebRequest)System.Net.WebRequest.Create(URL);

            // 서비스 응답 메시지 처리구문.
            using (var response = webrequest.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var result = reader.ReadToEnd();
                    strReturn = Convert.ToString(result);
                }
            }

            try
            {
                JToken.Parse(strReturn);  //JSON 문자열이 아니면 Catch 로
            }
            catch
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(strReturn);
                XmlNodeList nodeList = doc.GetElementsByTagName("ErrorMessage");
                if (nodeList != null && nodeList.Count > 0)
                {
                    //LogWriter.WriteLog("ErrorMessage:\r\n" + nodeList[0].InnerText);
                    throw new Exception(nodeList[0].InnerText);
                }
                //LogWriter.WriteLog("JToken.Parse Error");
                throw new Exception("JToken.Parse Error");
            }
            string deserialized = JsonConvert.DeserializeObject<string>(strReturn);
            JsonSerializerSettings settings = new JsonSerializerSettings() { StringEscapeHandling = StringEscapeHandling.EscapeHtml };
            YlwDataSet yds = JsonConvert.DeserializeObject<YlwDataSet>(deserialized, settings);
            if (yds.Tables[0].TableName == "ErrorMessage")
            {
                //LogWriter.WriteLog("ErrorMessage:\r\n" + yds.Tables[0].Rows[0]["Result"].ToString());
                throw new Exception("ErrorMessage:\r\n" + yds.Tables[0].Rows[0]["Result"].ToString());
            }
            return yds;
        }

        public static DataTable GetYlwServiceDataTable(string strSql)
        {
            YlwSecurityJson security = YLWService.MTRServiceModule.SecurityJson.Clone();  //깊은복사
            security.serviceId = "Metro.Package.AdjSL.BisAdjSLReturnJSON";
            security.methodId = "Query";

            DataSet ds = new DataSet("ROOT");
            DataTable dt = ds.Tables.Add("DataBlock1");

            dt.Columns.Add("SQL");
            DataRow dr = dt.Rows.Add();
            dr["SQL"] = strSql;

            DataSet yds = YLWServiceModule.CallYlwServiceCallPost(security, ds);
            if (yds == null)
            {
                return null;
            }
            if (yds.Tables["DataBlock1"] == null || yds.Tables["DataBlock1"].Rows.Count < 1) return null;
            return JsonConvert.DeserializeObject<DataTable>(yds.Tables["DataBlock1"].Rows[0]["JSON"].ToString());
        }

        private static DataSet ExecuteQuery(DataSet pds, string serviceid, string methodid)
        {
            DataTable dtr = null;
            DataSet dsr = null;

            dtr = GetServiceWF(serviceid, methodid);
            if (dtr == null || dtr.Rows.Count < 1) return null;
            int serviceSeq = Utils.ToInt(dtr.Rows[0]["ServiceSeq"]);
            int methodSeq = Utils.ToInt(dtr.Rows[0]["MethodSeq"]);
            string methodType = dtr.Rows[0]["MethodType"] + "";

            dtr = GetProcedureList(serviceSeq, methodSeq);
            if (dtr == null || dtr.Rows.Count < 1) return null;
            for (int ii = 0; ii < dtr.Rows.Count; ii++)
            {
                string strScript = dtr.Rows[ii]["SqlScriptID"] + "";

                DataTable dtIn = GetBizSchema(serviceSeq, methodSeq, "Input");
                if (dtIn != null && dtIn.Rows.Count > 0)
                {
                    DataTable dt = dtIn.DefaultView.ToTable(true, new string[] { "DataBlock" });
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string blockName = dt.Rows[i]["DataBlock"] + "";
                        DataRow[] drs = dtIn.Select("DataBlock = '" + blockName + "' ");
                        CreateInQuery(pds.Tables[blockName], blockName, drs);
                    }
                }
                DataTable dtOut = GetBizSchema(serviceSeq, methodSeq, "Output");
                if (dtOut != null && dtOut.Rows.Count > 0)
                {
                    DataTable dt = dtOut.DefaultView.ToTable(true, new string[] { "DataBlock" });
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string blockName = dt.Rows[i]["DataBlock"] + "";
                        DataRow[] drs = dtOut.Select("DataBlock = '" + blockName + "' ");
                        DataRow[] dri = dtIn.Select("DataBlock = '" + blockName + "' ");
                        CreateOutQuery(blockName, drs, dri, methodType);
                    }
                }
                dsr = DataAccessObject.ExecuteDataSet(strScript);
                if (methodType == "70023")  //R-조회
                {
                    dsr = GetOutDataBlock(dsr, dtOut);
                }
                else if (methodType == "70002")  //S-저장
                {
                    dsr = new DataSet("ROOT");
                    if (dtOut != null && dtOut.Rows.Count > 0)
                    {
                        DataTable dt = dtOut.DefaultView.ToTable(true, new string[] { "DataBlock" });
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            string blockName = dt.Rows[i]["DataBlock"] + "";
                            DataRow[] drs = dtOut.Select("DataBlock = '" + blockName + "' ");
                            DataTable dtTmp = GetOutDataBlockQuery(blockName, drs);
                            dsr.Tables.Add(dtTmp);
                        }
                    }
                }
                //DataAccessObject.GetConnection().Close();
            }
            return dsr;
        }

        private static int CreateInQuery(DataTable pdt, string tblname, DataRow[] pdrs)
        {
            string strHdrSql = "";
            string strTBLSql = "";
            string strINSSql = "";
            string strSELSql = "";

            try
            {
                strTBLSql = "";
                strTBLSql = strTBLSql + "IF EXISTS (SELECT * FROM TEMPDB.DBO.SYSOBJECTS WHERE id = object_id(N'[TEMPDB].[DBO].[#BIZ_IN_" + tblname + "]')) ";
                strTBLSql = strTBLSql + " DROP TABLE #BIZ_IN_" + tblname + "; ";
                strTBLSql = strTBLSql + "CREATE TABLE #BIZ_IN_" + tblname + " ";
                strTBLSql = strTBLSql + " ( ";
                strTBLSql = strTBLSql + "  DataSeq      INT, ";
                strTBLSql = strTBLSql + "  WorkingTag   NVARCHAR(10), ";
                strTBLSql = strTBLSql + "  MessageType  INT, ";
                strTBLSql = strTBLSql + "  Status       INT, ";
                strTBLSql = strTBLSql + "  Result       NVARCHAR(255) ";
                strTBLSql = strTBLSql + " ); ";

                for (int i = 0; i < pdrs?.Length; i++)
                {
                    string colName = pdrs[i]["DataFieldID"] + "";
                    string colType = GetDataType(pdrs[i]);
                    strTBLSql = strTBLSql + " ALTER TABLE #BIZ_IN_" + tblname + " ADD " + colName + " " + colType + " NULL; ";
                }

                List<IDbDataParameter> lstPara = new List<IDbDataParameter>();
                lstPara.Clear();
                for (int ri = 0; ri < pdt.Rows.Count; ri++)
                {
                    strINSSql = "INSERT INTO #BIZ_IN_" + tblname + " (DataSeq, WorkingTag, MessageType, Status, Result ";
                    strSELSql = "SELECT " + ri + " ";
                    strSELSql = strSELSql + ", '" + (pdt.Columns.Contains("WorkingTag") ? pdt.Rows[ri]["WorkingTag"] : "") + "' ";
                    strSELSql = strSELSql + ", " + (pdt.Columns.Contains("MessageType") ? pdt.Rows[ri]["MessageType"] : "0") + " ";
                    strSELSql = strSELSql + ", " + (pdt.Columns.Contains("Status") ? pdt.Rows[ri]["Status"] : "0") + " ";
                    strSELSql = strSELSql + ", '" + (pdt.Columns.Contains("Result") ? pdt.Rows[ri]["Result"] : "") + "' ";
                    for (int i = 0; i < pdrs?.Length; i++)
                    {
                        string colName = pdrs[i]["DataFieldID"] + "";
                        strINSSql = strINSSql + ", " + colName + " ";
                        strSELSql = strSELSql + ", @" + colName + " ";
                        lstPara.Add(new SqlParameter("@" + colName, (pdt.Columns.Contains(colName) ? pdt.Rows[ri][colName] : "")));
                    }
                    strINSSql = strINSSql + ") ";
                    strSELSql = strSELSql + "; ";
                }

                strHdrSql = strTBLSql;
                DataAccessObject.ExecuteNonQuery(strHdrSql);

                strHdrSql = strINSSql + strSELSql;
                if (strHdrSql == "") return 0;
                return DataAccessObject.ExecuteNonQuery(strHdrSql, lstPara.ToArray());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return -1;
            }
        }

        private static int CreateOutQuery(string tblname, DataRow[] pdrs, DataRow[] pdri, string methodType)
        {
            string strHdrSql = "";
            string strTBLSql = "";
            string strINSSql = "";
            string strSELSql = "";

            try
            {
                strTBLSql = "";
                strTBLSql = strTBLSql + "IF EXISTS (SELECT * FROM TEMPDB.DBO.SYSOBJECTS WHERE id = object_id(N'[TEMPDB].[DBO].[#BIZ_OUT_" + tblname + "]')) ";
                strTBLSql = strTBLSql + " DROP TABLE #BIZ_OUT_" + tblname + "; ";
                strTBLSql = strTBLSql + "CREATE TABLE #BIZ_OUT_" + tblname + " ";
                strTBLSql = strTBLSql + " ( ";
                strTBLSql = strTBLSql + "  DataSeq      INT, ";
                strTBLSql = strTBLSql + "  WorkingTag   NVARCHAR(10), ";
                strTBLSql = strTBLSql + "  MessageType  INT, ";
                strTBLSql = strTBLSql + "  Status       INT, ";
                strTBLSql = strTBLSql + "  Result       NVARCHAR(255) ";
                strTBLSql = strTBLSql + " ); ";

                for (int i = 0; i < pdrs?.Length; i++)
                {
                    string colName = pdrs[i]["DataFieldID"] + "";
                    string colType = GetDataType(pdrs[i]);
                    strTBLSql = strTBLSql + " ALTER TABLE #BIZ_OUT_" + tblname + " ADD " + colName + " " + colType + " NULL; ";
                }

                strINSSql = "INSERT INTO #BIZ_OUT_" + tblname + " (DataSeq, WorkingTag, MessageType, Status, Result ";
                strSELSql = "SELECT DataSeq, WorkingTag, MessageType, Status, Result ";
                for (int i = 0; i < pdrs?.Length; i++)
                {
                    string colName = pdrs[i]["DataFieldID"] + "";
                    bool find = false;
                    for (int j = 0; j < pdri?.Length; j++) if (pdri[j]["DataFieldID"] + "" == colName) find = true;
                    if (!find) continue;
                    strINSSql = strINSSql + ", " + colName + " ";
                    strSELSql = strSELSql + ", " + colName + " ";
                }
                strINSSql = strINSSql + ") ";
                strSELSql = strSELSql + " FROM #BIZ_IN_" + tblname + " ; ";

                if (methodType == "70023")  //R-조회
                {
                    strHdrSql = strTBLSql + " ; ";
                }
                else if (methodType == "70002")  //S-저장
                {
                    strHdrSql = strTBLSql + strINSSql + strSELSql + " ; ";
                }
                return DataAccessObject.ExecuteNonQuery(strHdrSql);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return -1;
            }
        }

        private static string GetDataType(DataRow pdr)
        {
            string dataType = pdr["DataType"] + "";
            string dataLength = pdr["DataLength"] + "";
            if (dataLength == "-1") dataLength = "MAX";
            if (dataType == "121001") return "NVARCHAR(" + dataLength + ")";
            if (dataType == "121002") return "INT";
            if (dataType == "121003") return "NCHAR(" + dataLength + ")";
            if (dataType == "121004") return "BIT";
            if (dataType == "121005") return "DATETIME";
            if (dataType == "121006") return "DECIMAL(19, 5)";
            if (dataType == "121007") return "IMAGE";
            return "INT";
        }

        private static DataSet GetOutDataBlock(DataSet pds, DataTable pdts)
        {
            DataSet dsRtn = new DataSet("ROOT");
            DataTable dt = null;
            int tableIndex = 0;
            string blockName = "";
            string blockNameBk = "";
            List<string> blockColumns = new List<string>();
            DataRow[] drs = pdts?.Select("Output = '1'");
            for (int i = 0; i < drs.Length; i++)
            {
                blockName = drs[i]["DataBlock"] + "";
                if (blockNameBk != "" && blockName != blockNameBk)
                {
                    if (tableIndex >= pds.Tables.Count) return dsRtn;
                    dt =  pds.Tables[tableIndex].DefaultView.ToTable(false, blockColumns.ToArray());
                    dt.TableName = blockNameBk;
                    dsRtn.Tables.Add(dt);
                    tableIndex++;
                    blockColumns = new List<string>();
                }
                blockColumns.Add(drs[i]["DataFieldID"] + "");
                blockNameBk = blockName;
            }
            if (tableIndex >= pds.Tables.Count) return dsRtn;
            dt = pds.Tables[tableIndex].DefaultView.ToTable(false, blockColumns.ToArray());
            dt.TableName = blockNameBk;
            dsRtn.Tables.Add(dt);
            return dsRtn;
        }

        private static DataTable GetOutDataBlockQuery(string tblname, DataRow[] pdrs)
        {
            string strSELSql = "";

            try
            {
                strSELSql = strSELSql + "SELECT ";
                for (int i = 0; i < pdrs?.Length; i++)
                {
                    string colName = pdrs[i]["DataFieldID"] + "";
                    if (i > 0) strSELSql = strSELSql + " , ";
                    strSELSql = strSELSql + colName;
                }
                strSELSql = strSELSql + " FROM #BIZ_OUT_" + tblname + " ";
                DataTable dtr = DataAccessObject.ExecuteDataTable(strSELSql);
                dtr.TableName = tblname;
                return dtr;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private static int DropQuery(DataTable pdt)
        {
            string tblname = pdt.TableName;

            string strTBLSql = "";

            try
            {
                strTBLSql = "";
                strTBLSql = strTBLSql + "IF EXISTS (SELECT * FROM TEMPDB.DBO.SYSOBJECTS WHERE id = object_id(N'[TEMPDB].[DBO].[#BIZ_IN_" + tblname + "]')) ";
                strTBLSql = strTBLSql + " DROP TABLE #BIZ_IN_" + tblname + "; ";

                return DataAccessObject.ExecuteNonQuery(strTBLSql);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return -1;
            }
        }

        private static DataTable GetBizSchema(int serviceSeq, int methodSeq, string InOut)
        {
            string strSql = @"
                    select blk.DataBlock, blk.DataFieldID, blk.DataFieldName, blk.DataType, blk.DataLength, svcml.Input, svcml.Output
                    from   dbo._TCAServiceMethodLink as svcml WITH(NOLOCK)
                           left join dbo._TCAServiceDataBlock as blk WITH(NOLOCK) ON blk.CompanySeq = svcml.CompanySeq
                                                                                 and blk.ServiceSeq = svcml.ServiceSeq
                                                                                 and blk.DataFieldSeq = svcml.DataFieldSeq
                    where svcml.companyseq = 0
                    and   svcml.ServiceSeq = '" + serviceSeq + @"'
                    and   svcml.MethodSeq = '" + methodSeq + @"' 
                    and   svcml." + InOut + " = '1' ";
            try
            {
                DataSet ds = new DataSet();  //creates data set
                string connectionString = "Data source=192.168.1.145,14233;Initial Catalog=DEVMETROCommon;User ID=cosmoden@metrosoft.co.kr;Password=1;";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(strSql, conn))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        adapter.SelectCommand = cmd;
                        adapter.Fill(ds);
                        adapter.Dispose();
                    }
                    conn.Close();
                }
                if (ds == null) return null;
                if (ds.Tables.Count < 1) return null;
                return ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private static DataTable GetProcedureList(int serviceSeq, int methodSeq)
        {
            string strSql = @"
                    select b.SqlScriptID, b.SqlScriptName
                    from   dbo._TCAServiceMethodKWF a WITH(NOLOCK)
                           join dbo._TCASQLScripts b WITH(NOLOCK) ON b.CompanySeq = a.CompanySeq
	                                                             and b.SqlScriptSeq = a.SqlScriptSeq
                    where  a.companyseq = 0
                    and    a.ServiceSeq = '" + serviceSeq + @"'
                    and    a.MethodSeq = '" + methodSeq + @"' ";
            try
            {
                DataSet ds = new DataSet();  //creates data set
                string connectionString = "Data source=192.168.1.145,14233;Initial Catalog=DEVMETROCommon;User ID=cosmoden@metrosoft.co.kr;Password=1;";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(strSql, conn))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        adapter.SelectCommand = cmd;
                        adapter.Fill(ds);
                        adapter.Dispose();
                    }
                    conn.Close();
                }
                if (ds == null) return null;
                if (ds.Tables.Count < 1) return null;
                return ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        private static DataTable GetServiceWF(string serviceid, string methodid)
        {
            string strSql = @"
                    select svc.ServiceSeq, svcm.MethodSeq, svcm.MethodType
                    from   dbo._TCAService as svc WITH(NOLOCK)
                           join dbo._TCAServiceMethod as svcm WITH(NOLOCK) ON svcm.CompanySeq = svc.CompanySeq
	                                                                      and svcm.ServiceSeq = svc.ServiceSeq
                    where  svc.companyseq = 0
                    and    svc.ServiceID = '" + serviceid + @"'
                    and    svcm.MethodID = '" + methodid + @"' ";
            try
            {
                DataSet ds = new DataSet();  //creates data set
                string connectionString = "Data source=192.168.1.145,14233;Initial Catalog=DEVMETROCommon;User ID=cosmoden@metrosoft.co.kr;Password=1;";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(strSql, conn))
                    {
                        SqlDataAdapter adapter = new SqlDataAdapter();
                        adapter.SelectCommand = cmd;
                        adapter.Fill(ds);
                        adapter.Dispose();
                    }
                    conn.Close();
                }
                if (ds == null) return null;
                if (ds.Tables.Count < 1) return null;
                return ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
    }
}
