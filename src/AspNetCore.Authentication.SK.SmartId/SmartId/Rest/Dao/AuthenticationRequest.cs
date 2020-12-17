using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AspNetCore.Authentication.SK.SmartId.SmartId.Rest.Dao
{
    public class AuthenticationRequest
    {
        public AuthenticationRequest(string relyingPartyUuid, string relyingPartyName, string hash, string hashType)
        {
            RelyingPartyUuid = relyingPartyUuid;
            RelyingPartyName = relyingPartyName;
            Hash = hash;
            HashType = hashType;
        }

        [JsonPropertyName("relyingPartyUUID")]
        [Required] public string RelyingPartyUuid { get; }

        [Required] public string RelyingPartyName { get; }

        public string CertificateLevel { get; set; }

        [Required] public string Hash { get; }

        [Required] public string HashType { get; }

        public string DisplayText { get; set; }

        public string Nonce { get; set; }
        
        public RequestProperties RequestProperties { get; set; }

        public List<string> Capabilities { get; set; }
    }

    public class RequestProperties
    {
        public bool VcChoice { get; set; }
    }
}