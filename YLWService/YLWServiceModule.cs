using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
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
using YLWService.Extensions;

namespace YLWService
{
    public class YLWServiceModule
    {
        public static YlwSecurityJson SecurityJson { get { return securityJson; } set { securityJson = value; } }

        static string configFileName = "YLWService.Config";  //환경파일
        static Encoding baseEncoding = Encoding.GetEncoding("ks_c_5601-1987");
        static XmlDocument configXml = new XmlDocument();  //config File Xml 관리 Document

        static string ApiDomain = "http://metrokstudio.ksystemace.com/Angkor.ylw.Common.HttpExecute/RestOutsideService.svc/GetServiceMethodSQLWFJson";
        static string ApiDomainPost = "http://metrokstudio.ksystemace.com/Angkor.ylw.Common.HttpExecute/RestOutsideService.svc/OpenApi";
        static string ApiDomain2 = "https://metrodev.ksystemace.com/api/WebOpenApi";
        static string encryptionType = "0";
        static string timeOut = "30";
        static string inPath = "in";
        static string outPath = "out";
        static string responsePath = "re";
        static string sendfilePath = "send";
        static string getfilePath = "get";
        static string startupPath = Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);  // Application.StartupPath;
        static string serverFilePath = @"F:\ksystemace\WK\Upload";
        static YlwSecurityJson securityJson = new YlwSecurityJson();

        static YLWServiceModule()
        {
            //config Xml Load
            string fileName = GetStartupPath() + @"\" + configFileName;
            SetConfigFile(fileName);
        }

        public static void SetConfigFile(string fileName)
        {
            //string file = @"F:\ksystemace\haesungHASP\metrolog.log";
            //if (System.IO.File.Exists(file)) System.IO.File.Delete(file);
            //using (System.IO.StreamWriter sw = System.IO.File.CreateText(file))
            //{
            //    sw.Write(Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath));
            //    sw.Close();
            //}
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
                ApiDomain2 = GetNodeText((XmlElement)node, "ApiDomain2", ApiDomain2);
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
                serverFilePath = GetNodeText((XmlElement)node, "serverFilePath", serverFilePath);
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

        public static string GetResposePath()
        {
            return GetStartupPath() + @"\" + responsePath;
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

        public static string ServerFilePath
        {
            get { return serverFilePath; }
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
            secObj.Add("dsnBis", security.dsn);
            secObj.Add("companySeq", security.companySeq);
            secObj.Add("languageSeq", security.languageSeq);
            secObj.Add("securityType", security.securityType);
            secObj.Add("userId", security.userId);
            secObj.Add("data", dataJson);
            postObject.Add("ROOT", secObj);

            var postData = Utils.JObjectToJsonstring(postObject);
            var data = UTF8Encoding.UTF8.GetBytes(postData);

            // 서비스 콜.
            var webrequest = (HttpWebRequest)System.Net.WebRequest.Create(URL);
            webrequest.Method = "POST";
            webrequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            webrequest.ContentLength = data.Length;

            // 서비스 응답 메시지 처리구문.
            using (var stream = webrequest.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            string strReturn = string.Empty;
            using (var response = webrequest.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var result = reader.ReadToEnd();
                    strReturn = Convert.ToString(result);
                }
            }

            XDocument doc;
            try
            {
                doc = XDocument.Parse(strReturn);
            }
            catch
            {
                JsonSerializerSettings settings = new JsonSerializerSettings() { StringEscapeHandling = StringEscapeHandling.EscapeHtml };
                YlwError yerr = JsonConvert.DeserializeObject<YlwError>(strReturn, settings);
                if (yerr != null && yerr.ErrorMessage.Count > 0)
                {
                    //LogWriter.WriteLog("ErrorMessage:\r\n" + yerr.ErrorMessage[0].Result);
                    throw new Exception(yerr.ErrorMessage[0].Result);
                }
                throw new Exception("XDocument.Parse Error");
            }

            var errs = doc.Descendants("ErrorMessage");
            if (errs.Count() > 0)
            {
                //LogWriter.WriteLog("ErrorMessage:\r\n" + errs.FirstOrDefault().Value);
                throw new Exception(errs.FirstOrDefault().Value);
            }
            DataSet dsr = new DataSet();
            dsr.ReadXml(new StringReader(strReturn), XmlReadMode.Auto);
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

        public static DataTable GetYlwServiceDataTable(int companyseq,  string strSql)
        {
            YlwSecurityJson security = YLWServiceModule.SecurityJson.Clone();  //깊은복사
            security.serviceId = "Metro.Package.AdjSL.BisAdjSLReturnJSON";
            security.methodId = "Query";
            security.companySeq = companyseq;

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

        public static DataSet Fileupload(YLWService.YlwSecurityJson security, DataSet ds)
        {
            return AsyncHelper.RunSync(() => CallAsyncFileUpload(security, ds));
        }

        public static string FileuploadGetSeq(YLWService.YlwSecurityJson security, DataSet ds)
        {
            DataSet dsr = AsyncHelper.RunSync(() => CallAsyncFileUpload(security, ds));
            if (dsr != null && dsr.Tables.Contains("DataBlock4"))  //Table1 (WebYlwPlugin_MetroSoft) -> DataBlock4
            {
                return dsr.Tables["DataBlock4"].Rows[0]["AttachFileSeq"].ToString();
            }
            return "";
        }

        private async static Task<DataSet> CallAsyncFileUpload(YLWService.YlwSecurityJson security, DataSet ds)
        {
            try
            {
                /*
                    DataSet ds = new DataSet();
                    ds.Tables.Add("DataBlock1");
                    ds.Tables[0].Columns.Add("CompanySeq");
                    ds.Tables[0].Columns.Add("LanguageSeq");
                    ds.Tables[0].Columns.Add("LoginPgmSeq");
                    ds.Tables[0].Columns.Add("UserSeq");
                    ds.Tables[0].Columns.Add("AttachFileConstSeq");
                    ds.Tables[0].Columns.Add("AttachFileSeq");
                    ds.Tables[0].Columns.Add("AttachFileNo");
                    ds.Tables[0].Columns.Add("IDX_NO");
                    ds.Tables[0].Columns.Add("IsRepFile");
                    ds.Tables[0].Columns.Add("fileName");
                    ds.Tables[0].Columns.Add("fileExt");
                    ds.Tables[0].Columns.Add("FileSize");
                    ds.Tables[0].Columns.Add("Remark");
                    ds.Tables[0].Columns.Add("fileBase64");
                    ds.Tables[0].Rows.Add(new string[] { security.companySeq.ToString(), security.languageSeq.ToString(), "0", security.empSeq.ToString(), fileConstSeq, fileSeq, fileNo, idxNo, "0", fileName, fileExt, fileSize, "", fileBase64 });
                    string ds_str = YLW_Util.GetData<DataSet>(ds);

                    string clientInfo = "metrosoft" + "~," + "ePQ4zTHVSBZqwtGlp8sncmoiLEh2d6j3" + "~," + security.userId + "~," + security.companySeq.ToString();
                    JObject param_json = new JObject();
                    param_json.Add("AssemblyName", "Web.MetroSoft.Plugin");
                    param_json.Add("ClassName", "Web.MetroSoft.Plugin.Common");
                    param_json.Add("MethodName", "FileUpload");
                    param_json.Add("ParamData", ds_str);
                    param_json.Add("JSonData", "");
                    param_json.Add("ClientInfo", clientInfo);
                    string data_str = param_json.ToString();

                    // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
                    var httpContent = new StringContent(data_str, Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        //var httpResponse = await httpClient.PostAsync("http://localhost:65466/api/WebApi", httpContent);
                        //var httpResponse = await httpClient.PostAsync("http://metrosoft.co.kr/haesunghasp/YLWWebApi/api/WebApi", httpContent);
                        var httpResponse = await httpClient.PostAsync(ApiDomain2, httpContent);

                        // If the response contains content we want to read it!
                        if (httpResponse.Content != null)
                        {
                            var responseContent = await httpResponse.Content.ReadAsStringAsync();
                            string deserialized = JsonConvert.DeserializeObject<string>(responseContent);
                            return YLW_Util.JSonToDataSet<DataSet>(deserialized);
                        }
                    }
                    return null;
                */

                DataSet dsData_result = null;
                await Task.Run(() =>
                {
                    ClsConnInfo info = new ClsConnInfo(security);
                    YlwPlugin ylw = new YlwPlugin();
                    dsData_result = ylw.FileUpload(info, ds);
                });
                return dsData_result;
            }
            catch (Exception ex)
            {
                DataSet dsr = new DataSet();
                DataTable dtr = dsr.Tables.Add("ErrorMessage");
                dtr.Columns.Add("Status");
                dtr.Columns.Add("Message");
                DataRow dr = dtr.Rows.Add();
                dr["Status"] = "ERR";
                dr["Message"] = ex.Message;
                return dsr;
            }
        }

        public static DataSet FileDownload(YLWService.YlwSecurityJson security, DataSet ds)
        {
            return AsyncHelper.RunSync(() => CallAsyncFileDownload(security, ds));
        }

        public static string FileDownloadBase64(YLWService.YlwSecurityJson security, DataSet ds)
        {
            DataSet dsr = AsyncHelper.RunSync(() => CallAsyncFileDownload(security, ds));
            if (dsr != null && !dsr.Tables.Contains("ErrorMessage"))
            {
                return dsr.Tables[0].Rows[0]["FileBase64"].ToString();
            }
            return "";
        }

        private async static Task<DataSet> CallAsyncFileDownload(YLWService.YlwSecurityJson security, DataSet ds)
        {
            try
            {
                /*
                    DataSet ds = new DataSet();
                    ds.Tables.Add("DataBlock1");
                    ds.Tables[0].Columns.Add("CompanySeq");
                    ds.Tables[0].Columns.Add("LanguageSeq");
                    ds.Tables[0].Columns.Add("UserSeq");
                    ds.Tables[0].Columns.Add("workingTag");
                    ds.Tables[0].Columns.Add("AttachFileSeq");
                    ds.Tables[0].Columns.Add("AttachFileNo");
                    ds.Tables[0].Columns.Add("IsRepFile");
                    ds.Tables[0].Rows.Add(new string[] { security.companySeq.ToString(), security.languageSeq.ToString(), security.empSeq.ToString(), "Q", attachFileSeq, attachFileNo, isRepFile });

                    string ds_str = YLW_Util.GetData<DataSet>(ds);
                    string clientInfo = "metrosoft" + "~," + "ePQ4zTHVSBZqwtGlp8sncmoiLEh2d6j3" + "~," + security.userId + "~," + security.companySeq.ToString();
                    JObject param_json = new JObject();
                    param_json.Add("AssemblyName", "Web.MetroSoft.Plugin");
                    param_json.Add("ClassName", "Web.MetroSoft.Plugin.Common");
                    param_json.Add("MethodName", "FileDownload");
                    param_json.Add("ParamData", ds_str);
                    param_json.Add("JSonData", "");
                    param_json.Add("ClientInfo", clientInfo);
                    string data_str = param_json.ToString();

                    // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
                    var httpContent = new StringContent(data_str, Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        //var httpResponse = await httpClient.PostAsync("http://localhost:65466/api/WebApi", httpContent);
                        //var httpResponse = await httpClient.PostAsync("http://metrosoft.co.kr/haesunghasp/YLWWebApi/api/WebApi", httpContent);
                        var httpResponse = await httpClient.PostAsync(ApiDomain2, httpContent);

                        // If the response contains content we want to read it!
                        if (httpResponse.Content != null)
                        {
                            var responseContent = await httpResponse.Content.ReadAsStringAsync();
                            string deserialized = JsonConvert.DeserializeObject<string>(responseContent);
                            return YLW_Util.JSonToDataSet<DataSet>(deserialized);
                        }
                    }
                    return null;
                */

                DataSet dsData_result = null;
                await Task.Run(() =>
                {
                    ClsConnInfo info = new ClsConnInfo(security);
                    YlwPlugin ylw = new YlwPlugin();
                    dsData_result = ylw.FileDownload(info, ds);
                });
                return dsData_result;
            }
            catch (Exception ex)
            {
                DataSet dsr = new DataSet();
                DataTable dtr = dsr.Tables.Add("ErrorMessage");
                dtr.Columns.Add("Status");
                dtr.Columns.Add("Message");
                DataRow dr = dtr.Rows.Add();
                dr["Status"] = "ERR";
                dr["Message"] = ex.Message;
                return dsr;
            }
        }

        public static string FileDelete(YLWService.YlwSecurityJson security, DataSet ds)
        {
            DataSet dsr = AsyncHelper.RunSync(() => CallAsyncFileDelete(security, ds));
            if (dsr != null && !dsr.Tables.Contains("ErrorMessage"))
            {
                return dsr.Tables[0].Rows[0]["Status"].ToString();
            }
            return "";
        }

        private async static Task<DataSet> CallAsyncFileDelete(YLWService.YlwSecurityJson security, DataSet ds)
        {
            try
            {
                /*
                    DataSet ds = new DataSet();
                    ds.Tables.Add("DataBlock1");
                    ds.Tables[0].Columns.Add("CompanySeq");
                    ds.Tables[0].Columns.Add("LanguageSeq");
                    ds.Tables[0].Columns.Add("LoginPgmSeq");
                    ds.Tables[0].Columns.Add("UserSeq");
                    ds.Tables[0].Columns.Add("AttachFileConstSeq");
                    ds.Tables[0].Columns.Add("AttachFileSeq");
                    ds.Tables[0].Columns.Add("AttachFileNo");
                    ds.Tables[0].Rows.Add(new string[] { security.companySeq.ToString(), security.languageSeq.ToString(), "0", security.empSeq.ToString(), "0", attachFileSeq, attachFileNo });

                    string ds_str = YLW_Util.GetData<DataSet>(ds);
                    string clientInfo = "metrosoft" + "~," + "ePQ4zTHVSBZqwtGlp8sncmoiLEh2d6j3" + "~," + security.userId + "~," + security.companySeq.ToString();
                    JObject param_json = new JObject();
                    param_json.Add("AssemblyName", "Web.MetroSoft.Plugin");
                    param_json.Add("ClassName", "Web.MetroSoft.Plugin.Common");
                    param_json.Add("MethodName", "FileDelete");
                    param_json.Add("ParamData", ds_str);
                    param_json.Add("JSonData", "");
                    param_json.Add("ClientInfo", clientInfo);
                    string data_str = param_json.ToString();

                    // Wrap our JSON inside a StringContent which then can be used by the HttpClient class
                    var httpContent = new StringContent(data_str, Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        //var httpResponse = await httpClient.PostAsync("http://localhost:65466/api/WebApi", httpContent);
                        //var httpResponse = await httpClient.PostAsync("http://metrosoft.co.kr/haesunghasp/YLWWebApi/api/WebApi", httpContent);
                        var httpResponse = await httpClient.PostAsync(ApiDomain2, httpContent);

                        // If the response contains content we want to read it!
                        if (httpResponse.Content != null)
                        {
                            var responseContent = await httpResponse.Content.ReadAsStringAsync();
                            string deserialized = JsonConvert.DeserializeObject<string>(responseContent);
                            //return YLW_Util.JSonToDataSet<DataSet>(deserialized); 
                            //위의 YLW_Util.JSonToDataSet에서 FileUploadDateTime 에러남 _SCAAttachFileSave에서 FileUploadDateTime 값을 채우지 않음
                            //그래서 아래의 데이타셋을 임의로 반환함, 여기까지 왔으면 삭제가 됬다고 본다
                            DataSet dsr = new DataSet();
                            DataTable dtr = dsr.Tables.Add("Table1");
                            dtr.Columns.Add("Status");
                            DataRow dr = dtr.Rows.Add();
                            dr["Status"] = "OK";
                            return dsr;
                        }
                    }
                    return null;
                */

                DataSet dsData_result = null;
                await Task.Run(() => 
                {
                    ClsConnInfo info = new ClsConnInfo(security);
                    YlwPlugin ylw = new YlwPlugin();
                    dsData_result = ylw.FileDelete(info, ds);
                });
                return dsData_result;
            }
            catch (Exception ex)
            {
                DataSet dsr = new DataSet();
                DataTable dtr = dsr.Tables.Add("ErrorMessage");
                dtr.Columns.Add("Status");
                dtr.Columns.Add("Message");
                DataRow dr = dtr.Rows.Add();
                dr["Status"] = "ERR";
                dr["Message"] = ex.Message;
                return dsr;
            }
        }

        public static DataSet FileUpdate(YLWService.YlwSecurityJson security, DataSet ds)
        {
            return AsyncHelper.RunSync(() => CallAsyncFileUpdate(security, ds));
        }

        private async static Task<DataSet> CallAsyncFileUpdate(YLWService.YlwSecurityJson security, DataSet ds)
        {
            try
            {
                DataSet dsData_result = null;
                await Task.Run(() =>
                {
                    ClsConnInfo info = new ClsConnInfo(security);
                    YlwPlugin ylw = new YlwPlugin();
                    dsData_result = ylw.FileUpdate(info, ds);
                });
                return dsData_result;
            }
            catch (Exception ex)
            {
                DataSet dsr = new DataSet();
                DataTable dtr = dsr.Tables.Add("ErrorMessage");
                dtr.Columns.Add("Status");
                dtr.Columns.Add("Message");
                DataRow dr = dtr.Rows.Add();
                dr["Status"] = "ERR";
                dr["Message"] = ex.Message;
                return dsr;
            }
        }

        public static DataSet YDsToDataSet(YlwDataSet dsMyData)
        {
            if (dsMyData == null)
                return null;

            DataSet dsData = new DataSet();

            foreach (YlwTable dtMyData in dsMyData.Tables)
            {
                DataTable dtData = new DataTable(dtMyData.TableName);

                for (int c = 0; c < dtMyData.Columns.Count; c++)
                {
                    dtData.Columns.Add(dtMyData.Columns[c].ColumnName, GetType(dtMyData.Columns[c].ColumnType));
                }

                foreach (JObject row in dtMyData.Rows)
                {
                    DataRow dr = dtData.Rows.Add();
                    foreach (var x in row)
                    {
                        dr[x.Key] = JTokenToObject(dtMyData.Columns.First(xi=>xi.ColumnName == x.Key).ColumnType, x.Value);
                    }
                }

                dsData.Tables.Add(dtData);
            }

            return dsData;
        }

        public static object JTokenToObject(string intJSonType, JToken token)
        {
            switch (intJSonType)
            {
                case "string":
                case "nvarchar":
                case "varchar":
                case "nchar":
                case "char":
                    return Utils.ConvertToString(token);
                case "int32":
                case "int":
                    return Utils.ToInt(token);
                case "numeric":
                case "long":
                    return Utils.ToLong(token);
                case "decimal":
                    return Utils.ToDecimal(token);
                case "datetime":
                    return Utils.ConvertToDateTime(token);
                case "bit":
                    return Utils.ToBool(token);
                default:
                    return Utils.ConvertToString(token);
            }
        }

        public static Type GetType(string intJSonType)
        {
            switch (intJSonType)
            {
                case "string":
                case "nvarchar":
                case "varchar":
                case "nchar":
                case "char":
                    return typeof(string);
                case "int32":
                case "int":
                    return typeof(int);
                case "numeric":
                case "long":
                    return typeof(long);
                case "decimal":
                    return typeof(decimal);
                case "datetime":
                    return typeof(DateTime);
                case "bit":
                    return typeof(Boolean);
                default:
                    return typeof(string);
            }
        }
    }

    //https://stackoverflow.com/questions/14435520/why-use-httpclient-for-synchronous-connection/14435574
    public static class AsyncHelper
    {
        private static readonly TaskFactory _taskFactory = new
            TaskFactory(System.Threading.CancellationToken.None,
                        TaskCreationOptions.None,
                        TaskContinuationOptions.None,
                        TaskScheduler.Default);

        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
            => _taskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();

        public static void RunSync(Func<Task> func)
            => _taskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
    }

    #region classes
    public class YlwSecurityJson : ICloneable
    {
        public string certId { get; set; }
        public string certKey { get; set; }
        public string dsnOper { get; set; }
        public string dsn { get; set; }
        public string workingTag { get; set; } = "";
        public int securityType { get; set; } = 0;
        public int isDebug { get; set; } = 0;
        public int companySeq { get; set; } = 1;
        public int languageSeq { get; set; } = 1;
        public string serviceId { get; set; }
        public string methodId { get; set; }
        public string hostComputername { get; set; }
        public string hostIPAddress { get; set; }
        public string userId { get; set; }
        public string userPwd { get; set; }
        public int empSeq { get; set; }

        #region ICloneable Members
        // Type safe Clone
        public YlwSecurityJson Clone()
        {
            return new YlwSecurityJson()
            {
                certId = this.certId,
                certKey = this.certKey,
                dsnOper = this.dsnOper,
                dsn = this.dsn,
                workingTag = this.workingTag,
                securityType = this.securityType,
                isDebug = this.isDebug,
                companySeq = this.companySeq,
                languageSeq = this.languageSeq,
                serviceId = this.serviceId,
                methodId = this.methodId,
                hostComputername = this.hostComputername,
                hostIPAddress = this.hostIPAddress,
                userId = this.userId,
                userPwd = this.userPwd,
                empSeq = this.empSeq
            };
        }

        // ICloneable implementation
        object ICloneable.Clone()
        {
            return Clone();
        }
        #endregion
    }

    public class YlwDataBlockModel
    {
        public string WorkingTag { get; set; } = "";
        public int IDX_NO { get; set; } = 0;
        public string Status { get; set; } = "0";
        public int DataSeq { get; set; } = 1;
        public int Selected { get; set; } = 1;
        public string TABLE_NAME { get; set; } = "";
    }

    public class YlwDataSet
    {
        public List<YlwTable> Tables;

        public YlwTable GetDataBlock(string blockName)
        {
            return this.Tables.Find(x => x.TableName == blockName);
        }
    }

    public class YlwTable
    {
        public string TableName { get; set; }
        public List<YlwColumn> Columns { get; set; }
        public List<JObject> Rows { get; set; }
    }

    public class YlwColumnCollection : System.Collections.CollectionBase
    {
        public YlwColumn this[int index]
        {
            get
            {
                if ((index < 0) || (index >= List.Count)) return null;
                return (YlwColumn)List[index];
            }
        }

        public YlwColumn this[string name]
        {
            get
            {
                if (name == "") return null;
                foreach (YlwColumn var in List)
                    if (var.ColumnName == name)
                        return var;
                return null;
            }
        }

        private void Add(string name, string type)
        {
            bool isFound = false;
            //이미 있으면 Value만 다시 설정
            foreach (YlwColumn item in List)
            {
                if (item.ColumnName == name)
                {
                    isFound = true;
                    break;
                }
            }
            if (!isFound)
            {
                YlwColumn var = new YlwColumn() { ColumnName = name, ColumnType = type };
                List.Add(var);
            }
        }
    }

    public class YlwColumn
    {
        public string ColumnName { get; set; }
        public string ColumnType { get; set; }
    }

    public class YlwPostDataResult
    {
        XDocument _doc = null;
        XElement _elmt = null;

        public XDocument Doc
        {
            get { return _doc; }
            set
            {
                _doc = value;
                _elmt = _doc?.Elements().FirstOrDefault();
            }
        }

        public YlwPostDataResult(XDocument doc)
        {
            this.Doc = doc;
        }

        public DataTable GetDataBlock(string blockName)
        {
            DataSet ds = new DataSet();
            List<XElement> elements = _elmt?.Descendants(blockName).ToList();
            string xml = elements.ToString();
            DataTable dt = new DataTable();
            dt.ReadXml(new System.IO.StringReader(xml));
            return dt;
        }

        public DataTable GetDataBlock<T>(string blockName)
        {
            List<XElement> elements = _elmt?.Descendants(blockName).ToList();
            List<T> yds = elements.Select(x => Utils.Deserialize<T>(x.ToString())).ToList();
            return yds.ToDataTable();
        }
    }

    public class YlwError
    {
        public List<YlwErrorMessage> ErrorMessage { get; set; }
    }

    public class YlwErrorMessage
    {
        public string Status { get; set; }
        public string Result { get; set; }
    }

    public class Response
    {
        // 프로퍼티
        public int Result { get; set; }
        public string Message { get; set; }
    }

    public class ReportParam
    {
        // 프로퍼티
        public string ReportName { get; set; }
        public string AcptMgmtSeq { get; set; }
        public string ReSurvAsgnNo { get; set; }
        public string ParamStr { get; set; }
        public string ReportType { get; set; }
        public string BizNo { get; set; }
        public int Seq { get; set; }
        public int CompanySeq { get; set; }
        public string UserID { get; set; }
    }

    public class AttachFileParam
    {
        // 프로퍼티
        public string PgmName { get; set; }
        public string FileSeqName { get; set; }
        public string AcptMgmtSeq { get; set; }
        public string ReSurvAsgnNo { get; set; }
        public string KeyStr { get; set; }
        public string ReadOnlyFg { get; set; } = "0";
        public string FileConstSeq { get; set; }
        public string FileSeq { get; set; }
        public int CompanySeq { get; set; }
        public string UserID { get; set; }
    }

    public class ReportData
    {
        // 프로퍼티
        public Response Response { get; set; }
        public string ReportName { get; set; }
        public string ReportText { get; set; }
    }

    public class FileData
    {
        // 프로퍼티
        public Response Response { get; set; }
        public AttachFileMaster AttachFileMaster { get; set; }
        public AttachFileData AttachFileData { get; set; }
        public string FileBase64Text { get; set; }
    }

    public class AttachFileMaster
    {
        public int AttachFileConstSeq { get; set; }
        public int AttachFileSeq { get; set; }
        public int LoginPgmSeq { get; set; }
        public string PcName { get; set; }
        public string HostIp { get; set; }
    }

    public class AttachFileData
    { 
        public int IDX_NO { get; set; }
        public string WorkingTag { get; set; }
        public int AttachFileNo { get; set; }
        public int IsRepFile { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public string FileExt { get; set; }
        public int FileSize { get; set; }
        public string Remark { get; set; }
        public string Request { get; set; }
        public string Result { get; set; }
    }

    public class Data_Json
    {
        public string AssemblyName { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public YLW_Util.DataSetJs ParamData { get; set; }
        public YLW_Util.DataSetJs JSonData { get; set; }
    }
    #endregion classes
}
