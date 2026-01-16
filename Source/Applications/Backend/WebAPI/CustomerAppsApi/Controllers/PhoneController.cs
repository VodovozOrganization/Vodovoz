using System;
using System.Linq;
using CustomerAppsApi.Library.Dto.Contacts;
using CustomerAppsApi.Library.Services;
using CustomerAppsApi.Library.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Extensions;

namespace CustomerAppsApi.Controllers
{
	/// <summary>
	/// Контроллер для работы с телефонами
	/// </summary>
	[ApiController]
	[Route("/api/[action]")]
	public class PhoneController : ControllerBase
	{
		private readonly ILogger<PhoneController> _logger;
		private readonly ICounterpartyRequestDataValidator _requestDataValidator;
		private readonly CustomerAppPhoneService _customerAppPhoneService;

		public PhoneController(
			ILogger<PhoneController> logger,
			ICounterpartyRequestDataValidator requestDataValidator,
			CustomerAppPhoneService customerAppPhoneService
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_requestDataValidator = requestDataValidator ?? throw new ArgumentNullException(nameof(requestDataValidator));
			_customerAppPhoneService = customerAppPhoneService ?? throw new ArgumentNullException(nameof(customerAppPhoneService));
		}

		/// <summary>
		/// Добавление номера телефона к клиенту
		/// </summary>
		/// <param name="dto">Детали запроса <see cref="AddingPhoneNumberDto"/></param>
		/// <returns></returns>
		[HttpPost]
		public IActionResult AddPhoneNumber(AddingPhoneNumberDto dto)
		{
			var source = dto.Source.GetEnumDisplayName();
			
			_logger.LogInformation(
				"Поступил запрос на добавление телефона {PhoneNumber} к клиенту {CounterpartyId} от пользователя: {ExternalCounterpartyId} с {Source}",
				dto.PhoneNumber,
				dto.ErpCounterpartyId,
				dto.ExternalCounterpartyId,
				source);
			
			try
			{
				var validationResult = _requestDataValidator.AddPhoneToCounterpartyValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при добавлении телефона {PhoneNumber} к клиенту " +
						"{CounterpartyId} от пользователя: {ExternalCounterpartyId}:\n{ValidationResult}",
						dto.PhoneNumber,
						dto.ErpCounterpartyId,
						dto.ExternalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}

				var result = _customerAppPhoneService.AddPhoneToCounterparty(dto);
				
				if(result.IsFailure)
				{
					var error = result.Errors.First();

					return error.Code switch
					{
						"400" => BadRequest(error.Message),
						"404" => NotFound(error.Message),
						"422" => UnprocessableEntity(error.Message),
						_ => Problem(error.Message)
					};
				}
				
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при добавлении телефона {PhoneNumber} к клиенту " +
					"{CounterpartyId} от пользователя: {ExternalCounterpartyId} с {Source}",
					dto.PhoneNumber,
					dto.ErpCounterpartyId,
					dto.ExternalCounterpartyId,
					source);
				return Problem();
			}
		}
	}
}
