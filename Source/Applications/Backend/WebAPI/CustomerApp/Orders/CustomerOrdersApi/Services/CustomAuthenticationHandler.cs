using CustomerOrdersApi.Library.Config;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;

namespace CustomerOrdersApi.Services
{
	public class CustomAuthenticationHandler : AuthenticationHandler<SignatureOptions>
	{
		private const string _authorizationHeaderName = "Authorization";
		private const string _authenticateFailValue = "Unauthorized";

		public CustomAuthenticationHandler(
			IOptionsMonitor<SignatureOptions> options,
			ILoggerFactory logger,
			UrlEncoder encoder,
			ISystemClock clock)
			: base(options, logger, encoder, clock)
		{
		}

		protected override Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			if(!Request.Headers.ContainsKey(_authorizationHeaderName))
			{
				return Task.FromResult(AuthenticateResult.Fail(_authenticateFailValue));
			}

			string authorizationToken = Request.Headers[_authorizationHeaderName];

			if(string.IsNullOrEmpty(authorizationToken))
			{
				return Task.FromResult(AuthenticateResult.Fail(_authenticateFailValue));
			}

			var sourceAndToken = authorizationToken.Split('-');

			if(sourceAndToken.Length != 2)
			{
				return Task.FromResult(AuthenticateResult.Fail(_authenticateFailValue));
			}

			try
			{
				return Task.FromResult(ValidateToken(sourceAndToken));
			}
			catch(Exception ex)
			{
				return Task.FromResult(AuthenticateResult.Fail(ex.Message));
			}
		}

		private AuthenticateResult ValidateToken(string[] sourceAndToken)
		{
			Source? source;
			try
			{
				source = Enum.Parse<Source>(sourceAndToken[0]);
			}
			catch(Exception e)
			{
				Logger.LogError(e, "Произошла ошибка при парсинге источника из параметра аутентификации");
				throw;
			}

			if(source is null)
			{
				return AuthenticateResult.Fail(_authenticateFailValue);
			}

			var token = sourceAndToken[1];

			var isValid = source switch
			{
				Source.MobileApp => Options.MobileApp == token,
				Source.VodovozWebSite => Options.VodovozWebSite == token,
				_ => false
			};

			if(!isValid)
			{
				return AuthenticateResult.Fail(_authenticateFailValue);
			}

			var claims = new List<Claim>
			{
				new(ClaimTypes.Name, isValid.ToString()),
			};

			var identity = new ClaimsIdentity(claims, Scheme.Name);
			var principal = new System.Security.Principal.GenericPrincipal(identity, null);
			var ticket = new AuthenticationTicket(principal, Scheme.Name);
			return AuthenticateResult.Success(ticket);
		}
	}
}
