using System.Collections.Generic;

namespace AspNetCore.Authentication.SK.SmartId.SmartId.Rest.Dao
{
    public class SessionStatus
    {
        public string State { get; set; }

        public SessionStatusResult Result { get; set; }

        public SessionStatusSignature Signature { get; set; }

        public SessionStatusCertificate Cert { get; set; }

        public List<string> IgnoredProperties { get; set; }
    }
}