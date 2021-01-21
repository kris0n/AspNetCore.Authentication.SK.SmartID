using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore.Authentication.SK.SmartID.SmartID;
using Xunit;

namespace AspNetCore.Authentication.SK.SmartID.Tests
{
    public class SmartIdClientShould
    {
        private readonly SmartIdClient _smartIdClient;

        public static IEnumerable<object[]> Accounts
        {
            get
            {
                yield return new object[] {"OK", "EE", "10101010005", "PNOEE-10101010005", "DEMO", "SMART-ID" };
                yield return new object[] {"OK", "LV", "010101-10006", "PNOLV-010101-10006", "DEMO", "SMART-ID" };
                yield return new object[] {"OK", "LT", "10101010005", "PNOLT-10101010005", "DEMO", "SMART-ID" };
                yield return new object[] { "USER_REFUSED", "EE", "10101010016", null, null, null };
                yield return new object[] { "USER_REFUSED", "LV", "010101-10014", null, null, null };
                yield return new object[] { "USER_REFUSED", "LT", "10101010016", null, null, null };
                yield return new object[] { "TIMEOUT", "EE", "10101010027", null, null, null };
                yield return new object[] { "TIMEOUT", "LT", "10101010027", null, null, null };
            }
        }

        public SmartIdClientShould()
        {
            var smartIdOptions = new SmartIdOptions().UseDemo();
            _smartIdClient =
                new SmartIdClient(
                    new HttpClient(new SmartIdHttpClientHandler(SmartIdDefaults.DemoCertificatePublicKey)))
                {
                    RelyingPartyUuid = smartIdOptions.RelyingPartyUUID,
                    RelyingPartyName = smartIdOptions.RelyingPartyName,
                    HostUrl = smartIdOptions.HostUrl
                };
        }

        [Theory, MemberData(nameof(Accounts))]
        public async Task ProcessAuthentication(string expectedResult, string countryCode, string nationalIdentityNumber,
            string nameIdentifier, string givenName, string surName)
        {
            var session = await _smartIdClient.StartAuthenticationAsync(countryCode, nationalIdentityNumber,
                new List<AllowedInteraction>
                    {new AllowedInteraction(AllowedInteractionType.DisplayTextAndPin, "Test")});

            Assert.NotNull(session);
            Assert.Equal(4, session.VerificationCode.Length);

            var sessionResult = await _smartIdClient.CheckSessionAsync(session.Id, session.AuthenticationHashInBase64);

            Assert.NotNull(sessionResult);
            Assert.Equal(expectedResult, sessionResult.EndResult);

            if (expectedResult == "OK")
            {
                sessionResult.EnsureValid();

                var identity = sessionResult.GetIdentity();
                Assert.NotNull(identity);

                var nameIdentifierClaim = identity.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier);
                Assert.Equal(nameIdentifier, nameIdentifierClaim.Value);

                var givenNameClaim = identity.FindFirst(claim => claim.Type == ClaimTypes.GivenName);
                Assert.Equal(givenName, givenNameClaim.Value);

                var surNameClaim = identity.FindFirst(claim => claim.Type == ClaimTypes.Surname);
                Assert.Equal(surName, surNameClaim.Value);

                var serialNumberClaim = identity.FindFirst(claim => claim.Type == ClaimTypes.SerialNumber);
                Assert.Equal(nationalIdentityNumber, serialNumberClaim.Value);

                var countryClaim = identity.FindFirst(claim => claim.Type == ClaimTypes.Country);
                Assert.Equal(countryCode, countryClaim.Value);
            }
        }
    }
}