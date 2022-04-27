/*
 * Created by SharpDevelop.
 * User: hiworld
 * Date: 2011-07-31
 * Time: 오전 10:01
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace MetroSoft.HIS
{
	public class BindVar
	{
        private string varName = string.Empty;
        private object varValue = null;

        public string VarName
        {
            get { return varName; }
        }
        public object VarValue
        {
            get { return varValue; }
            set { varValue = value; }
        }

        public BindVar(string varName) : this(varName, null) { }

        public BindVar(string varName, object varValue)
        {
            this.varName = varName;
            this.varValue = varValue;
        }
    }

	public class BindVarCollection : System.Collections.CollectionBase
	{
		public BindVar this[int index]
		{
			get
			{
				if ((index < 0) || (index >= List.Count)) return null;
				return (BindVar) List[index];
			}
		}
		
		public BindVar this[string key]
		{
			get 
			{
				if (key == "") return null;
				foreach (BindVar var in List)
					if (var.VarName == key)
						return var;
				return null;
			}
		}

        public void Add(string varName)
        {
            Add(varName, string.Empty);
        }

        public void Add(string varName, object varValue)
        {
            bool isFound = false;
            //이미 있으면 Value만 다시 설정
            foreach (BindVar item in List)
            {
                if (item.VarName == varName)
                {
                    isFound = true;
                    item.VarValue = varValue;
                    break;
                }
            }
            if (!isFound)
            {
                BindVar var = new BindVar(varName, varValue);
                List.Add(var);
            }
        }
		
		public void Remove(string varName)
		{
			int index = 0;
			bool isFound = false;
			foreach (BindVar item in List)
			{
				if (item.VarName == varName)
				{
					isFound = true;
					break;
				}
				index++;
			}
			if (isFound)
				this.RemoveAt(index);
		}
		
		public bool Contains(string varName)
		{
			foreach (BindVar item in List)
				if (item.VarName == varName)
					return true;
			return false;
		}
	}
}
