# AspNetCore.Authentication.SK

**AspNetCore.Authentication.SK** has a Smart-ID security middleware that you can use in your **ASP.NET Core** application to support SK Smart-ID authentication. It is inspired by **[Microsoft Twitter authentication](https://github.com/dotnet/aspnetcore/tree/master/src/Security/Authentication/Twitter/src)** and **[SK Smart-ID java client](https://github.com/SK-EID/smart-id-java-client)**. It is not perfect, but functional as external authentication.

**The first alpha release can be found on [GitHub](https://github.com/kaupov?tab=packages&repo_name=AspNetCore.Authentication.SK)**.

## Getting started
**Install live or demo SK root CA and intermediate certificates to your running computer or server from [SK site](https://www.skidsolutions.eu/repositoorium/sk-sertifikaadid/)**. They have to be installed in propriate stores or received user certifiactes are not validated.

Add following lines to your `Startup` class:
```AspNetCore
public void ConfigureServices(IServiceCollection services)
{
    services.AddAuthentication()
        .AddSmartId(SmartIdDefaults.DemoCertificatePublicKey, options =>
            {
                options.UseDemo();
                options.DisplayText = "Smart-ID ASP.NET Core";
                options.AskVerificationCodeChoice = true;
            });
}

public void Configure(IApplicationBuilder app)
{
    app.UseAuthentication();
    app.UseAuthorization();
}
```
See the [/sample](https://github.com/kaupov/AspNetCore.Authentication.SK/tree/main/sample) directory for a complete sample **using ASP.NET Core MVC**.