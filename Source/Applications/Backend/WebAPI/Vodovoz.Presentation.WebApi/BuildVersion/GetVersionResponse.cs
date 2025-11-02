using System;

namespace Vodovoz.Presentation.WebApi.BuildVersion
{
	public class GetVersionResponse
	{
		public Version Version { get; internal set; }
		public DateTime BuildedAt { get; set; }
	}
}
