using System;
using System.Linq;
using System.Net.Mime;
using CustomerAppsApi.Library.Dto.Counterparties;
using CustomerAppsApi.Library.Dto.Counterparties.Password;
using CustomerAppsApi.Library.Services;
using CustomerAppsApi.Library.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Extensions;

namespace CustomerAppsApi.Controllers
{
	[ApiController]
	[Route("/api/[action]")]
	public class AccountController : ControllerBase
	{
		private readonly ILogger<LegalCounterpartyController> _logger;
		private readonly ICounterpartyRequestDataValidator _requestDataValidator;
		private readonly CustomerAppAccountService _accountService;

		public AccountController(
			ILogger<LegalCounterpartyController> logger,
			ICounterpartyRequestDataValidator requestDataValidator,
			CustomerAppAccountService accountService)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_requestDataValidator = requestDataValidator ?? throw new ArgumentNullException(nameof(requestDataValidator));
			_accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
		}
		
		/// <summary>
		/// Смена пароля учетной записи
		/// </summary>
		/// <param name="dto">Детали запроса <see cref="ChangePasswordRequest"/></param>
		/// <returns></returns>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult ChangePassword([FromBody] ChangePasswordRequest dto)
		{
			return Problem("Не работает");
			var source = dto.Source.GetEnumDisplayName();
			
			_logger.LogInformation(
				"Поступил запрос на смену пароля аккаунта клиента {LegalCounterpartyId} с почтой {Email} от {ExternalUserId} {Source}",
				dto.ErpCounterpartyId,
				dto.Email,
				dto.ExternalCounterpartyId,
				source);
			
			try
			{
				var validationResult = _requestDataValidator.ChangePasswordValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при смене пароля от {ExternalUserId}:\n{ValidationResult}",
						dto.ExternalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}
				
				var result = _accountService.ChangePassword(dto);

				if(result.IsSuccess)
				{
					return Ok();
				}

				var error = result.Errors.First();

				return error.Code switch
				{
					"404" => NotFound(),
					"401" => Problem(error.Message, statusCode: int.Parse(error.Code), title: "Password is not correct"),
					_ => Problem(error.Message)
				};
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при смене пароля аккаунта клиента {LegalCounterpartyId} с почтой {Email} от {ExternalUserId} {Source}",
					dto.ErpCounterpartyId,
					dto.Email,
					dto.ExternalCounterpartyId,
					source);
				return Problem();
			}
		}
		
		/// <summary>
		/// Удаление аккаунта учетной записи юр лица
		/// </summary>
		/// <param name="dto">Детали запроса <see cref="ChangePasswordRequest"/></param>
		/// <returns></returns>
		[HttpPost]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult DeleteLegalCounterpartyAccount([FromBody] DeleteLegalCounterpartyAccountRequest dto)
		{
			return Problem("Не работает");
			
			var source = dto.Source.GetEnumDisplayName();
			
			_logger.LogInformation(
				"Поступил запрос на смену пароля аккаунта клиента {LegalCounterpartyId} с почтой {Email} от {ExternalUserId} {Source}",
				dto.ErpCounterpartyId,
				dto.Email,
				dto.ExternalCounterpartyId,
				source);
			
			try
			{
				var validationResult = _requestDataValidator.DeleteLegalCounterpartyAccountValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при смене пароля от {ExternalUserId}:\n{ValidationResult}",
						dto.ExternalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}
				
				var result = _accountService.DeleteLegalCounterpartyAccount(dto);

				if(result.IsSuccess)
				{
					return Ok();
				}

				var error = result.Errors.First();

				return error.Code switch
				{
					"404" => NotFound(),
					"401" => Problem(error.Message, statusCode: int.Parse(error.Code), title: "Password is not correct"),
					_ => Problem(error.Message)
				};
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при смене пароля аккаунта клиента {LegalCounterpartyId} с почтой {Email} от {ExternalUserId} {Source}",
					dto.ErpCounterpartyId,
					dto.Email,
					dto.ExternalCounterpartyId,
					source);
				return Problem();
			}
		}
	}
}
