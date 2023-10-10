using Microsoft.AspNetCore.Mvc;

namespace Mango.Api.Dto
{
	public abstract class EventRequestBase
	{
		[FromForm(Name = "vpbx_api_key")]
		public string VpbxApiKey { get; set; }

		[FromForm(Name = "sign")]
		public string Sign { get; set; }

		[FromForm(Name = "json")]
		public abstract string Json { get; set; }
	}
}
