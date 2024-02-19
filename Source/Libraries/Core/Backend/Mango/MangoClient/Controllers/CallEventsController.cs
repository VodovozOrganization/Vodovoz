using Mango.Api.Dto;
using Mango.Api.Validators;
using Mango.Core.Handlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;

namespace Mango.Api.Controllers
{
	[ApiController]
	[Route("mango/events")]
	public class CallEventsController : ControllerBase
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly IRequestValidator _requestValidator;
		private readonly IEnumerable<ICallEventHandler> _callEventHandlers;
		private readonly ILogger _eventsJsonLogger;

		public CallEventsController(ILoggerFactory loggerFactory, IRequestValidator requestValidator, IEnumerable<ICallEventHandler> callEventHandlers)
		{
			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			_requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
			_callEventHandlers = callEventHandlers ?? throw new ArgumentNullException(nameof(callEventHandlers));

			_eventsJsonLogger = _loggerFactory.CreateLogger("Mango.CallEvent.Json");
		}

		[HttpPost("call")]
		public void Call([FromForm] CallEventRequest eventRequest)
		{
			_eventsJsonLogger.LogInformation(eventRequest.Json);

			HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;

			if(!_requestValidator.Validate(eventRequest))
			{
				return;
			}

			foreach(var callEventHandler in _callEventHandlers)
			{
				callEventHandler.HandleAsync(eventRequest.Event);
			}
			return;
		}
	}
}
