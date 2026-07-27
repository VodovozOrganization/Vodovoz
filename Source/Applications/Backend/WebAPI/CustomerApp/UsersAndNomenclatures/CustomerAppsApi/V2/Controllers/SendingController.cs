using System;
using CustomerAppsApi.Library.V2.Dto;
using CustomerAppsApi.Library.V2.Models;
using CustomerAppsApi.Library.V2.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.V2.Controllers
{
	[Authorize]
	[ApiVersion("2.0")]
	public class SendingController : VersionedController
	{
		private readonly ISendingService _sendingService;
		private readonly ICounterpartyModelValidator _counterpartyModelValidator;

		public SendingController(
			ILogger<SendingController> logger,
			ISendingService sendingService,
			ICounterpartyModelValidator counterpartyModelValidator) : base(logger)
		{
			_sendingService = sendingService ?? throw new ArgumentNullException(nameof(sendingService));
			_counterpartyModelValidator = counterpartyModelValidator ?? throw new ArgumentNullException(nameof(counterpartyModelValidator));
		}

		/// <summary>
		/// Отправка кода авторизации на указанный email
		/// </summary>
		/// <param name="codeToEmailDto">Данные для отправки</param>
		/// <returns>Http код</returns>
		[HttpPost]
		public IActionResult SendCodeToEmail(SendingCodeToEmailDto codeToEmailDto)
		{
			_logger.LogInformation(
				"Запрос на отправку кода авторизации от {ExternalCounterpartyId} на {Email}",
				codeToEmailDto.ExternalUserId,
				codeToEmailDto.EmailAddress);
			
			try
			{
				var validationResult = _counterpartyModelValidator.SendingCodeToEmailDtoValidate(codeToEmailDto);
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogWarning(
						"Не прошли валидацию для отправки кода авторизации от {ExternalCounterpartyId} на {Email} {ValidationResult}",
						codeToEmailDto.ExternalUserId,
						codeToEmailDto.EmailAddress,
						validationResult);
					
					return ValidationProblem(validationResult);
				}
				
				var isDryRun = HttpResponseHelper.IsHealthCheckRequest(Request);
				
				var result = _sendingService.SendCodeToEmail(codeToEmailDto, isDryRun);

				if(result.IsFailure)
				{
					return BadRequest(result.GetErrorsString());
				}

				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при отправке кода авторизации от {ExternalCounterpartyId} на {Email}",
					codeToEmailDto.ExternalUserId,
					codeToEmailDto.EmailAddress);

				return Problem();
			}
		}
	}
}
