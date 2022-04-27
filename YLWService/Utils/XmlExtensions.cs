using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MetroSoft.HIS.Extensions
{
    public static class XmlExtensions
    {
        /// <summary>
        /// XmlNode에 있는 특성값을 찾습니다. 대소문자 비교 옵션을 사용할 수 있는 확장 함수입니다.
        /// </summary>        
        /// <param name="name">찾고자 하는 특성값의 이름입니다.</param>
        /// <param name="ignoreCase">대소문자 구분을 하여 비교할 지 여부입니다.</param>
        /// <returns>해당 이름의 특성을 리턴합니다. 찾지 못한 경우 null을 리턴합니다.</returns>
        public static XmlAttribute GetAttribute(this XmlNode node, string name, bool ignoreCase = true)
        {
            foreach (XmlAttribute attr in node.Attributes)
            {
                if (ignoreCase)
                {
                    if (String.Compare(attr.Name, name, true) == 0)
                        return attr;
                }
                else
                {
                    if (attr.Name.Equals(name))
                        return attr;
                }
            }

            return null;
        }

        /// <summary>
        /// XmlNode의 자식 노드들 중 특정 이름을 가지는 자식 노들의 리스트를 얻는 확장 함수입니다.
        /// </summary>        
        /// <param name="nodeName">찾고자 하는 자식 노드의 이름입니다.</param>
        /// <param name="ignoreCase">대소문자 구분을 하여 찾을지 여부입니다.</param>
        /// <returns>지정한 이름을 가진 자식 XmlNode의 리스트를 리턴합니다. 특정 이름으로 된 자식 노드를 찾지 못한 경우 null을 리턴합니다.</returns>
        public static List<XmlNode> SelectNodes(this XmlNode node, string nodeName, bool ignoreCase)
        {
            List<XmlNode> list = new List<XmlNode>();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (String.Compare(child.Name, nodeName, ignoreCase) == 0)
                    list.Add(child);
            }

            if (list.Count == 0)
            {
                list.Clear();
                list = null;
            }

            return list;
        }
    }
}
