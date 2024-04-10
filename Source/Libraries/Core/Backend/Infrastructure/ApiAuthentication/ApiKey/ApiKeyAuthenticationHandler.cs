using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace ApiAuthentication.ApiKey
{
	public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
	{
		private readonly IConfiguration _configuration;

		public ApiKeyAuthenticationHandler(
			IConfiguration configuration,
			IOptionsMonitor<ApiKeyAuthenticationOptions> options,
			ILoggerFactory logger,
			UrlEncoder encoder,
			ISystemClock clock)
			: base(options, logger, encoder, clock)
		{
			_configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
		}

		protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			if(!Request.Headers.TryGetValue(ApiKeyAuthenticationOptions.HeaderName, out var apiKeyFromRequest) || apiKeyFromRequest.Count != 1)
			{
				Logger.LogWarning($"An API request was received without the {ApiKeyAuthenticationOptions.HeaderName} header");
				return AuthenticateResult.Fail("Invalid parameters");
			}

			var apiKey = _configuration.GetValue<string>(ApiKeyAuthenticationOptions.HeaderName);

			if(!apiKeyFromRequest.Equals(apiKey))
			{
				return AuthenticateResult.Fail("Unauthorized client");
			}

			Logger.LogInformation("Client authenticated");
			var claims = new[] { new Claim(ClaimTypes.Name, "key") };
			var identity = new ClaimsIdentity(claims, ApiKeyAuthenticationOptions.DefaultScheme);
			var identities = new List<ClaimsIdentity> { identity };
			var principal = new ClaimsPrincipal(identities);
			var ticket = new AuthenticationTicket(principal, ApiKeyAuthenticationOptions.DefaultScheme);

			return AuthenticateResult.Success(ticket);
		}
	}
}
