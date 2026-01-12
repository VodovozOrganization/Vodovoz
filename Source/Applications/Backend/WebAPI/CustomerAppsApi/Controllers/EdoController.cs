using System;
using System.Linq;
using CustomerAppsApi.Library.Dto.Edo;
using CustomerAppsApi.Library.Services;
using CustomerAppsApi.Library.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Extensions;

namespace CustomerAppsApi.Controllers
{
	/// <summary>
	/// Контроллер для работы с параметрами ЭДО
	/// </summary>
	[ApiController]
	[Route("/api/[action]")]
	public class EdoController : ControllerBase
	{
		private readonly ILogger<PhoneController> _logger;
		private readonly ICounterpartyRequestDataValidator _requestDataValidator;
		private readonly CustomerAppEdoService _customerAppEdoService;

		public EdoController(
			ILogger<PhoneController> logger,
			ICounterpartyRequestDataValidator requestDataValidator,
			CustomerAppEdoService customerAppEdoService
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_requestDataValidator = requestDataValidator ?? throw new ArgumentNullException(nameof(requestDataValidator));
			_customerAppEdoService = customerAppEdoService ?? throw new ArgumentNullException(nameof(customerAppEdoService));
		}
		
		/// <summary>
		/// Обновление информации о целях покупки воды
		/// </summary>
		/// <param name="dto">Детали запроса <see cref="UpdatingCounterpartyPurposeOfPurchase"/></param>
		/// <returns></returns>
		[HttpPost]
		public IActionResult UpdateCounterpartyPurposeOfPurchase(UpdatingCounterpartyPurposeOfPurchase dto)
		{
			var source = dto.Source.GetEnumDisplayName();
			
			_logger.LogInformation(
				"Поступил запрос на обновление цели покупки воды {PurposeOfPurchase} у клиента {CounterpartyId} от пользователя: {ExternalCounterpartyId} с {Source}",
				dto.WaterPurposeOfPurchase,
				dto.CounterpartyErpId,
				dto.ExternalCounterpartyId,
				source);
			
			try
			{
				var validationResult = _requestDataValidator.UpdateCounterpartyPurposeOfPurchaseValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при обновлении цели покупки воды у клиента " +
						"{CounterpartyId} от пользователя: {ExternalCounterpartyId}:\n{ValidationResult}",
						dto.CounterpartyErpId,
						dto.ExternalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}

				var result = _customerAppEdoService.UpdateCounterpartyPurposeOfPurchase(dto);
				
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
					"Ошибка при обновлении цели покупки воды у клиента " +
					"{CounterpartyId} от пользователя: {ExternalCounterpartyId} с {Source}",
					dto.CounterpartyErpId,
					dto.ExternalCounterpartyId,
					source);
				return Problem();
			}
		}
		
		/// <summary>
		/// Добавление ЭДО аккаунта
		/// </summary>
		/// <param name="dto">Детали запроса <see cref="AddingEdoAccount"/></param>
		/// <returns></returns>
		[HttpPost]
		public IActionResult AddEdoAccount(AddingEdoAccount dto)
		{
			var source = dto.Source.GetEnumDisplayName();
			
			_logger.LogInformation(
				"Поступил запрос на добавление ЭДО аккаунта {EdoAccount} клиенту {CounterpartyId} от пользователя: {ExternalCounterpartyId} с {Source}",
				dto.EdoAccount,
				dto.CounterpartyErpId,
				dto.ExternalCounterpartyId,
				source);
			
			try
			{
				var validationResult = _requestDataValidator.AddEdoAccountValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при добавлении ЭДО аккаунта у клиента " +
						"{CounterpartyId} от пользователя: {ExternalCounterpartyId}:\n{ValidationResult}",
						dto.CounterpartyErpId,
						dto.ExternalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}

				var result = _customerAppEdoService.AddEdoAccount(dto);
				
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
					"Ошибка при добавлении ЭДО аккаунта клиенту " +
					"{CounterpartyId} от пользователя: {ExternalCounterpartyId} с {Source}",
					dto.CounterpartyErpId,
					dto.ExternalCounterpartyId,
					source);
				return Problem();
			}
		}
		
		/// <summary>
		/// Получение списка операторов ЭДО
		/// </summary>
		/// <param name="request">Детали запроса <see cref="GetEdoOperatorsRequest"/></param>
		/// <returns></returns>
		[HttpGet]
		public IActionResult GetEdoOperators(GetEdoOperatorsRequest request)
		{
			var source = request.Source.GetEnumDisplayName();
			
			_logger.LogInformation(
				"Поступил запрос на получение операторов ЭДО от {ExternalCounterpartyId} {Source}",
				request.ExternalCounterpartyId,
				source);
			
			try
			{
				var validationResult = _requestDataValidator.GetOperatorsValidate(request);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при получении операторов ЭДО от {ExternalCounterpartyId}:\n{ValidationResult}",
						request.ExternalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}
				
				var result = _customerAppEdoService.GetEdoOperators();
				return Ok(result);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при получении операторов ЭДО от {ExternalCounterpartyId} {Source}",
					request.ExternalCounterpartyId,
					source);
				return Problem();
			}
		}
	}
}
