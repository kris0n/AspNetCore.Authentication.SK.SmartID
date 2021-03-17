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

        public List<AllowedInteraction> AllowedInteractions { get; } = new List<AllowedInteraction>();

        /// <summary>
        /// Set to load user certificate validation chain certificates from CurrentUser/My store instead of using default system stores.
        /// Strongly adviced to use only in environments where Trusted Root and Intermediate Certificate stores are not available, like in Azure App Service.
        /// </summary>
        public bool LoadCertsFromMyStore { get; set; }

        internal string HostUrl { get; private set; } = SmartIdDefaults.LiveHostUrl;

        internal bool SkipRevocationCheck { get; private set; }

        /// <summary>
        /// Call this to use Smart-ID demo environment. Do not set RelyingPartyUUID or RelyingPartyName after calling this method.
        /// </summary>
        /// <param name="skipRevocationCheck">Pass true to work around revocation check failure due to faulty configuration in demo intermediate certificate.</param>
        /// <returns>This options object.</returns>
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