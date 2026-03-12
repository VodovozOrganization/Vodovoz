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
using Vodovoz.Settings.SecureCodes;

namespace SecureCodeSenderApi.Controllers.V1
{
	/// <summary>
	/// Контроллер для работы с кодами авторизации
	/// </summary>
	public class SecureCodeController : VersionedController
	{
		private readonly ISecureCodeServiceValidator _codeServiceValidator;
		private readonly ISecureCodeHandler _secureCodeHandler;
		private readonly ISecureCodeSettings _secureCodeSettings;

		public SecureCodeController(
			ILogger<SecureCodeController> logger,
			ISecureCodeServiceValidator codeServiceValidator,
			ISecureCodeHandler secureCodeHandler,
			ISecureCodeSettings secureCodeSettings)
			: base(logger)
		{
			_codeServiceValidator = codeServiceValidator ?? throw new ArgumentNullException(nameof(codeServiceValidator));
			_secureCodeHandler = secureCodeHandler ?? throw new ArgumentNullException(nameof(secureCodeHandler));
			_secureCodeSettings = secureCodeSettings ?? throw new ArgumentNullException(nameof(secureCodeSettings));
		}

		/// <summary>
		/// Генерация и отправка кода авторизации
		/// </summary>
		/// <param name="sendSecureCodeDto">Информация для отправки</param>
		/// <returns>
		/// 200 - в случае успеха с временем до следующего запроса <see cref="SecureCodeSent"/>
		/// 422 - невозможность отправки в Телеграм
		/// 500 - ошибка/неудача
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
				_logger.LogInformation(
					"Пришел запрос на отправку кода от {Source}: {@SendSecureCode}",
					source,
					sendSecureCodeDto);

				var validationResult = _codeServiceValidator.Validate(sendSecureCodeDto);
				if(validationResult != null)
				{
					return ValidationProblem(validationResult);
				}

				var result = await _secureCodeHandler.GenerateAndSendSecureCode(sendSecureCodeDto);

				if(result.IsSuccess)
				{
					return Ok(SecureCodeSent.Create(_secureCodeSettings.TimeForNextCodeSeconds));
				}

				var firstError = result.Errors.First();
				return firstError.Code switch
				{
					"422" => Problem(firstError.Message, statusCode: int.Parse(firstError.Code)),
					_ => Problem(firstError.Message)
				};
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при генерации и отправке кода авторизации на {SendTo} {Target} для пользователя {ExternalUserId}",
					nameof(sendSecureCodeDto.Method),
					sendSecureCodeDto.Target,
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
		/// 404 - неверный код доступа
		/// 408 - истекший код
		/// 500 - ошибка
		/// </returns>
		[HttpGet("Check")]
		[Consumes(MediaTypeNames.Application.Json)]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> CheckSecureCode([FromBody] CheckSecureCodeDto checkSecureCodeDto)
		{
			try
			{
				var source = checkSecureCodeDto.Source;
				_logger.LogInformation(
					"Пришел запрос на проверку кода от {Source}: {@CheckSecureCode}",
					source,
					checkSecureCodeDto);

				var validationResult = _codeServiceValidator.Validate(checkSecureCodeDto);
				if(validationResult != null)
				{
					return ValidationProblem(validationResult);
				}

				var result = await _secureCodeHandler.CheckSecureCode(checkSecureCodeDto);

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
