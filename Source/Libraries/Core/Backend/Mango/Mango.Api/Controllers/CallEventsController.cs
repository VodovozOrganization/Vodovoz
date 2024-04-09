using Mango.Api.Validators;
using Mango.Core.Dto;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mango.Api.Controllers
{
	[ApiController]
	[Route("events")]
	public class CallEventsController : ControllerBase
	{
		private readonly ILoggerFactory _loggerFactory;
		private readonly KeyValidator _keyValidator;
		private readonly SignValidator _signValidator;
		private readonly IBus _messageBus;
		private readonly ILogger _eventsJsonLogger;
		private readonly ILogger _deserializationErrorLogger;

		public CallEventsController(
			ILoggerFactory loggerFactory, 
			KeyValidator keyValidator,
			SignValidator signValidator,
			IBus messageBus)
		{
			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
			_keyValidator = keyValidator ?? throw new ArgumentNullException(nameof(keyValidator));
			_signValidator = signValidator ?? throw new ArgumentNullException(nameof(signValidator));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));

			_eventsJsonLogger = _loggerFactory.CreateLogger("Events.Json");
			_deserializationErrorLogger = _loggerFactory.CreateLogger("Deserialization.Errors");
		}

		[HttpPost()]
		[Route("call")]
		public async Task CallEvent(
			[FromForm(Name = "vpbx_api_key")] string vpbxKey,
			[FromForm(Name = "sign")] string sign,
			[FromForm(Name = "json")] string json)
		{
			HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
			_eventsJsonLogger.LogTrace("Call event: {json}", json);

			if(!_keyValidator.Validate(vpbxKey))
			{
				return;
			}

			if(!_signValidator.Validate(sign, json))
			{
				return;
			}

			try
			{
				var call = JsonSerializer.Deserialize<MangoCallEvent>(json);
				await _messageBus.Publish(call);
			}
			catch(JsonException ex)
			{
				_deserializationErrorLogger.LogError(ex, "Ошибка десериализации события звонка. Json: {json}", json);
			}
		}

		[HttpPost()]
		[Route("summary")]
		public async Task SummaryEvent(
			[FromForm(Name = "vpbx_api_key")] string vpbxKey,
			[FromForm(Name = "sign")] string sign,
			[FromForm(Name = "json")] string json)
		{
			HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
			_eventsJsonLogger.LogTrace("Summary event: {json}", json);

			if(!_keyValidator.Validate(vpbxKey))
			{
				return;
			}

			if(!_signValidator.Validate(sign, json))
			{
				return;
			}

			try
			{
				var summary = JsonSerializer.Deserialize<MangoSummaryEvent>(json);
				await _messageBus.Publish(summary);
			}
			catch(JsonException ex)
			{
				_deserializationErrorLogger.LogError(ex, "Ошибка десериализации события завершения звонка. Json: {json}", json);
			}
		}
	}
}
