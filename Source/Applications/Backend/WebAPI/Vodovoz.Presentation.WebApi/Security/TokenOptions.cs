using System;

namespace Vodovoz.Presentation.WebApi.Security
{
	public class TokenOptions
	{
		public bool ValidateIssuer { get; set; } = false;
		public string Issuer { get; set; }
		public bool ValidateAudience { get; set; } = false;
		public string Audience { get; set; }
		public bool ValidateIssuerSigningKey { get; set; } = true;
		public string Key { get; set; }
		public bool RequireExpirationTime { get; set; } = false;
		public TimeSpan Lifetime { get; set; }
	}
}
