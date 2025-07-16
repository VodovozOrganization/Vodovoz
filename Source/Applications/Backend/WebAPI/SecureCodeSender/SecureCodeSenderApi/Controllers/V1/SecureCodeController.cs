using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using SecureCodeSender.Contracts.Requests;
using SecureCodeSender.Contracts.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SecureCodeSenderApi.Services;
using Vodovoz.Presentation.WebApi.Common;

namespace SecureCodeSenderApi.Controllers.V1
{
	/// <summary>
	/// Контроллер для работы с кодами авторизации
	/// </summary>
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

		/// <summary>
		/// Генерация и отправка коа авторизации
		/// </summary>
		/// <param name="sendSecureCodeDto">Информация для отправки</param>
		/// <returns>
		/// 200 - в случае успеха с временем до следующего запроса
		/// 404 - неверный код доступа
		/// 408 - истекший код
		/// 500 - ошибка
		/// </returns>
		[HttpPost("Send")]
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

				return Ok(SecureCodeSent.Create(result.Value.TimeForNextCode));
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
		
		/// <summary>
		/// Проверка кода авторизации
		/// </summary>
		/// <param name="checkSecureCodeDto">Данные для проверки</param>
		/// <returns>
		/// 200 - в случае успеха
		/// 500 - ошибка/неудача
		/// </returns>
		[HttpGet("Check")]
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
