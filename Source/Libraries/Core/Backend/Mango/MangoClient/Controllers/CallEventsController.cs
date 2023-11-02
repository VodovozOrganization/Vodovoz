using Mango.Api.Dto;
using Mango.Api.Validators;
using Mango.Core.Handlers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;

namespace Mango.Api.Controllers
{
	[ApiController]
	[Route("mango/events")]
	public class CallEventsController : ControllerBase
	{
		private readonly IRequestValidator _requestValidator;
		private readonly IEnumerable<ICallEventHandler> _callEventHandlers;

		public CallEventsController(IRequestValidator requestValidator, IEnumerable<ICallEventHandler> callEventHandlers)
		{
			_requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
			_callEventHandlers = callEventHandlers ?? throw new ArgumentNullException(nameof(callEventHandlers));
		}

		[HttpPost("call")]
		public void Call([FromForm] CallEventRequest eventRequest)
		{
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
