using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroSoft.HIS
{
    internal class CredentialInfo
    {
        private string _credentialId;
        private string _credentialKey;

        public string CredentialId
        {
            get { return _credentialId; }
        }

        public string CredentialKey
        {
            get { return _credentialKey; }
        }

        public CredentialInfo(string credentialId, string credentialKey)
        {
            this._credentialId = credentialId;
            this._credentialKey = credentialKey;
        }
    }
}
