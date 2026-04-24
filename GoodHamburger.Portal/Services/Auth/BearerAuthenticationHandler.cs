using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;

namespace GoodHamburger.Portal.Services.Auth;

public class BearerAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public BearerAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Não faz validação real - apenas permite que o middleware exista
        // A validação real é feita no CustomAuthStateProvider
        return Task.FromResult(AuthenticateResult.NoResult());
    }
}