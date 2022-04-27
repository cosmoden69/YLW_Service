/*
 * Created by SharpDevelop.
 * User: hiworld
 * Date: 2011-07-09
 * Time: 오전 8:43
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Runtime.InteropServices; 

namespace MetroSoft.HIS
{
	/// <summary>
	/// Description of MicFile.
	/// </summary>
	public class cFile
	{
        private string mIniFileName;

        public string IniFileName
        {
            get {return mIniFileName;}
            set {mIniFileName = value;}
        }

		public cFile()
		{
		}

        public cFile(string FileName)
        {
            this.mIniFileName = FileName;
        }

        [DllImport("kernel32")] 
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
		[DllImport("shell32.dll")]
		private static extern bool SHGetSpecialFolderPath(IntPtr hwndOwner, [Out]StringBuilder lpszPath, int nFolder, bool fCreate);

		private static string GetSystemDirectory()
		{
		    StringBuilder path = new StringBuilder(260);
		    SHGetSpecialFolderPath(IntPtr.Zero,path,0x0029,false);
		    return path.ToString();
		}
		
        // INI 값 읽기
        public static string INIRead(string session, string keyValue)
        {
            StringBuilder S = new StringBuilder(1024);
            //string file = System.Environment.SystemDirectory + @"\MetroClient.ini";  //system32 폴더가 반환됨. 64비트에도!!
            string file = GetSystemDirectory() + @"\MetroClient.ini";
            int i = GetPrivateProfileString("MetroHis", "INIFILEPATH", "", S, S.Capacity, file);
            string INIFile = S + @"\MetroHIS.ini";
            i = GetPrivateProfileString(session, keyValue, "", S, 1024, INIFile);
            return S.ToString();
        }

        public static string GetIniValue(String Section, String Key, String iniPath) 
        {
            StringBuilder temp = new StringBuilder(1024);
            int i = GetPrivateProfileString(Section, Key, "", temp, 1024, iniPath);
            return temp.ToString(); 
        }

        public string GetIniValue(String Section, String Key) 
        {
            StringBuilder temp = new StringBuilder(1024);
            int i = GetPrivateProfileString(Section, Key, "", temp, 1024, mIniFileName);
            return temp.ToString(); 
        }

        // INI 값 설정 
        public static void SetIniValue(String Section, String Key, String Value, String iniPath)
        {
            WritePrivateProfileString(Section, Key, Value, iniPath); 
        }

        public void SetIniValue(String Section, String Key, String Value)
        {
            WritePrivateProfileString(Section, Key, Value, mIniFileName); 
        }

        public static bool FileExists(string file)
        {
			return File.Exists(file);
        }

        public static bool DeleteFile(string file)
        {
            try
            {
                File.Delete(file);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void FileCopy(string srcFile, string destFile)
        {
            if (FileExists(srcFile)) File.Copy(srcFile, destFile);
        }

        public static void FileCopy(string srcFile, string destFile, bool overwrite)
        {
            if (FileExists(srcFile)) File.Copy(srcFile, destFile, overwrite);
        }

        public static void MoveFile(string srcFile, string destFile)
        {
            File.Move(srcFile, destFile);
        }

        public static string GetFileName(string file)
        {
            return Path.GetFileName(file);
        }

        public static string GetExtension(string file)
        {
            return Path.GetExtension(file);
        }

        public static string GetFileNameWithoutExtension(string file)
        {
            return Path.GetFileNameWithoutExtension(file);
        }

        public static string GetDirectoryName(string file)
        {
            return Path.GetDirectoryName(file);
        }

        public static string GetTempPath()
        {
            return Path.GetTempPath();
        }

        public static string GetTempFileName()
        {
            return Path.GetTempFileName();
        }

        public static object ReadBinaryFile(string file)
        {
            try
            {
                if (!FileExists(file)) return DBNull.Value;

                FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read);
                BinaryReader r = new BinaryReader(fs);
                Byte[] bytBLOBData = new Byte[fs.Length];
                bytBLOBData = r.ReadBytes((int)fs.Length);
                r.Close();
                fs.Close();
                return bytBLOBData;
            }
            catch
            {
                return DBNull.Value;
            }
        }

        public static bool WriteBinaryFile(string file, object value)
        {
            try
            {
                if (File.Exists(file)) File.Delete(file);

                FileStream fs = new FileStream(file, FileMode.CreateNew);
                BinaryWriter w = new BinaryWriter(fs);
                Byte[] byteBLOBData = new Byte[0];
                byteBLOBData = (Byte[])(value);
                w.Write(byteBLOBData);
                w.Close();
                fs.Close();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static object GetSetting(String subKeyname, String keyname, object defaultvalue)
        {
            return GetSetting(Registry.LocalMachine, subKeyname, keyname, defaultvalue);
        }

        public static object GetSetting(RegistryKey rkey, String subKeyname, String keyname, object defaultvalue)
        {
            RegistryKey rkeys = rkey.OpenSubKey(subKeyname, true);
            if (rkeys == null)
            {
                rkeys = rkey.CreateSubKey(subKeyname);
            }
            object rtn = rkeys.GetValue(keyname, defaultvalue);
            rkeys.Close();
            return rtn;
        }

        public static void SaveSetting(String subKeyname, String keyname, object value)
        {
            SaveSetting(Registry.LocalMachine, subKeyname, keyname, value);
        }

        public static void SaveSetting(RegistryKey rkey, String subKeyname, String keyname, object value)
        {
            RegistryKey rkeys = rkey.OpenSubKey(subKeyname, true);
            if (rkeys == null)
            {
                rkeys = rkey.CreateSubKey(subKeyname);
            }
            rkeys.SetValue(keyname, value);
            rkeys.Close();
        }

        #region Get
        public static string GetValue(RegistryKey rootKey, string keyPath, string valueName, string defaultValue)
        {
            try
            {
                RegistryKey reg = rootKey.OpenSubKey(keyPath, false);

                if (reg != null)
                {
                    object value = reg.GetValue(valueName);
                    if (value != null)
                    {
                        return cConvert.ToString(value);
                    }
                }

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static int GetValue(RegistryKey rootKey, string keyPath, string valueName, int defaultValue)
        {
            string valueStr = GetValue(rootKey, keyPath, valueName, cConvert.ToString(defaultValue));

            int result = 0;
            if (int.TryParse(valueStr, out result))
                return result;

            return defaultValue;
        }

        public static float GetValue(RegistryKey rootKey, string keyPath, string valueName, float defaultValue)
        {
            string valueStr = GetValue(rootKey, keyPath, valueName, cConvert.ToString(defaultValue));

            float result = 0;

            if (float.TryParse(valueStr, out result))
                return result;

            return defaultValue;
        }

        public static double GetValue(RegistryKey rootKey, string keyPath, string valueName, double defaultValue)
        {
            string valueStr = GetValue(rootKey, keyPath, valueName, cConvert.ToString(defaultValue));

            double result = 0;

            if (double.TryParse(valueStr, out result))
                return result;

            return defaultValue;
        }

        public static bool GetValue(RegistryKey rootKey, string keyPath, string valueName, bool defaultValue)
        {
            string valueStr = GetValue(rootKey, keyPath, valueName, cConvert.ToString(defaultValue));

            bool result = false;

            if (bool.TryParse(valueStr, out result))
                return result;

            return defaultValue;
        }

        public static DateTime GetValue(RegistryKey rootKey, string keyPath, string valueName, DateTime defaultValue)
        {
            string valueStr = GetValue(rootKey, keyPath, valueName, cConvert.ToString(defaultValue));

            DateTime result = DateTime.MinValue;

            if (DateTime.TryParse(valueStr, out result))
                return result;

            return defaultValue;
        }
        #endregion

        #region Set

        public static bool SetValue(RegistryKey rootkey, string keyPath, string valueName, object value)
        {
            try
            {
                RegistryKey reg = rootkey.OpenSubKey(keyPath, true);

                if (reg == null)
                {
                    rootkey.CreateSubKey(keyPath);
                    reg = rootkey.OpenSubKey(keyPath, true);
                }

                reg.SetValue(valueName, value);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region Delete
        public static bool DeleteValue(RegistryKey rootkey, string keyPath, string valueName)
        {
            try
            {
                RegistryKey reg = rootkey.OpenSubKey(keyPath, true);

                if (reg == null)
                {
                    return true;
                }

                reg.DeleteValue(valueName);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        public static void WriteLog(string prjKey, string msg)
        {
            try
            {
                // Write the string to a file.
                string str = DateTime.Now.ToString("yyyy/MM/dd hh:mm:ss") + " > " + msg;
                string mypath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                System.IO.StreamWriter file = new System.IO.StreamWriter(mypath + @"\MetroSoft.HIS.Log_" + DateTime.Now.ToString("yyyyMM") + ".txt", true);
                file.WriteLine(str);
                file.Close();
            }
            catch { }
        }
	}
}
