using System;
using AspNetCore.Authentication.SK.SmartID.SmartID;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.Authentication.SK.SmartID
{
    public static class SmartIdExtensions
    {
        public static AuthenticationBuilder AddSmartId(this AuthenticationBuilder builder) =>
            builder.AddSmartId(string.Empty, _ => { });

        public static AuthenticationBuilder AddSmartId(this AuthenticationBuilder builder,
            string serverCertificatePublicKey,
            Action<SmartIdOptions> configureOptions) =>
            builder.AddSmartId(SmartIdDefaults.AuthenticationScheme, serverCertificatePublicKey, configureOptions);

        public static AuthenticationBuilder AddSmartId(this AuthenticationBuilder builder, string authenticationScheme,
            string serverCertificatePublicKey,
            Action<SmartIdOptions> configureOptions) =>
            builder.AddSmartId(authenticationScheme, SmartIdDefaults.DisplayName, serverCertificatePublicKey, configureOptions);

        public static AuthenticationBuilder AddSmartId(this AuthenticationBuilder builder,
            string authenticationScheme, string displayName,
            string serverCertificatePublicKey,
            Action<SmartIdOptions> configureOptions)
        {
            builder.Services.AddHttpClient<SmartIdClient>(client => {})
                .ConfigurePrimaryHttpMessageHandler(() => new SmartIdHttpClientHandler(serverCertificatePublicKey));

            return builder.AddScheme<SmartIdOptions, SmartIdHandler>(authenticationScheme, displayName,
                configureOptions);
        }
    }
}