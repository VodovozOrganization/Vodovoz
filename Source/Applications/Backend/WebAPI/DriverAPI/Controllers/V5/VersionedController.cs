﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Presentation.WebApi.Common;

namespace DriverAPI.Controllers.V5
{
	/// <summary>
	/// Базовый контроллер с версией
	/// </summary>
	[ApiVersion(Startup._apiVersion)]
	[Route("api/v{version:apiVersion}/[action]")]
	[ApiController]
	public class VersionedController : ApiControllerBase
	{
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="logger"></param>
		public VersionedController(ILogger<ApiControllerBase> logger) : base(logger)
		{
		}
	}
}
