using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Models;
using CustomerAppsApi.Library.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.Controllers
{
	[ApiController]
	[Route("/api/[action]")]
	public class SendingController : ControllerBase
	{
		private readonly ILogger<SendingController> _logger;
		private readonly ISendingService _sendingService;
		private readonly ICounterpartyRequestDataValidator _counterpartyRequestDataValidator;

		public SendingController(
			ILogger<SendingController> logger,
			ISendingService sendingService,
			ICounterpartyRequestDataValidator counterpartyRequestDataValidator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_sendingService = sendingService ?? throw new ArgumentNullException(nameof(sendingService));
			_counterpartyRequestDataValidator = counterpartyRequestDataValidator ?? throw new ArgumentNullException(nameof(counterpartyRequestDataValidator));
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
				var validationResult = _counterpartyRequestDataValidator.SendingCodeToEmailDtoValidate(codeToEmailDto);
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
