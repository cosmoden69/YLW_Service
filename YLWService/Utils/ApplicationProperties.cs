using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

using MetroSoft.HIS.Extensions;

namespace MetroSoft.HIS
{
    internal class ApplicationProperty
    {
        public string Category { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    internal class ApplicationPropertyCategory
    {
        private List<ApplicationProperty> _list;

        public string Name { get; set; }

        public ApplicationPropertyCategory()
        {
            _list = new List<ApplicationProperty>();
        }

        public void InitProperties(XmlNode categoryNode)
        {
            if (categoryNode != null)
            {
                List<XmlNode> nodeList = categoryNode.SelectNodes("property", true);

                if (nodeList.Count > 0)
                {
                    foreach (XmlNode node in nodeList)
                    {
                        ApplicationProperty property = new ApplicationProperty();

                        property.Category = this.Name;
                        //property.Name = node.GetAttribute(MapperConstants.Id).InnerText;
                        property.Value = node.InnerText;

                        _list.Add(property);
                    }
                }
            }
        }

        public ApplicationProperty FindProperty(string propertyName)
        {
            foreach (ApplicationProperty prop in _list)
            {
                if (prop.Name == propertyName)
                    return prop;
            }

            return null;
        }
    }

    internal class ApplicationProperties
    {
        private static List<ApplicationPropertyCategory> _categories = new List<ApplicationPropertyCategory>();
        private static XmlDocument _xml = new XmlDocument();
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;

            StringReader reader = null;
            XmlReader xmlReader = null;

            try
            {
                string resData = string.Empty;

                using (StreamReader strReader = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MetroSoft.HIS.config")))
                {
                    resData = strReader.ReadToEnd();
                }

                reader = new StringReader(resData);
                xmlReader = new XmlTextReader(reader);
                _xml.Load(xmlReader);

                XmlNode root = _xml.LastChild;

                if (root != null)
                {
                    List<XmlNode> nodeList = root.SelectNodes("applicationProperties", true);

                    if (nodeList.Count > 0)
                    {
                        List<XmlNode> cateList = nodeList[0].SelectNodes("category", true);

                        if (cateList.Count > 0)
                        {
                            foreach (XmlNode node in cateList)
                            {
                                ApplicationPropertyCategory category = new ApplicationPropertyCategory();
                                //category.Name = node.GetAttribute(MapperConstants.Id).InnerText;
                                category.InitProperties(node);

                                _categories.Add(category);
                            }
                        }

                    }
                }
                _initialized = true;
            }
            catch 
            {
                throw;
            }
            finally
            {
                if (reader != null)
                    reader.Close();

                if (xmlReader != null)
                    xmlReader.Close();
            }
        }

        private static ApplicationPropertyCategory FindCategory(string category)
        {
            foreach(ApplicationPropertyCategory cate in _categories)
            {
                if (cate.Name == category)
                    return cate;
            }

            return null;
        }

        private static ApplicationProperty GetPropertyInstance(string categoryName, string propertyName)
        {
            Initialize();

            ApplicationProperty result = null;

            ApplicationPropertyCategory cate = FindCategory(categoryName);

            if (cate != null)
            {
                ApplicationProperty prop = cate.FindProperty(propertyName);
                result = prop;
            }

            return result;
        }

        public static int GetProperty(string categoryName, string propertyName, int defaultValue)
        {
            int result = defaultValue;
            ApplicationProperty prop = GetPropertyInstance(categoryName, propertyName);

            if (prop != null)
            {
                if (!int.TryParse(prop.Value, out result))
                    result = defaultValue;
            }
            return result;
        }

        public static float GetProperty(string categoryName, string propertyName, float defaultValue)
        {
            float result = defaultValue;
            ApplicationProperty prop = GetPropertyInstance(categoryName, propertyName);

            if (prop != null)
            {
                if (!float.TryParse(prop.Value, out result))
                    result = defaultValue;
            }
            return result;
        }

        public static bool GetProperty(string categoryName, string propertyName, bool defaultValue)
        {
            bool result = defaultValue;
            ApplicationProperty prop = GetPropertyInstance(categoryName, propertyName);

            if (prop != null)
            {
                if (!bool.TryParse(prop.Value, out result))
                    result = defaultValue;
            }
            return result;
        }

        public static string GetProperty(string categoryName, string propertyName, string defaultValue)
        {
            string result = defaultValue;
            ApplicationProperty prop = GetPropertyInstance(categoryName, propertyName);

            if (prop != null)
            {
                result = Convert.ToString(prop.Value);
            }
            return result;
        }
    }
}
