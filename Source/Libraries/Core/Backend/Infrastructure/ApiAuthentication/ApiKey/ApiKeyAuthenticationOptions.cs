using Microsoft.AspNetCore.Authentication;

namespace ApiAuthentication.ApiKey
{
	public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
	{
		public const string DefaultScheme = "ClientKey";
		public const string HeaderName = "ApiKey";
	}
}
