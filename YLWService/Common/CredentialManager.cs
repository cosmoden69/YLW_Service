using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Security;

using MetroSoft.HIS.Utils;

namespace MetroSoft.HIS
{
    internal class CredentialManager
    {
        internal static CredentialInfo GetCredentialInfo()
        {
            string key = ApplicationProperties.GetProperty("server", "credentialKey", "");
            return GetCredentialInfo(key);
        }

        internal static CredentialInfo GetCredentialInfo(string Key)
        {
            CredentialInfo credential = null;

            string credentialId = string.Empty;
            string credentialKey = string.Empty;

            string temp = cEncryptionUtil.DecryptString(Key);

            string[] arr = temp.Split(new string[] { ";" }, StringSplitOptions.None);

            if (arr.Length == 2)
            {
                credential = new CredentialInfo(arr[0], arr[1]);                
            }

            return credential;
        }

        internal static SqlCredential GetCredential()
        {
            return GetCredential(GetCredentialInfo());
        }

        internal static SqlCredential GetCredential(CredentialInfo info)
        {
            return GetCredential(info.CredentialId, info.CredentialKey);
        }

        internal static SqlCredential GetCredential(string id, string passwd)
        {
            SecureString str = new SecureString();
            foreach (char c in passwd)
                str.AppendChar(c);

            str.MakeReadOnly();

            return new SqlCredential(id, str);
        }
    }
}
