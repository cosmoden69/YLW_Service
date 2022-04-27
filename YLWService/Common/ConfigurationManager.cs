using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MetroSoft.HIS
{
    class ConfigurationManager
    {
        private static ConfigurationManager _manager;
        private static string _defaultConfigFileName = "MetroSoft.HIS.Config";  //환경파일
        
        public static ConfigurationManager Global
        {
            get
            {
                if (_manager == null)
                    _manager = new ConfigurationManager(_defaultConfigFileName);

                return _manager;
            }
        }

        private string _fileName = string.Empty;
        private XmlDocument configXml = new XmlDocument();  //config File Xml 관리 Document
        private Encoding baseEncoding = Encoding.GetEncoding("ks_c_5601-1987");

        public XmlNode this[string configName]
        {
            get
            {
                return GetConfigXml(configName);
            }
        }
        
        private ConfigurationManager(string configFileName)
        {
            _fileName = configFileName;

            TextReader textReader = null;
            XmlReader xmlReader = null;

            try
            {
                string inPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (inPath.Length == 0)
                {
                    XMessageBox.Warning("Cannot locate current executing program.");
#if DEBUG             
                    inPath = AppDomain.CurrentDomain.BaseDirectory;
#else               
                    return;
#endif
                }
                inPath = Path.GetDirectoryName(inPath);
                string fileFullPath = Path.Combine(inPath, configFileName);
                
                if (!File.Exists(fileFullPath))
                {
                    XMessageBox.Warning("MetroSoft.HIS Config file(MetroSoft.HIS.Config) does not exist");
                    return;
                }

                textReader = new StreamReader(fileFullPath);
                xmlReader = new XmlTextReader(textReader);

                configXml.Load(xmlReader);

                textReader.Close();
                xmlReader.Close();                
            }
            catch(Exception ex)
            {
                XMessageBox.Warning("Error occured during load MetroSoft.HIS.Config file. [" + ex.Message + "]");
                return;
            }
            finally
            {
                if (textReader != null)
                {
                    textReader.Close();
                    textReader.Dispose();
                }

                if (xmlReader != null)
                {
                    xmlReader.Close();
                    xmlReader.Dispose();
                }
            }
        }

        public XmlNode GetConfigXml(string configName)
        {
            
            if (configXml != null)
            {
                XmlNodeList elem = configXml.DocumentElement.GetElementsByTagName(configName);

                if (elem != null && elem.Count > 0)
                    return elem[0];

            }           
            return null;
        }

        internal void SetConfigStringNode(XmlNode rootConfig, string elementName, string attributeName, string setValue)
        {
            try
            {
                XmlNode sNode = null;
                foreach (XmlNode node in rootConfig.ChildNodes)
                {
                    if (node.Name == elementName)
                    {
                        if (node.Attributes[attributeName] != null)
                            node.Attributes[attributeName].Value = setValue;
                        else
                            ((XmlElement)node).SetAttribute(attributeName, setValue);
                        sNode = node;
                        break;
                    }
                }
                //Node가 없으면 Set
                if (sNode == null)
                {
                    XmlElement element = configXml.CreateElement(elementName);
                    element.SetAttribute(attributeName, setValue);
                    configXml.DocumentElement.AppendChild(element);
                }

                //Save Config File
                string inPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                if (inPath.Length == 0)
                {
                    XMessageBox.Warning("Cannot locate current executing program.");
#if DEBUG             
                    inPath = AppDomain.CurrentDomain.BaseDirectory;
#else               
                    return;
#endif
                }

                inPath = Path.GetDirectoryName(inPath);
                string fileFullPath = Path.Combine(inPath, _fileName);

                using (TextWriter tw = new StreamWriter(fileFullPath, false, baseEncoding))
                    configXml.Save(tw);
            }
            catch { }
        }
    }
}
