using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AspNetCore.Authentication.SK.SmartID.SmartID.Rest.Dao
{
    public class AuthenticationRequest
    {
        public AuthenticationRequest(string relyingPartyUuid, string relyingPartyName, string hash, string hashType,
            List<AllowedInteractionsOrder> allowedInteractionsOrder)
        {
            RelyingPartyUuid = relyingPartyUuid;
            RelyingPartyName = relyingPartyName;
            Hash = hash;
            HashType = hashType;
            AllowedInteractionsOrder = allowedInteractionsOrder;
        }

        [JsonPropertyName("relyingPartyUUID")]
        [Required]
        public string RelyingPartyUuid { get; }

        [Required] public string RelyingPartyName { get; }

        public string CertificateLevel { get; set; }

        [Required] public string Hash { get; }

        [Required] public string HashType { get; }

        [Required] public List<AllowedInteractionsOrder> AllowedInteractionsOrder { get; }

        public string Nonce { get; set; }

        public RequestProperties RequestProperties { get; set; }

        public List<string> Capabilities { get; set; }
    }

    public class AllowedInteractionsOrder
    {
        public string Type { get; set; }

        public string DisplayText60 { get; set; }

        public string DisplayText200 { get; set; }
    }

    public class RequestProperties
    {
    }
}