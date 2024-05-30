using System;

namespace Vodovoz.Presentation.WebApi.Security
{
	public class TokenOptions
	{
		public string Issuer { get; set; }
		public string Audience { get; set; }
		public string Key { get; set; }
		public TimeSpan Lifetime { get; set; }
	}
}
