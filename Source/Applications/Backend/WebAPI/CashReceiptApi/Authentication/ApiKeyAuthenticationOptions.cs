using Microsoft.AspNetCore.Authentication;

namespace CashReceiptApi.Authentication
{
	public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
	{
		public const string DefaultScheme = "ClientKey";
	}
}
