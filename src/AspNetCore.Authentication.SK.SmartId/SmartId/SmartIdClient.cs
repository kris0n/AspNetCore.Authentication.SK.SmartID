using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AspNetCore.Authentication.SK.SmartId.SmartId.Rest.Dao;

namespace AspNetCore.Authentication.SK.SmartId.SmartId
{
    public class SmartIdClient
    {
        private readonly HttpClient _httpClient;

        public string RelyingPartyUuid { get; set; }

        public string RelyingPartyName { get; set; }

        public string HostUrl { get; set; }

        public bool AskVerificationCodeChoice { get; set; }

        public string DisplayText { get; set; }

        public SmartIdClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Session> StartAuthenticationAsync(string countryCode,
            string nationalIdentityNumber)
        {
            var uriBuilder = new UriBuilder(HostUrl);
            uriBuilder.Path += $"authentication/pno/{countryCode}/{nationalIdentityNumber}";

            var authenticationHash = AuthenticationHash.GenerateRandomHash();
            var authenticationRequest = CreateAuthenticationRequest(authenticationHash);
            var jsonString = JsonSerializer.Serialize(authenticationRequest,
                new JsonSerializerOptions {IgnoreNullValues = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var responseMessage = await _httpClient.PostAsync(uriBuilder.Uri, content);

            switch (responseMessage.StatusCode)
            {
                case HttpStatusCode.Forbidden:
                    throw new SmartIdTroubleException(Trouble.NoPermissionToIssueRequest);
                case HttpStatusCode.NotFound:
                    throw new SmartIdTroubleException(Trouble.UserDoesNotHaveAccountInSmartIdSystem);
            }

            responseMessage.EnsureSuccessStatusCode();

            var responseStream = await responseMessage.Content.ReadAsStreamAsync();
            var session = await JsonSerializer.DeserializeAsync<AuthenticationResponse>(responseStream);
            return new Session(session.SessionId, authenticationHash.CalculateVerificationCode(),
                authenticationHash.HashInBase64());
        }

        public async Task<SmartIdAuthenticationResponse> CheckSessionAsync(string sessionId, string hash)
        {
            var uriBuilder = new UriBuilder(HostUrl);
            uriBuilder.Path += $"session/{sessionId}";

            SessionStatus sessionStatus;

            do
            {
                var responseMessage = await _httpClient.GetAsync(uriBuilder.Uri);

                if (responseMessage.StatusCode == HttpStatusCode.NotFound)
                    throw new SmartIdTroubleException(Trouble.SessionExpired);

                responseMessage.EnsureSuccessStatusCode();

                var responseString = await responseMessage.Content.ReadAsStringAsync();
                sessionStatus = JsonSerializer.Deserialize<SessionStatus>(responseString,
                    new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
            } while (sessionStatus.State == "RUNNING");

            var smartIdAuthenticationResponse = new SmartIdAuthenticationResponse(sessionStatus, hash);

            return smartIdAuthenticationResponse;
        }

        private AuthenticationRequest CreateAuthenticationRequest(SignableHash hash)
        {
            var request = new AuthenticationRequest(RelyingPartyUuid, RelyingPartyName, hash.HashInBase64(), "SHA512")
            {
                DisplayText = !string.IsNullOrEmpty(DisplayText) ? DisplayText : null,
                RequestProperties = AskVerificationCodeChoice ? new RequestProperties {VcChoice = true} : null
            };

            return request;
        }
    }
}