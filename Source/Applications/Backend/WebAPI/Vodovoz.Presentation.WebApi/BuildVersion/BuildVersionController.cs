using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;
using Vodovoz.Presentation.WebApi.Common;

namespace Vodovoz.Presentation.WebApi.BuildVersion
{
	public class BuildVersionController : ApiControllerBase
	{
		private readonly Version _entryAssemblyVersion;

		public BuildVersionController(ILogger<BuildVersionController> logger) : base(logger)
		{
			_entryAssemblyVersion = Assembly.GetEntryAssembly().GetName().Version;
		}

		[HttpGet("GetVersion")]
		[Produces(typeof(GetVersionResponse))]
		[ApiExplorerSettings(IgnoreApi = true)]
		public IActionResult GetVersion()
		{
			return Ok(
				new GetVersionResponse
				{
					Version = _entryAssemblyVersion,
					BuildedAt = GetDateTimeFromVersion(_entryAssemblyVersion)
				});
		}

		private DateTime GetDateTimeFromVersion(Version version) =>
			new DateTime(2000, 1, 1)
				.AddDays(version.Build)
				.AddSeconds(version.Revision * 2);
	}
}
