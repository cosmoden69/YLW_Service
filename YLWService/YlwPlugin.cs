using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YLWService
{
    public class YlwPlugin
    {
        private static string dsn_bis = "server=20.194.52.25,14233;database=HASP;uid=ace;pwd=asdf4$sza123";
        //private static string dsn_bis = "server=192.168.1.145,14233;database=DEVMETRO;uid=cosmoden@metrosoft.co.kr;pwd=1";

        private string GetRealFilePath(ClsConnInfo conninfo, string attachFileConstSeq)
        {
            try
            {
                DataSet dsData_result = new DataSet();
                //로컬 개발 시 사용
                if (string.IsNullOrEmpty(conninfo.Dsn) || string.IsNullOrEmpty(conninfo.DsnBis))
                {
                    SqlConnection conn = new SqlConnection(dsn_bis);
                    conn.Open();
                    string sql = "_SAdjSLAttachFileConstQuery N'" + attachFileConstSeq + "'";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    adapter.Fill(dsData_result, "DataBlock1");
                    conn.Close();
                }
                //서버에서 사용
                else
                {
                    YLWService.YlwSecurityJson security = YLWService.YLWServiceModule.SecurityJson.Clone();  //깊은복사
                    security.serviceId = "Metro.Package.AdjSL.BisFileUpAndDown";
                    security.methodId = "GetRealFilePath";
                    security.companySeq = conninfo.security.companySeq;

                    DataSet pds = new DataSet("ROOT");
                    DataTable dt = pds.Tables.Add("DataBlock1");
                    dt.Columns.Add("AttachFileConstSeq");
                    dt.Rows.Add(new string[] { attachFileConstSeq });
                    dsData_result = YLWService.YLWServiceModule.CallYlwServiceCallPost(security, pds);
                }
                if (dsData_result == null || dsData_result.Tables.Count < 1) return "";
                DataTable db1 = dsData_result.Tables[0];
                if (db1.Rows.Count < 1) return "";
                return db1.Rows[0]["RootPath"].ToString();
            }
            catch
            {
                return "";
            }
        }

        private DataTable GetAttachFileInfo(ClsConnInfo conninfo, string attachFileSeq)
        {
            try
            {
                DataSet dsData_result = new DataSet();
                //로컬 개발 시 사용
                if (string.IsNullOrEmpty(conninfo.Dsn) || string.IsNullOrEmpty(conninfo.DsnBis))
                {
                    SqlConnection conn = new SqlConnection(dsn_bis);
                    conn.Open();
                    string sql = "_SAdjSLAttachFileQuery N'" + attachFileSeq + "'";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    adapter.Fill(dsData_result, "DataBlock1");
                    conn.Close();
                }
                //서버에서 사용
                else
                {
                    YLWService.YlwSecurityJson security = YLWService.YLWServiceModule.SecurityJson.Clone();  //깊은복사
                    security.serviceId = "Metro.Package.AdjSL.BisFileUpAndDown";
                    security.methodId = "GetAttachFileInfo";
                    security.companySeq = conninfo.security.companySeq;

                    DataSet pds = new DataSet("ROOT");
                    DataTable dt = pds.Tables.Add("DataBlock1");
                    dt.Columns.Add("attachFileSeq");
                    dt.Columns.Add("IsRepFile");
                    dt.Rows.Add(new string[] { attachFileSeq, "" });
                    dsData_result = YLWService.YLWServiceModule.CallYlwServiceCallPost(security, pds);
                }
                if (dsData_result == null || dsData_result.Tables.Count < 1) return null;
                return dsData_result.Tables[0];
            }
            catch
            {
                return null;
            }
        }

        public DataSet FileDelete(ClsConnInfo conninfo, DataSet dsData)
        {
            try
            {
                if (dsData == null) return null;
                if (dsData.Tables.Count == 0) return null;
                if (dsData.Tables.Count == 0 || dsData.Tables.Contains("DataBlock1") == false) return null;

                DataSet dsData_result = new DataSet();

                DataTable dtData = dsData.Tables["DataBlock1"];
                string companySeq = dtData.Rows[0]["CompanySeq"].ToString();
                string languageSeq = dtData.Rows[0]["LanguageSeq"].ToString();
                string loginPgmSeq = dtData.Rows[0]["LoginPgmSeq"].ToString();
                string userSeq = dtData.Rows[0]["UserSeq"].ToString();
                string attachFileConstSeq = dtData.Rows[0]["AttachFileConstSeq"].ToString();
                string attachFileSeq = dtData.Rows[0]["AttachFileSeq"].ToString();
                string attachFileNo = dtData.Rows[0]["AttachFileNo"].ToString();

                DataTable dtDel = GetAttachFileInfo(conninfo, attachFileSeq);
                if (dtDel != null && dtDel.Rows.Count > 0)
                {
                    //string path1 = "~" + dtDel.Rows[0]["RealFilePath"].ToString();
                    //string file1 = dtDel.Rows[0]["RealFileName"].ToString();
                    //string realpath1 = HostingEnvironment.MapPath(path1);
                    //string realfile1 = Path.Combine(realpath1, file1);
                    string path1 = dtDel.Rows[0]["RealFilePath"].ToString();
                    string file1 = dtDel.Rows[0]["RealFileName"].ToString();
                    string realfile1 = GetFileDownloadRootPath(conninfo, path1, file1);
                    realfile1 = Path.Combine(realfile1, file1);
                    try
                    {
                        System.IO.File.Delete(realfile1);
                    }
                    catch { }
                }

                XmlDocument xml = new XmlDocument();

                XmlElement root = xml.CreateElement("AttachFileInfo");
                XmlElement elmt = xml.CreateElement("WorkingTag"); elmt.InnerText = "D";
                root.AppendChild(elmt);
                elmt = xml.CreateElement("AttachFileSeq"); elmt.InnerText = attachFileSeq;  //삭제시 필요
                root.AppendChild(elmt);
                elmt = xml.CreateElement("AttachFileNo"); elmt.InnerText = attachFileNo;    //삭제시 필요
                root.AppendChild(elmt);

                //로컬 개발 시 사용
                if (string.IsNullOrEmpty(conninfo.Dsn) || string.IsNullOrEmpty(conninfo.DsnBis))
                {
                    SqlConnection conn = new SqlConnection(dsn_bis);
                    conn.Open();
                    string sql = "_SAdjSLFileDelete N'" + companySeq + "', N'" + languageSeq + "', N'" + loginPgmSeq + "', N'" + userSeq + "', N'" + attachFileConstSeq + "', N'" + root.OuterXml + "'";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    adapter.Fill(dsData_result);
                    conn.Close();
                }
                //서버에서 사용
                else
                {
                    YLWService.YlwSecurityJson security = YLWService.YLWServiceModule.SecurityJson.Clone();  //깊은복사
                    security.serviceId = "Metro.Package.AdjSL.BisFileUpAndDown";
                    security.methodId = "FileDelete";
                    security.companySeq = conninfo.security.companySeq;

                    DataSet pds = new DataSet("ROOT");
                    DataTable dt = pds.Tables.Add("DataBlock1");
                    dt.Columns.Add("AttachFileConstSeq");
                    dt.Columns.Add("xmlFileInfo");
                    dt.Rows.Add(new string[] { attachFileConstSeq, root.OuterXml });
                    dsData_result = YLWService.YLWServiceModule.CallYlwServiceCallPost(security, pds);
                }

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

        public DataSet FileUpload(ClsConnInfo conninfo, DataSet dsData)
        {
            try
            {
                if (dsData == null) return null;
                if (dsData.Tables.Count == 0) return null;
                if (dsData.Tables.Count == 0 || dsData.Tables.Contains("DataBlock1") == false) return null;

                DataSet dsData_result = new DataSet();

                DataTable dtData = dsData.Tables["DataBlock1"];
                string companySeq = dtData.Rows[0]["CompanySeq"].ToString();
                string languageSeq = dtData.Rows[0]["LanguageSeq"].ToString();
                string loginPgmSeq = dtData.Rows[0]["LoginPgmSeq"].ToString();
                string userSeq = dtData.Rows[0]["UserSeq"].ToString();
                string attachFileConstSeq = dtData.Rows[0]["AttachFileConstSeq"].ToString();
                string attachFileSeq = dtData.Rows[0]["AttachFileSeq"].ToString();
                string attachFileNo = dtData.Rows[0]["AttachFileNo"].ToString();
                string IDX_NO = dtData.Rows[0]["IDX_NO"].ToString();
                string isRepFile = dtData.Rows[0]["IsRepFile"].ToString();
                string fileName = dtData.Rows[0]["fileName"].ToString();
                string fileExt = dtData.Rows[0]["fileExt"].ToString();
                string fileSize = dtData.Rows[0]["FileSize"].ToString();
                string remark = dtData.Rows[0]["Remark"].ToString();

                string filePath = GetRealFilePath(conninfo, attachFileConstSeq);
                if (filePath == "") throw new Exception("filePath not found");
                string realFilePath = Path.Combine(filePath, DateTime.Now.ToString("yyyyMM"));

                string fileBase64 = dtData.Rows[0]["fileBase64"].ToString();
                string replaceFileName = fileName.Substring(fileName.LastIndexOf("\\") + 1, fileName.Length - fileName.LastIndexOf("\\") - 1);
                string replaceFileBase64 = fileBase64.Replace("data:application/vnd.openxmlformats-officedocument.wordprocessingml.document;base64,", "");
                string realFileName = Guid.NewGuid().ToString() + "_" + replaceFileName;

                //base64 파일생성
                string fileRealPath = GetFileDownloadRootPath(conninfo, realFilePath, realFileName);
                if (!Directory.Exists(fileRealPath))
                {
                    Directory.CreateDirectory(fileRealPath);
                }
                fileRealPath = Path.Combine(fileRealPath, realFileName);

                byte[] bytes_file = Convert.FromBase64String(replaceFileBase64);
                FileStream orgFile = new FileStream(fileRealPath, FileMode.Create);
                orgFile.Write(bytes_file, 0, bytes_file.Length);
                orgFile.Close();
                //upload directory

                //이전 파일이 있으면 삭제
                if (Convert.ToInt32(attachFileSeq) != 0 && Convert.ToInt32(IDX_NO) == 0)
                {
                    FileDelete(conninfo, dsData);
                    attachFileSeq = "0";
                }

                XmlDocument xml = new XmlDocument();

                XmlElement root = xml.CreateElement("AttachFileInfo");
                XmlElement elmt = xml.CreateElement("WorkingTag"); elmt.InnerText = "A";
                root.AppendChild(elmt);
                elmt = xml.CreateElement("AttachFileSeq"); elmt.InnerText = attachFileSeq;  //삭제시 필요
                root.AppendChild(elmt);
                elmt = xml.CreateElement("AttachFileNo"); elmt.InnerText = attachFileNo;    //삭제시 필요
                root.AppendChild(elmt);
                elmt = xml.CreateElement("IDX_NO"); elmt.InnerText = IDX_NO;                //여러개 등록시 필요
                root.AppendChild(elmt);
                elmt = xml.CreateElement("IsRepFile"); elmt.InnerText = isRepFile;
                root.AppendChild(elmt);
                elmt = xml.CreateElement("FileName"); elmt.InnerText = fileName;
                root.AppendChild(elmt);
                elmt = xml.CreateElement("RealFileName"); elmt.InnerText = realFileName;
                root.AppendChild(elmt);
                elmt = xml.CreateElement("RealFilePath"); elmt.InnerText = realFilePath;
                root.AppendChild(elmt);
                elmt = xml.CreateElement("FileExt"); elmt.InnerText = fileExt;
                root.AppendChild(elmt);
                elmt = xml.CreateElement("FileSize"); elmt.InnerText = fileSize;
                root.AppendChild(elmt);
                elmt = xml.CreateElement("Remark"); elmt.InnerText = remark;
                root.AppendChild(elmt);

                //로컬 개발 시 사용
                if (string.IsNullOrEmpty(conninfo.Dsn) || string.IsNullOrEmpty(conninfo.DsnBis))
                {
                    SqlConnection conn = new SqlConnection(dsn_bis);
                    conn.Open();
                    string sql = "_SAdjSLFileUpload N'" + companySeq + "', N'" + languageSeq + "', N'" + loginPgmSeq + "', N'" + userSeq + "', N'" + attachFileConstSeq + "', N'" + root.OuterXml + "'";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    adapter.Fill(dsData_result);
                    conn.Close();
                }
                //서버에서 사용
                else
                {
                    YLWService.YlwSecurityJson security = YLWService.YLWServiceModule.SecurityJson.Clone();  //깊은복사
                    security.serviceId = "Metro.Package.AdjSL.BisFileUpAndDown";
                    security.methodId = "FileUpload";
                    security.companySeq = conninfo.security.companySeq;

                    DataSet pds = new DataSet("ROOT");
                    DataTable dt = pds.Tables.Add("DataBlock1");
                    dt.Columns.Add("AttachFileConstSeq");
                    dt.Columns.Add("xmlFileInfo");
                    dt.Rows.Add(new string[] { attachFileConstSeq, root.OuterXml });
                    dsData_result = YLWService.YLWServiceModule.CallYlwServiceCallPost(security, pds);
                }

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

        public DataSet FileDownload(ClsConnInfo conninfo, DataSet dsData)
        {
            try
            {
                if (dsData == null) return null;
                if (dsData.Tables.Count == 0 || dsData.Tables.Contains("DataBlock1") == false) return null;

                DataTable dtData = dsData.Tables["DataBlock1"];
                string companySeq = dtData.Rows[0]["CompanySeq"].ToString();
                string languageSeq = dtData.Rows[0]["LanguageSeq"].ToString();
                string userSeq = dtData.Rows[0]["UserSeq"].ToString();
                string workingTag = dtData.Rows[0]["workingTag"].ToString();
                string fileSeq = dtData.Rows[0]["AttachFileSeq"].ToString();
                string fileNo = dtData.Rows[0]["AttachFileNo"].ToString();
                string isRepFile = dtData.Rows[0]["IsRepFile"].ToString();

                DataSet dsData_result = new DataSet();
                //로컬 개발 시 사용
                if (string.IsNullOrEmpty(conninfo.Dsn) || string.IsNullOrEmpty(conninfo.DsnBis))
                {
                    SqlConnection conn = new SqlConnection(dsn_bis);
                    conn.Open();
                    string sql = "_SAdjSLFileDownload N'" + companySeq + "', N'" + languageSeq + "', N'" + userSeq + "', N'" + workingTag + "', N'" + fileSeq + "', N'" + fileNo + "', N'" + isRepFile + "'";
                    SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
                    adapter.Fill(dsData_result, "DataBlock1");
                    conn.Close();
                }
                //서버에서 사용
                else
                {
                    YLWService.YlwSecurityJson security = YLWService.YLWServiceModule.SecurityJson.Clone();  //깊은복사
                    security.serviceId = "Metro.Package.AdjSL.BisFileUpAndDown";
                    security.methodId = "FileDownload";
                    security.companySeq = conninfo.security.companySeq;

                    DataSet pds = new DataSet("ROOT");
                    DataTable dt = pds.Tables.Add("DataBlock1");
                    dt.Columns.Add("AttachFileSeq");
                    dt.Columns.Add("AttachFileNo");
                    dt.Columns.Add("IsRepFile");
                    dt.Clear();
                    DataRow dr = dt.Rows.Add();
                    dr["attachFileSeq"] = fileSeq;
                    dr["AttachFileNo"] = fileNo;
                    dr["IsRepFile"] = isRepFile;

                    dsData_result = YLWService.YLWServiceModule.CallYlwServiceCallPost(security, pds);
                }

                if (dsData_result == null || dsData_result.Tables.Count < 1) throw new Exception("DataBlock1 not returned");
                if (dsData_result.Tables[0].Rows.Count < 1) throw new Exception("DataRow not returned");
                DataTable db1 = dsData_result.Tables[0];

                for (int ii = 0; ii < db1.Rows.Count; ii++)
                {
                    //string filePath = "~" + db1.Rows[ii]["RealFilePath"].ToString();
                    //string fileName = db1.Rows[ii]["FileName"].ToString();
                    //string fileRealPath = HostingEnvironment.MapPath(filePath) + @"\" + db1.Rows[ii]["RealFileName"].ToString();
                    string filePath = db1.Rows[ii]["RealFilePath"].ToString();
                    string fileName = db1.Rows[ii]["RealFileName"].ToString();
                    string fileRealPath = GetFileDownloadRootPath(conninfo, filePath, fileName);
                    fileRealPath = Path.Combine(fileRealPath, fileName);
                    FileStream fs = new FileStream(fileRealPath, FileMode.Open, FileAccess.Read);
                    BinaryReader r = new BinaryReader(fs);
                    Byte[] bytBLOBData = new Byte[fs.Length];
                    bytBLOBData = r.ReadBytes((int)fs.Length);
                    r.Close();
                    fs.Close();
                    string fileBase64 = Convert.ToBase64String(bytBLOBData);
                    if (!db1.Columns.Contains("FileBase64")) db1.Columns.Add("FileBase64");
                    db1.Rows[ii]["FileBase64"] = fileBase64;
                }

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

        private string GetFileDownloadRootPath(ClsConnInfo conninfo, string path1, string file1)
        {
            try
            {
                string realPath = YLWService.YLWServiceModule.ServerFilePath;
                return realPath + path1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class ClsConnInfo
    {
        public YLWService.YlwSecurityJson security { get; set; }
        public string Dsn { get; set; } = "1";
        public string DsnBis { get; set; } = "1";

        public ClsConnInfo(YLWService.YlwSecurityJson sec)
        {
            this.security = sec;
        }
    }
}
