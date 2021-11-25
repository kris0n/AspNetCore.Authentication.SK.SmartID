using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AspNetCore.Authentication.SK.SmartID.SmartID.Rest.Dao;

namespace AspNetCore.Authentication.SK.SmartID.SmartID
{
    public class SmartIdClient
    {
        private readonly HttpClient _httpClient;
        
        public SmartIdOptions Options { get; set; }
        
        public SmartIdClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Session> StartAuthenticationAsync(string countryCode,
            string nationalIdentityNumber, List<AllowedInteraction> allowedInteractionsOrder)
        {
            var uriBuilder = new UriBuilder(Options.HostUrl);
            uriBuilder.Path += $"authentication/etsi/PNO{countryCode}-{nationalIdentityNumber}";

            var authenticationHash = AuthenticationHash.GenerateRandomHash();
            var authenticationRequest = CreateAuthenticationRequest(authenticationHash, allowedInteractionsOrder);
            var jsonString = JsonSerializer.Serialize(authenticationRequest,
                new JsonSerializerOptions {IgnoreNullValues = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var responseMessage = await _httpClient.PostAsync(uriBuilder.Uri, content);

            switch ((int) responseMessage.StatusCode)
            {
                case (int) HttpStatusCode.Unauthorized:
                    throw new SmartIdTroubleException(Trouble.InterfaceAuthenticationFailed);
                case (int) HttpStatusCode.Forbidden:
                    throw new SmartIdTroubleException(Trouble.NoPermissionToIssueRequest);
                case (int) HttpStatusCode.NotFound:
                    throw new SmartIdTroubleException(Trouble.UserDoesNotHaveAccountInSmartIdSystem);
                case 471:
                    throw new SmartIdTroubleException(Trouble
                        .NoSuitableAccountOfRequestedTypeFoundButUserHasSomeOtherAccounts);
                case 472:
                    throw new SmartIdTroubleException(Trouble.PersonShouldViewSmartIdAppOrSmartIdSelfServicePortalNow);
                case 480:
                    throw new SmartIdTroubleException(Trouble.TheClientIsTooOldAndNotSupportedAnyMore);
                case 580:
                    throw new SmartIdTroubleException(Trouble.SystemIsUnderMaintenance);
            }

            responseMessage.EnsureSuccessStatusCode();

            var responseString = await responseMessage.Content.ReadAsStringAsync();
            var session = JsonSerializer.Deserialize<AuthenticationResponse>(responseString);
            return new Session(session.SessionId, authenticationHash.CalculateVerificationCode(),
                authenticationHash.HashInBase64());
        }

        public async Task<Session> StartSigningAsync(string countryCode,
            string nationalIdentityNumber, List<AllowedInteraction> allowedInteractionsOrder, byte[] document)
        {
            var uriBuilder = new UriBuilder(Options.HostUrl);
            uriBuilder.Path += $"signature/etsi/PNO{countryCode}-{nationalIdentityNumber}";

            var signatureHash = SignatureHash.GetHash(document);
            var authenticationRequest = CreateAuthenticationRequest(signatureHash, allowedInteractionsOrder);
            var jsonString = JsonSerializer.Serialize(authenticationRequest,
                new JsonSerializerOptions { IgnoreNullValues = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            var responseMessage = await _httpClient.PostAsync(uriBuilder.Uri, content);

            switch ((int)responseMessage.StatusCode)
            {
                case (int)HttpStatusCode.Unauthorized:
                    throw new SmartIdTroubleException(Trouble.InterfaceAuthenticationFailed);
                case (int)HttpStatusCode.Forbidden:
                    throw new SmartIdTroubleException(Trouble.NoPermissionToIssueRequest);
                case (int)HttpStatusCode.NotFound:
                    throw new SmartIdTroubleException(Trouble.UserDoesNotHaveAccountInSmartIdSystem);
                case 471:
                    throw new SmartIdTroubleException(Trouble
                        .NoSuitableAccountOfRequestedTypeFoundButUserHasSomeOtherAccounts);
                case 472:
                    throw new SmartIdTroubleException(Trouble.PersonShouldViewSmartIdAppOrSmartIdSelfServicePortalNow);
                case 480:
                    throw new SmartIdTroubleException(Trouble.TheClientIsTooOldAndNotSupportedAnyMore);
                case 580:
                    throw new SmartIdTroubleException(Trouble.SystemIsUnderMaintenance);
            }

            responseMessage.EnsureSuccessStatusCode();

            var responseString = await responseMessage.Content.ReadAsStringAsync();
            var session = JsonSerializer.Deserialize<AuthenticationResponse>(responseString);
            return new Session(session.SessionId, signatureHash.CalculateVerificationCode(),
                signatureHash.HashInBase64());
        }

        public async Task<SmartIdAuthenticationResponse> CheckSessionAsync(string sessionId, string hash)
        {
            var uriBuilder = new UriBuilder(Options.HostUrl);
            uriBuilder.Path += $"session/{sessionId}";

            SessionStatus sessionStatus;

            do
            {
                var responseMessage = await _httpClient.GetAsync(uriBuilder.Uri);

                if (responseMessage.StatusCode == HttpStatusCode.NotFound)
                    throw new SmartIdTroubleException(Trouble.SessionDoesNotExistOrHasExpired);

                responseMessage.EnsureSuccessStatusCode();

                var responseString = await responseMessage.Content.ReadAsStringAsync();
                sessionStatus = JsonSerializer.Deserialize<SessionStatus>(responseString,
                    new JsonSerializerOptions {PropertyNamingPolicy = JsonNamingPolicy.CamelCase});
            } while (sessionStatus.State == "RUNNING");

            var smartIdAuthenticationResponse = new SmartIdAuthenticationResponse(sessionStatus, hash);

            if (Options.HostUrl == SmartIdDefaults.DemoHostUrl && Options.SkipRevocationCheck)
                smartIdAuthenticationResponse.SkipCertificateRevocationCheck();

            if (Options.LoadCertsFromMyStore)
            {
                using var certStore = new X509Store();
                certStore.Open(OpenFlags.ReadOnly);

                smartIdAuthenticationResponse.SetChainExtraStore(certStore.Certificates);
            }

            return smartIdAuthenticationResponse;
        }

        private AuthenticationRequest CreateAuthenticationRequest(SignableHash hash,
            List<AllowedInteraction> allowedInteractionsOrder)
        {
            var allowedInteraction = allowedInteractionsOrder.Select(CreateAllowedInteraction).ToList();
            var request = new AuthenticationRequest(Options.RelyingPartyUUID, Options.RelyingPartyName, hash.HashInBase64(), "SHA512",
                allowedInteraction);

            return request;
        }

        private static AllowedInteractionsOrder CreateAllowedInteraction(AllowedInteraction interaction)
        {
            var result = new AllowedInteractionsOrder();

            switch (interaction.Type)
            {
                case AllowedInteractionType.DisplayTextAndPin:
                    result.Type = "displayTextAndPIN";
                    result.DisplayText60 = interaction.DisplayText;
                    break;
                case AllowedInteractionType.VerificationCodeChoice:
                    result.Type = "verificationCodeChoice";
                    result.DisplayText60 = interaction.DisplayText;
                    break;
                case AllowedInteractionType.ConfirmationMessage:
                    result.Type = "confirmationMessage";
                    result.DisplayText200 = interaction.DisplayText;
                    break;
                case AllowedInteractionType.ConfirmationMessageAndVerificationCodeChoice:
                    result.Type = "confirmationMessageAndVerificationCodeChoice";
                    result.DisplayText200 = interaction.DisplayText;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }
    }
}