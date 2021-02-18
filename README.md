# AspNetCore.Authentication.SK.SmartID

**AspNetCore.Authentication.SK.SmartID** is a Smart-ID security middleware that you can use in your **ASP.NET Core** application to support SK Smart-ID authentication. It is inspired by **[Microsoft Twitter authentication](https://github.com/dotnet/aspnetcore/tree/master/src/Security/Authentication/Twitter/src)** and **[SK Smart-ID java client](https://github.com/SK-EID/smart-id-java-client)**. It is not perfect, but functional as external authentication.

**The latest alpha release can be found on [NuGet](https://www.nuget.org/packages/AspNetCore.Authentication.SK.SmartID)**.

## Getting started
**Install live or demo SK root CA and intermediate certificates to your running computer or server from [SK site](https://www.skidsolutions.eu/repositoorium/sk-sertifikaadid/)**. They have to be installed in propriate stores or received user certifiactes are not validated.

Add following lines to your `Startup` class:
```AspNetCore
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication()
        .AddSmartId<ApplicationUser>(SmartIdDefaults.DemoCertificatePublicKey, options =>
            {
                const string displayText = "Smart-ID ASP.NET Core";
                options.RelyingPartyUUID = Configuration["SmartID:RelyingPartyUUID"];
                options.RelyingPartyName = Configuration["SmartID:RelyingPartyName"];
                //options.UseDemo(true); // To use Smart-ID demo, use this.

                options.AllowedInteractions.Add(
                    new AllowedInteraction(AllowedInteractionType.VerificationCodeChoice, displayText));
                options.AllowedInteractions.Add(
                    new AllowedInteraction(AllowedInteractionType.DisplayTextAndPin, displayText));
            });
}

public void Configure(IApplicationBuilder app)
{
    app.UseAuthentication();
    app.UseAuthorization();
}
```
See the [/sample](https://github.com/kaupov/AspNetCore.Authentication.SK.SmartID/tree/main/sample) directory for a complete sample **using ASP.NET Core MVC**.