using System;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Counterparties;
using CustomerAppsApi.Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CustomerAppsApi.Controllers
{
	[ApiController]
	[Route("/api/[action]")]
	public class SendingController : ControllerBase
	{
		private readonly ILogger<SendingController> _logger;
		private readonly ISendingService _sendingService;

		public SendingController(
			ILogger<SendingController> logger,
			ISendingService sendingService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_sendingService = sendingService ?? throw new ArgumentNullException(nameof(sendingService));
		}

		[HttpPost]
		public IActionResult SendCodeToEmail(SendingCodeToEmailDto codeToEmailDto)
		{
			_logger.LogInformation(
				"Запрос на отправку кода авторизации от {ExternalCounterpartyId} на {Email}",
				codeToEmailDto.ExternalUserId,
				codeToEmailDto.EmailAddress);
			
			try
			{
				/*var validationResult = _counterpartyModelValidator.CounterpartyContactInfoDtoValidate(codeToEmailDto);
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogWarning(
						"Не прошли валидацию для отправки кода авторизации от {ExternalCounterpartyId} на {Email} {ValidationResult}",
						codeToEmailDto.ExternalUserId,
						codeToEmailDto.EmailAddress,
						validationResult);
					
					return ValidationProblem(validationResult);
				}*/

				var result = _sendingService.SendCodeToEmail(codeToEmailDto);

				if(result.IsFailure)
				{
					return Problem(result.GetErrorsString());
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
