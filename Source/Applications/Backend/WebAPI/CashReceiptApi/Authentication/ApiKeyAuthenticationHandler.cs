using CashReceiptApi.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace CashReceiptApi.Authentication
{
	public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
	{
		private readonly IConfiguration _configuration;
		private readonly IOptions<ServiceOptions> _serviceOptions;

		public ApiKeyAuthenticationHandler(
			IConfiguration configuration,
			IOptionsMonitor<ApiKeyAuthenticationOptions> options,
			IOptions<ServiceOptions> serviceOptions,
			ILoggerFactory logger,
			UrlEncoder encoder,
			ISystemClock clock)
			: base(options, logger, encoder, clock)
		{
			_configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
			_serviceOptions = serviceOptions ?? throw new ArgumentNullException(nameof(serviceOptions));
		}

		protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			if(!Request.Headers.TryGetValue(nameof(_serviceOptions.Value.ApiKey), out var apiKeyFromRequest) || apiKeyFromRequest.Count != 1)
			{
				Logger.LogWarning("An API request was received without the {0} header", nameof(_serviceOptions.Value.ApiKey));
				return AuthenticateResult.Fail("Invalid parameters");
			}

			if(!apiKeyFromRequest.Equals(_serviceOptions.Value.ApiKey))
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
