using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
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

namespace YLWService
{
    public class MTRServiceModule
    {
        public static YlwSecurityJson SecurityJson { get { return securityJson; } set { securityJson = value; } }

        static string configFileName = "MTRService.Config";  //환경파일
        static Encoding baseEncoding = Encoding.GetEncoding("ks_c_5601-1987");
        static XmlDocument configXml = new XmlDocument();  //config File Xml 관리 Document

        static string ApiDomainPost = "http://metrosoft.co.kr/haesungHASP/ReportView/Service1.svc";
        static string encryptionType = "0";
        static string timeOut = "30";
        static string inPath = "in";
        static string outPath = "out";
        static string responsePath = "re";
        static string sendfilePath = "send";
        static string getfilePath = "get";
        static string startupPath = Path.GetDirectoryName(new System.Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).LocalPath);  // Application.StartupPath;
        static YlwSecurityJson securityJson = new YlwSecurityJson();

        static MTRServiceModule()
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

        public static DataTable GetMTRServiceDataTable(int companyseq, string strSql)
        {
            try
            {
                DataSet yds = CallMTRGetDataSetPost(companyseq, strSql);
                if (yds == null || yds.Tables.Count < 1) return null;
                return yds.Tables[0];
            }
            catch
            {
                return null;
            }
        }

        public static DataSet CallMTRServiceCall(YlwSecurityJson security, DataSet ds)
        {
            try
            { 
                ChannelFactory<IMyContract> factory = new ChannelFactory<IMyContract>();

                // Address
                factory.Endpoint.Address = new EndpointAddress(ApiDomainPost);

                // Binding : HTTP 사용
                WebHttpBinding binding = new WebHttpBinding();
                binding.MaxReceivedMessageSize = 2147483647;
                binding.MaxBufferSize = 2147483647;
                binding.MaxBufferPoolSize = 2147483647;
                factory.Endpoint.Binding = binding;

                // Contract 설정
                factory.Endpoint.Contract.ContractType = typeof(IMyContract);
                factory.Endpoint.Behaviors.Add(new WebHttpBehavior());

                // Channel Factory 만들기
                IMyContract channel = factory.CreateChannel();

                // Server 쪽 함수 호출
                DataSet result = channel.ServiceCall(security, ds);

                // Close Channel
                ((ICommunicationObject)channel).Close();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataSet CallMTRServiceCallPost(YlwSecurityJson security, DataSet ds)
        {
            try
            {
                ChannelFactory<IMyContract> factory = new ChannelFactory<IMyContract>();

                // Address
                factory.Endpoint.Address = new EndpointAddress(ApiDomainPost);

                // Binding : HTTP 사용
                WebHttpBinding binding = new WebHttpBinding();
                binding.MaxReceivedMessageSize = 2147483647;
                binding.MaxBufferSize = 2147483647;
                binding.MaxBufferPoolSize = 2147483647;
                factory.Endpoint.Binding = binding;

                // Contract 설정
                factory.Endpoint.Contract.ContractType = typeof(IMyContract);
                factory.Endpoint.Behaviors.Add(new WebHttpBehavior());

                // Channel Factory 만들기
                IMyContract channel = factory.CreateChannel();

                // Server 쪽 함수 호출
                DataSet result = channel.ServiceCallPost(security, ds);
                if (result != null && result.Tables.Contains("ErrorMessage"))
                {
                    DataTable dtr = result.Tables["ErrorMessage"];
                    string msg = "Error : Unknown";
                    if (dtr.Columns.Contains("Message")) msg = dtr.Rows[0]["Message"] + "";
                    throw new Exception(msg);
                }

                // Close Channel
                ((ICommunicationObject)channel).Close();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataSet CallMTRGetDataSetPost(int companyseq, string streamdata)
        {
            try
            {
                ChannelFactory<IMyContract> factory = new ChannelFactory<IMyContract>();

                // Address
                factory.Endpoint.Address = new EndpointAddress(ApiDomainPost);

                // Binding : HTTP 사용
                WebHttpBinding binding = new WebHttpBinding();
                binding.MaxReceivedMessageSize = 2147483647;
                binding.MaxBufferSize = 2147483647;
                binding.MaxBufferPoolSize = 2147483647;
                factory.Endpoint.Binding = binding;

                // Contract 설정
                factory.Endpoint.Contract.ContractType = typeof(IMyContract);
                factory.Endpoint.Behaviors.Add(new WebHttpBehavior());

                // Channel Factory 만들기
                IMyContract channel = factory.CreateChannel();

                // Server 쪽 함수 호출
                DataSet result = channel.GetDataSetPost(companyseq, streamdata);
                if (result != null && result.Tables.Contains("ErrorMessage"))
                {
                    DataTable dtr = result.Tables["ErrorMessage"];
                    string msg = "Error : Unknown";
                    if (dtr.Columns.Contains("Message")) msg = dtr.Rows[0]["Message"] + "";
                    throw new Exception(msg);
                }

                // Close Channel
                ((ICommunicationObject)channel).Close();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataSet CallMTRFileupload(YlwSecurityJson security, string realFile, string fileConstSeq, string fileSeq = "0", string fileNo = "0", string idxNo = "0")
        {
            // File Info
            FileInfo finfo = new FileInfo(realFile);
            byte[] rptbyte = (byte[])MetroSoft.HIS.cFile.ReadBinaryFile(realFile);
            string fileBase64 = Convert.ToBase64String(rptbyte);

            string fileName = finfo.Name;
            string fileExt = Utils.Mid(finfo.Extension, 2, finfo.Extension.Length);
            string fileSize = Utils.ConvertToString(finfo.Length);
            // File Info

            return CallMTRFileupload(security, fileName, fileExt, fileSize, fileBase64, fileConstSeq, fileSeq, fileNo, idxNo);
        }

        public static DataSet CallMTRFileupload(YlwSecurityJson security, FileInfo finfo, string fileBase64, string fileConstSeq, string fileSeq = "0", string fileNo = "0", string idxNo = "0")
        {
            // File Info
            string fileName = finfo.Name;
            string fileExt = Utils.Mid(finfo.Extension, 2, finfo.Extension.Length);
            string fileSize = Utils.ConvertToString(finfo.Length);
            // File Info

            return CallMTRFileupload(security, fileName, fileExt, fileSize, fileBase64, fileConstSeq, fileSeq, fileNo, idxNo);
        }

        public static DataSet CallMTRFileupload(YlwSecurityJson security, string fileName, string fileExt, string fileSize, string fileBase64, string fileConstSeq, string fileSeq = "0", string fileNo = "0", string idxNo = "0")
        {
            try
            {
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

                ChannelFactory<IMyContract> factory = new ChannelFactory<IMyContract>();

                // Address
                factory.Endpoint.Address = new EndpointAddress(ApiDomainPost);

                // Binding : HTTP 사용
                WebHttpBinding binding = new WebHttpBinding();
                binding.MaxReceivedMessageSize = 2147483647;
                binding.MaxBufferSize = 2147483647;
                binding.MaxBufferPoolSize = 2147483647;
                factory.Endpoint.Binding = binding;

                // Contract 설정
                factory.Endpoint.Contract.ContractType = typeof(IMyContract);
                factory.Endpoint.Behaviors.Add(new WebHttpBehavior());

                // Channel Factory 만들기
                IMyContract channel = factory.CreateChannel();

                // Server 쪽 함수 호출
                DataSet result = channel.Fileupload(security, ds);
                if (result != null && result.Tables.Contains("ErrorMessage"))
                {
                    DataTable dtr = result.Tables["ErrorMessage"];
                    string msg = "Error : Unknown";
                    if (dtr.Columns.Contains("Message")) msg = dtr.Rows[0]["Message"] + "";
                    throw new Exception(msg);
                }

                // Close Channel
                ((ICommunicationObject)channel).Close();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string CallMTRFileuploadGetSeq(YlwSecurityJson security, string realFile, string fileConstSeq, string fileSeq = "0", string fileNo = "0", string idxNo = "0")
        {
            // File Info
            FileInfo finfo = new FileInfo(realFile);
            byte[] rptbyte = (byte[])MetroSoft.HIS.cFile.ReadBinaryFile(realFile);
            string fileBase64 = Convert.ToBase64String(rptbyte);

            string fileName = finfo.Name;
            string fileExt = Utils.Mid(finfo.Extension, 2, finfo.Extension.Length);
            string fileSize = Utils.ConvertToString(finfo.Length);
            // File Info

            return CallMTRFileuploadGetSeq(security, fileName, fileExt, fileSize, fileBase64, fileConstSeq, fileSeq, fileNo, idxNo);
        }

        public static string CallMTRFileuploadGetSeq(YlwSecurityJson security, FileInfo finfo, string fileBase64, string fileConstSeq, string fileSeq = "0", string fileNo = "0", string idxNo = "0")
        {
            // File Info
            string fileName = finfo.Name;
            string fileExt = Utils.Mid(finfo.Extension, 2, finfo.Extension.Length);
            string fileSize = Utils.ConvertToString(finfo.Length);
            // File Info

            return CallMTRFileuploadGetSeq(security, fileName, fileExt, fileSize, fileBase64, fileConstSeq, fileSeq, fileNo, idxNo);
        }

        public static string CallMTRFileuploadGetSeq(YlwSecurityJson security, string fileName, string fileExt, string fileSize, string fileBase64, string fileConstSeq, string fileSeq = "0", string fileNo = "0", string idxNo = "0")
        {
            try
            {
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

                ChannelFactory<IMyContract> factory = new ChannelFactory<IMyContract>();

                // Address
                factory.Endpoint.Address = new EndpointAddress(ApiDomainPost);

                // Binding : HTTP 사용
                WebHttpBinding binding = new WebHttpBinding();
                binding.MaxReceivedMessageSize = 2147483647;
                binding.MaxBufferSize = 2147483647;
                binding.MaxBufferPoolSize = 2147483647;
                factory.Endpoint.Binding = binding;

                // Contract 설정
                factory.Endpoint.Contract.ContractType = typeof(IMyContract);
                factory.Endpoint.Behaviors.Add(new WebHttpBehavior());

                // Channel Factory 만들기
                IMyContract channel = factory.CreateChannel();

                // Server 쪽 함수 호출
                string result = channel.FileuploadGetSeq(security, ds);

                // Close Channel
                ((ICommunicationObject)channel).Close();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataSet CallMTRFileDownload(YlwSecurityJson security, string attachFileSeq, string attachFileNo, string isRepFile)
        {
            try
            {
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

                ChannelFactory<IMyContract> factory = new ChannelFactory<IMyContract>();

                // Address
                factory.Endpoint.Address = new EndpointAddress(ApiDomainPost);

                // Binding : HTTP 사용
                WebHttpBinding binding = new WebHttpBinding();
                binding.MaxReceivedMessageSize = 2147483647;
                binding.MaxBufferSize = 2147483647;
                binding.MaxBufferPoolSize = 2147483647;
                factory.Endpoint.Binding = binding;

                // Contract 설정
                factory.Endpoint.Contract.ContractType = typeof(IMyContract);
                factory.Endpoint.Behaviors.Add(new WebHttpBehavior());

                // Channel Factory 만들기
                IMyContract channel = factory.CreateChannel();

                // Server 쪽 함수 호출
                DataSet result = channel.FileDownload(security, ds);
                if (result != null && result.Tables.Contains("ErrorMessage"))
                {
                    DataTable dtr = result.Tables["ErrorMessage"];
                    string msg = "Error : Unknown";
                    if (dtr.Columns.Contains("Message")) msg = dtr.Rows[0]["Message"] + "";
                    throw new Exception(msg);
                }

                // Close Channel
                ((ICommunicationObject)channel).Close();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string CallMTRFileDownloadBase64(YlwSecurityJson security, string attachFileSeq, string attachFileNo, string isRepFile)
        {
            try
            {
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

                ChannelFactory<IMyContract> factory = new ChannelFactory<IMyContract>();

                // Address
                factory.Endpoint.Address = new EndpointAddress(ApiDomainPost);

                // Binding : HTTP 사용
                WebHttpBinding binding = new WebHttpBinding();
                binding.MaxReceivedMessageSize = 2147483647;
                binding.MaxBufferSize = 2147483647;
                binding.MaxBufferPoolSize = 2147483647;
                factory.Endpoint.Binding = binding;

                // Contract 설정
                factory.Endpoint.Contract.ContractType = typeof(IMyContract);
                factory.Endpoint.Behaviors.Add(new WebHttpBehavior());

                // Channel Factory 만들기
                IMyContract channel = factory.CreateChannel();

                // Server 쪽 함수 호출
                string result = channel.FileDownloadBase64(security, ds);

                // Close Channel
                ((ICommunicationObject)channel).Close();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string CallMTRFileDelete(YlwSecurityJson security, string attachFileSeq, string attachFileNo)
        {
            try
            {
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

                ChannelFactory<IMyContract> factory = new ChannelFactory<IMyContract>();

                // Address
                factory.Endpoint.Address = new EndpointAddress(ApiDomainPost);

                // Binding : HTTP 사용
                WebHttpBinding binding = new WebHttpBinding();
                binding.MaxReceivedMessageSize = 2147483647;
                binding.MaxBufferSize = 2147483647;
                binding.MaxBufferPoolSize = 2147483647;
                factory.Endpoint.Binding = binding;

                // Contract 설정
                factory.Endpoint.Contract.ContractType = typeof(IMyContract);
                factory.Endpoint.Behaviors.Add(new WebHttpBehavior());

                // Channel Factory 만들기
                IMyContract channel = factory.CreateChannel();

                // Server 쪽 함수 호출
                string result = channel.FileDelete(security, ds);

                // Close Channel
                ((ICommunicationObject)channel).Close();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static ReportData CallMTRGetReportPost(string streamdata)
        {
            try
            { 
                ChannelFactory<IMyContract> factory = new ChannelFactory<IMyContract>();

                // Address
                factory.Endpoint.Address = new EndpointAddress(ApiDomainPost);

                // Binding : HTTP 사용
                WebHttpBinding binding = new WebHttpBinding();
                binding.MaxReceivedMessageSize = 2147483647;
                binding.MaxBufferSize = 2147483647;
                binding.MaxBufferPoolSize = 2147483647;
                factory.Endpoint.Binding = binding;

                // Contract 설정
                factory.Endpoint.Contract.ContractType = typeof(IMyContract);
                factory.Endpoint.Behaviors.Add(new WebHttpBehavior());

                // Channel Factory 만들기
                IMyContract channel = factory.CreateChannel();

                // Server 쪽 함수 호출
                ReportData result = channel.GetReportPost(streamdata);

                // Close Channel
                ((ICommunicationObject)channel).Close();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static ReportData CallMTRGetSaveReportPost(string streamdata)
        {
            try
            {
                ChannelFactory<IMyContract> factory = new ChannelFactory<IMyContract>();

                // Address
                factory.Endpoint.Address = new EndpointAddress(ApiDomainPost);

                // Binding : HTTP 사용
                WebHttpBinding binding = new WebHttpBinding();
                binding.MaxReceivedMessageSize = 2147483647;
                binding.MaxBufferSize = 2147483647;
                binding.MaxBufferPoolSize = 2147483647;
                factory.Endpoint.Binding = binding;

                // Contract 설정
                factory.Endpoint.Contract.ContractType = typeof(IMyContract);
                factory.Endpoint.Behaviors.Add(new WebHttpBehavior());

                // Channel Factory 만들기
                IMyContract channel = factory.CreateChannel();

                // Server 쪽 함수 호출
                ReportData result = channel.GetSaveReportPost(streamdata);

                // Close Channel
                ((ICommunicationObject)channel).Close();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static DataSet CallMTRSaveReportHistory(string streamdata)
        {
            try
            {
                ChannelFactory<IMyContract> factory = new ChannelFactory<IMyContract>();

                // Address
                factory.Endpoint.Address = new EndpointAddress(ApiDomainPost);

                // Binding : HTTP 사용
                WebHttpBinding binding = new WebHttpBinding();
                binding.MaxReceivedMessageSize = 2147483647;
                binding.MaxBufferSize = 2147483647;
                binding.MaxBufferPoolSize = 2147483647;
                factory.Endpoint.Binding = binding;

                // Contract 설정
                factory.Endpoint.Contract.ContractType = typeof(IMyContract);
                factory.Endpoint.Behaviors.Add(new WebHttpBehavior());

                // Channel Factory 만들기
                IMyContract channel = factory.CreateChannel();

                // Server 쪽 함수 호출
                DataSet result = channel.SaveReportHistory(streamdata);

                // Close Channel
                ((ICommunicationObject)channel).Close();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static ReportData CallMTRGetSaveRptHistoryPost(string streamdata)
        {
            try
            {
                ChannelFactory<IMyContract> factory = new ChannelFactory<IMyContract>();

                // Address
                factory.Endpoint.Address = new EndpointAddress(ApiDomainPost);

                // Binding : HTTP 사용
                WebHttpBinding binding = new WebHttpBinding();
                binding.MaxReceivedMessageSize = 2147483647;
                binding.MaxBufferSize = 2147483647;
                binding.MaxBufferPoolSize = 2147483647;
                factory.Endpoint.Binding = binding;

                // Contract 설정
                factory.Endpoint.Contract.ContractType = typeof(IMyContract);
                factory.Endpoint.Behaviors.Add(new WebHttpBehavior());

                // Channel Factory 만들기
                IMyContract channel = factory.CreateChannel();

                // Server 쪽 함수 호출
                ReportData result = channel.GetSaveRptHistoryPost(streamdata);

                // Close Channel
                ((ICommunicationObject)channel).Close();

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Server 쪽 함수 호출용 Interface
        [ServiceContract]
        public interface IMyContract
        {
            [OperationContract]
            [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/ServiceCall")]
            DataSet ServiceCall(YlwSecurityJson security, DataSet ds);

            [OperationContract]
            [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/ServiceCallPost")]
            DataSet ServiceCallPost(YlwSecurityJson security, DataSet ds);

            [OperationContract]
            [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/GetDataSetPost")]
            DataSet GetDataSetPost(int companyseq, string streamdata);

            [OperationContract]
            [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/Fileupload")]
            DataSet Fileupload(YlwSecurityJson security, DataSet ds);

            [OperationContract]
            [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/FileuploadGetSeq")]
            string FileuploadGetSeq(YlwSecurityJson security, DataSet ds);

            [OperationContract]
            [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/FileDownload")]
            DataSet FileDownload(YlwSecurityJson security, DataSet ds);

            [OperationContract]
            [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/FileDownloadBase64")]
            string FileDownloadBase64(YlwSecurityJson security, DataSet ds);

            [OperationContract]
            [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/FileDelete")]
            string FileDelete(YlwSecurityJson security, DataSet ds);

            [OperationContract]
            [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/GetReportPost")]
            ReportData GetReportPost(string streamdata);

            [OperationContract]
            [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/GetSaveReportPost")]
            ReportData GetSaveReportPost(string streamdata);

            [OperationContract]
            [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/SaveReportHistory")]
            DataSet SaveReportHistory(string streamdata);

            [OperationContract]
            [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Wrapped, UriTemplate = "/GetSaveRptHistoryPost")]
            ReportData GetSaveRptHistoryPost(string streamdata);
        }
    }
}
