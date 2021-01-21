using System;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AspNetCore.Authentication.SK.SmartID.SmartID;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AspNetCore.Authentication.SK.SmartID
{
    public class SmartIdHandler : AuthenticationHandler<SmartIdOptions>
    {
        private readonly SmartIdClient _smartIdClient;

        public SmartIdHandler(IOptionsMonitor<SmartIdOptions> options, ILoggerFactory logger, UrlEncoder encoder,
            ISystemClock clock, SmartIdClient smartIdClient) : base(options,
            logger, encoder, clock)
        {
            _smartIdClient = smartIdClient;
        }

        protected override Task InitializeHandlerAsync()
        {
            _smartIdClient.RelyingPartyUuid = Options.RelyingPartyUUID;
            _smartIdClient.RelyingPartyName = Options.RelyingPartyName;
            _smartIdClient.HostUrl = Options.HostUrl;

            return base.InitializeHandlerAsync();
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            throw new NotImplementedException();
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var sessionId = properties.GetString("SessionId");
            var hash = properties.GetString("Hash");

            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(hash))
            {
                var countryCode = properties.GetString("CountryCode");
                var nationalIdentityNumber = properties.GetString("NationalIdentityNumber");

                if (string.IsNullOrEmpty(countryCode) || string.IsNullOrEmpty(nationalIdentityNumber))
                {
                    Context.Response.Redirect($"./SmartIdAuthentication");
                    return;
                }

                await StartAuthenticationAsync(countryCode, nationalIdentityNumber);

                return;
            }

            await CheckSessionAsync(sessionId, hash, properties);
        }

        private async Task StartAuthenticationAsync(string countryCode, string nationalIdentityNumber)
        {
            try
            {
                var session = await _smartIdClient.StartAuthenticationAsync(countryCode, nationalIdentityNumber, Options.AllowedInteractions);

                Context.Response.Redirect(
                    $"./SmartIdAuthentication?handler=ShowVerificationCode&sessionId={session.Id}&verificationCode={session.VerificationCode}&hash={Uri.EscapeDataString(session.AuthenticationHashInBase64)}");
            }
            catch (SmartIdTroubleException exception)
            {
                Logger.LogError(exception,
                    $"Failed to start SmartId authentication, trouble {exception.Trouble} returned");
                Context.Response.Redirect($"./ExternalLogin?handler=Callback&remoteError={exception.Trouble:G}");
            }
            catch (HttpRequestException exception)
            {
                Logger.LogError(exception, "Failed to start SmartId authentication, invalid result returned");
                Context.Response.Redirect($"./ExternalLogin?handler=Callback&remoteError={Trouble.Unknown:G}");
            }
        }

        private async Task CheckSessionAsync(string sessionId, string hash, AuthenticationProperties properties)
        {
            try
            {
                var sessionResult = await _smartIdClient.CheckSessionAsync(sessionId, hash);

                sessionResult.EnsureValid();

                await Context.SignInAsync(IdentityConstants.ExternalScheme,
                    new ClaimsPrincipal(sessionResult.GetIdentity()), properties);
                Context.Response.Redirect("./ExternalLogin?handler=Callback");
            }
            catch (SmartIdTroubleException exception)
            {
                Logger.LogError(exception, $"Failed to check SmartId session, trouble {exception.Trouble} returned");
                Context.Response.Redirect($"./ExternalLogin?handler=Callback&remoteError={exception.Trouble:G}");
            }
            catch (HttpRequestException exception)
            {
                Logger.LogError(exception, "Failed to check SmartId session, invalid results returned");
                Context.Response.Redirect($"./ExternalLogin?handler=Callback&remoteError={Trouble.Unknown:G}");
            }
        }
    }
}