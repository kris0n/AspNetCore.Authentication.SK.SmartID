using System.Text.Json.Serialization;

namespace AspNetCore.Authentication.SK.SmartId.SmartId.Rest.Dao
{
    public class AuthenticationResponse
    {
        [JsonPropertyName("sessionID")]
        public string SessionId { get; set; }
    }
}