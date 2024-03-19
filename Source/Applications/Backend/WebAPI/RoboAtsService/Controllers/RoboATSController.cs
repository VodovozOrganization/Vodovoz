using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RoboatsService.Monitoring;
using RoboatsService.Requests;
using System;
using System.Diagnostics;
using Vodovoz.Domain.Roboats;

namespace RoboatsService.Controllers
{
	[ApiController]
	[Route("api")]
	[Authorize]
	public class RoboatsController : ControllerBase
	{
		private readonly ILogger<RoboatsController> _logger;

		private readonly RequestHandlerFactory _handlerFactory;
		private readonly RoboatsCallRegistrator _roboatsCallRegistrator;

		public RoboatsController(ILogger<RoboatsController> logger, RequestHandlerFactory handlerFactory, RoboatsCallRegistrator roboatsCallRegistrator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_handlerFactory = handlerFactory ?? throw new ArgumentNullException(nameof(handlerFactory));
			_roboatsCallRegistrator = roboatsCallRegistrator ?? throw new ArgumentNullException(nameof(roboatsCallRegistrator));
		}

		[HttpGet]
		public string Get([FromQuery] RequestDto request)
		{
			var stopWatch = new Stopwatch();
			stopWatch.Start();

			_roboatsCallRegistrator.RegisterCall(request.ClientPhone, request.CallGuid);

			var handler = _handlerFactory.GetHandler(request);
			if(handler == null)
			{
				_roboatsCallRegistrator.RegisterTerminatingFail(request.ClientPhone, request.CallGuid, RoboatsCallFailType.UnknownRequest, RoboatsCallOperation.OnCreateHandler,
					"Неизвестный тип запроса. Обратитесь в отдел разработки.");
				return "ERROR. Null request";
			}

			var result = handler.Execute();
			stopWatch.Stop();
			var query = HttpContext.Request.GetEncodedPathAndQuery();
			_logger.LogInformation($"Request: {query} | Response: {result} | Request time: {stopWatch.Elapsed.Seconds}.{stopWatch.Elapsed.Milliseconds} sec.");
			return result;
		}
	}
}
