using Mango.Api.Validators;
using Mango.Core.Dto;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mango.Api.Controllers
{
	[ApiController]
	[Route("events")]
	public class CallEventsController : ControllerBase
	{
		private readonly ILogger<CallEventsController> _logger;
		private readonly KeyValidator _keyValidator;
		private readonly SignValidator _signValidator;
		private readonly IBus _messageBus;

		public CallEventsController(
			ILogger<CallEventsController> logger,
			KeyValidator keyValidator,
			SignValidator signValidator,
			IBus messageBus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_keyValidator = keyValidator ?? throw new ArgumentNullException(nameof(keyValidator));
			_signValidator = signValidator ?? throw new ArgumentNullException(nameof(signValidator));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		[HttpPost()]
		[Route("call")]
		public async Task CallEvent(
			[FromForm(Name = "vpbx_api_key")] string vpbxKey,
			[FromForm(Name = "sign")] string sign,
			[FromForm(Name = "json")] string json)
		{
			Activity.Current?.AddTag("vpbx_api_key", vpbxKey);
			Activity.Current?.AddTag("sign", sign);
			HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
			_logger.LogTrace("Call event: {JsonInputString}", json);

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
				_logger.LogError(ex, "Ошибка десериализации события звонка. Json: {JsonInputString}", json);
			}
		}

		[HttpPost()]
		[Route("summary")]
		public async Task SummaryEvent(
			[FromForm(Name = "vpbx_api_key")] string vpbxKey,
			[FromForm(Name = "sign")] string sign,
			[FromForm(Name = "json")] string json)
		{
			Activity.Current?.AddTag("vpbx_api_key", vpbxKey);
			Activity.Current?.AddTag("sign", sign);
			HttpContext.Response.StatusCode = (int)HttpStatusCode.OK;
			_logger.LogTrace("Summary event: {JsonInputString}", json);

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
				_logger.LogError(ex, "Ошибка десериализации события завершения звонка. Json: {JsonInputString}", json);
			}
		}
	}
}
