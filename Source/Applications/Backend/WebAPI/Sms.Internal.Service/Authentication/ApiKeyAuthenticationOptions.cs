using Microsoft.AspNetCore.Authentication;

namespace Sms.Internal.Service.Authentication
{
	public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
	{
		public const string DefaultScheme = "ClientKey";
		public const string HeaderName = "ApiKey";
	}
}
