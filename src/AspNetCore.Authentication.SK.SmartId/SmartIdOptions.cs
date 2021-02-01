using System.Collections.Generic;
using AspNetCore.Authentication.SK.SmartID.SmartID;
using Microsoft.AspNetCore.Authentication;

namespace AspNetCore.Authentication.SK.SmartID
{
    public class SmartIdOptions : AuthenticationSchemeOptions
    {
        // ReSharper disable once InconsistentNaming
        public string RelyingPartyUUID { get; private set; }

        public string RelyingPartyName { get; private set; }

        public string HostUrl { get; private set; } = SmartIdDefaults.LiveHostUrl;

        public List<AllowedInteraction> AllowedInteractions { get; } = new List<AllowedInteraction>();

        internal bool SkipRevocationCheck { get; private set; }

        public SmartIdOptions UseDemo(bool skipRevocationCheck = false)
        {
            RelyingPartyUUID = SmartIdDefaults.DemoRelyingPartyUuid;
            RelyingPartyName = SmartIdDefaults.DemoRelyingPartyName;
            HostUrl = SmartIdDefaults.DemoHostUrl;
            SkipRevocationCheck = skipRevocationCheck;

            return this;
        }
    }
}