using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Contracts.Requests;
using Contracts.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecureCodeSenderApi.Services;
using Vodovoz.Presentation.WebApi.Common;

namespace SecureCodeSenderApi.Controllers.V1
{
	public class SecureCodeController : VersionedController
	{
		private readonly ISecureCodeHandler _secureCodeHandler;

		public SecureCodeController(
			ILogger<ApiControllerBase> logger,
			ISecureCodeHandler secureCodeHandler)
			: base(logger)
		{
			_secureCodeHandler = secureCodeHandler ?? throw new ArgumentNullException(nameof(secureCodeHandler));
		}

		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> SendSecureCode(SendSecureCodeDto sendSecureCodeDto)
		{
			try
			{
				var source = sendSecureCodeDto.Source;

				_logger.LogInformation("Пришел запрос на отправку кода от {Source}: {@SendSecureCode}", source, sendSecureCodeDto);

				var result = await _secureCodeHandler.GenerateAndSendSecureCode(sendSecureCodeDto);

				if(result.IsFailure)
				{
					return Problem(result.Errors.First().Message);
				}

				return Ok(result.Value.TimeForNextCode);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при генерации и отправке кода авторизации для пользователя {ExternalUserId}",
					sendSecureCodeDto.ExternalCounterpartyId
				);

				return Problem(ResponseMessage.Error);
			}
		}
		
		[HttpGet]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public IActionResult CheckSecureCode([FromBody] CheckSecureCodeDto checkSecureCodeDto)
		{
			try
			{
				var source = checkSecureCodeDto.Source;
				_logger.LogInformation("Пришел запрос на проверку кода от {Source}: {@CheckSecureCode}", source, checkSecureCodeDto);

				var result = _secureCodeHandler.CheckSecureCode(checkSecureCodeDto);

				return result.Response switch
				{
					200 => Ok(),
					_ => Problem(result.Message, statusCode: result.Response)
				};
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при проверке кода {Code} авторизации для пользователя {ExternalUserId}",
					checkSecureCodeDto.Code,
					checkSecureCodeDto.ExternalCounterpartyId
					);

				return Problem(ResponseMessage.Error);
			}
		}
	}
}
