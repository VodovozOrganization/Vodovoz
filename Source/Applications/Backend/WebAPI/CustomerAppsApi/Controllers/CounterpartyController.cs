using System;
using System.Linq;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Counterparties;
using CustomerAppsApi.Library.Models;
using CustomerAppsApi.Library.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Extensions;

namespace CustomerAppsApi.Controllers
{
	[ApiController]
	[Route("/api/[action]")]
	public class CounterpartyController : ControllerBase
	{
		private readonly ILogger<CounterpartyController> _logger;
		private readonly ICounterpartyModelValidator _modelValidator;
		private readonly ICounterpartyModel _counterpartyModel;

		public CounterpartyController(
			ILogger<CounterpartyController> logger,
			ICounterpartyModelValidator modelValidator,
			ICounterpartyModel counterpartyModel)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_modelValidator = modelValidator ?? throw new ArgumentNullException(nameof(modelValidator));
			_counterpartyModel = counterpartyModel ?? throw new ArgumentNullException(nameof(counterpartyModel));
		}

		/// <summary>
		/// Идентификация пользователя, как нашего клиента
		/// </summary>
		/// <param name="counterpartyContactInfoDto">
		/// Внешний номер пользователя с телефоном и кодом откуда запрос <see cref="CounterpartyContactInfoDto"/>
		/// </param>
		/// <returns></returns>
		[HttpPost]
		public CounterpartyIdentificationDto GetCounterparty(CounterpartyContactInfoDto counterpartyContactInfoDto)
		{
			try
			{
				return _counterpartyModel.GetCounterparty(counterpartyContactInfoDto);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при идентификации контрагента {ExternalCounterpartyId}",
					counterpartyContactInfoDto.ExternalCounterpartyId);
				throw;
			}
		}
		
		/// <summary>
		/// Регистрация нового клиента в Erp
		/// </summary>
		/// <param name="counterpartyDto">Информация о клиенте <see cref="CounterpartyDto"/></param>
		/// <returns></returns>
		[HttpPost]
		public CounterpartyRegistrationDto RegisterCounterparty(CounterpartyDto counterpartyDto)
		{
			try
			{
				return _counterpartyModel.RegisterCounterparty(counterpartyDto);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при регистрации контрагента {ExternalCounterpartyId}",
					counterpartyDto.ExternalCounterpartyId);
				throw;
			}
		}
		
		/// <summary>
		/// Обновление данных клиента, если что-то было заполнено неверно
		/// </summary>
		/// <param name="counterpartyDto">Информация о клиенте <see cref="CounterpartyDto"/></param>
		/// <returns></returns>
		[HttpPost]
		public CounterpartyUpdateDto UpdateCounterpartyInfo(CounterpartyDto counterpartyDto)
		{
			try
			{
				return _counterpartyModel.UpdateCounterpartyInfo(counterpartyDto);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при обновлении данных контрагента");
				throw;
			}
		}
		
		/// <summary>
		/// Получение юр лиц по ИНН
		/// </summary>
		/// <param name="dto">детали запроса <see cref="GetLegalCustomersByInnDto"/></param>
		/// <returns></returns>
		[HttpGet]
		public IActionResult GetLegalCustomersByInn(GetLegalCustomersByInnDto dto)
		{
			_logger.LogInformation(
				"Поступил запрос на получение юр лиц по ИНН {INN} от {ExternalCounterpartyId}",
				dto.Inn,
				dto.ExternalCounterpartyId);
			
			try
			{
				var validationResult = _counterpartyModel.GetLegalCustomersDtoByInnValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при получении юр лиц по ИНН {INN} от {ExternalCounterpartyId}:\n{ValidationResult}",
						dto.Inn,
						dto.ExternalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}
				
				var result = _counterpartyModel.GetLegalCustomersByInn(dto);

				if(!string.IsNullOrWhiteSpace(result.Message))
				{
					_logger.LogInformation(result.Message + "при получении юр лиц по ИНН {INN} от {ExternalCounterpartyId}",
						dto.Inn,
						dto.ExternalCounterpartyId);
					return BadRequest(result.Message);
				}
				
				return Ok(result.Data);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при получении юр лиц по ИНН {INN} для {ExternalCounterpartyId}", dto.Inn, dto.ExternalCounterpartyId);
				return Problem();
			}
		}

		/// <summary>
		/// Получение списка Id юр лиц с активной электронной почтой(по ней подключен пользователь физик), которая в запросе
		/// </summary>
		/// <param name="dto">Детали запроса <see cref="CompanyWithActiveEmailRequest"/></param>
		/// <returns></returns>
		[HttpGet]
		public IActionResult GetCompanyList(CompanyWithActiveEmailRequest dto)
		{
			var source = dto.Source.GetEnumDisplayName();
			
			_logger.LogInformation(
				"Поступил запрос на получение Id юр лиц с активной почтой {Email} от пользователя {ExternalCounterpartyId} {Source}",
				dto.Email,
				dto.ExternalCounterpartyId,
				source);
			
			try
			{
				var validationResult = _modelValidator.CompanyWithActiveEmailRequestDataValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при получении Id юр лиц с активной почтой {Email} от пользователя {ExternalCounterpartyId}:\n{ValidationResult}",
						dto.Email,
						dto.ExternalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}
				
				var result = _counterpartyModel.GetCompanyWithActiveEmail(dto);
				return Ok(result.Data);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при получении Id юр лиц с активной почтой {Email} от пользователя {ExternalCounterpartyId} {Source}",
					dto.Email,
					dto.ExternalCounterpartyId,
					source);
				
				return Problem();
			}
		}
		
		/// <summary>
		/// Получение списка юр лиц доступных физику для заказа
		/// </summary>
		/// <param name="dto">детали запроса <see cref="GetNaturalCounterpartyLegalCustomersDto"/></param>
		/// <returns></returns>
		[HttpGet]
		public IActionResult GetLegalCustomers(GetNaturalCounterpartyLegalCustomersDto dto)
		{
			_logger.LogInformation(
				"Поступил запрос на получение юр лиц клиента Id: {CounterpartyId} от {ExternalCounterpartyId}",
				dto.ErpCounterpartyId,
				dto.ExternalCounterpartyId);
			
			try
			{
				var validationResult = _counterpartyModel.GetNaturalCounterpartyLegalCustomersDtoValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при получении юр лиц клиента Id: {CounterpartyId} от {ExternalCounterpartyId}:\n{ValidationResult}",
						dto.ErpCounterpartyId,
						dto.ExternalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}
				
				var result = _counterpartyModel.GetNaturalCounterpartyLegalCustomers(dto);

				if(!string.IsNullOrWhiteSpace(result.Message))
				{
					_logger.LogInformation(
						result.Message + "при получении юр лиц клиента Id: {CounterpartyId} от {ExternalCounterpartyId}",
						dto.ErpCounterpartyId,
						dto.ExternalCounterpartyId);
					return BadRequest(result.Message);
				}
				
				return Ok(result.Data);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при получении юр лиц клиента Id: {CounterpartyId} от {ExternalCounterpartyId}",
					dto.ErpCounterpartyId,
					dto.ExternalCounterpartyId);
				return Problem();
			}
		}
		
		/// <summary>
		/// Регистрация нового юр лица
		/// </summary>
		/// <param name="dto">Детали запроса <see cref="RegisteringLegalCustomerDto"/></param>
		/// <returns></returns>
		[HttpPost]
		public IActionResult RegisterLegalCustomer(RegisteringLegalCustomerDto dto)
		{
			_logger.LogInformation(
				"Поступил запрос на регистрацию нового юр лица от клиента Id: {CounterpartyId}",
				dto.ErpCounterpartyId);
			
			try
			{
				var validationResult = _counterpartyModel.RegisteringLegalCustomerValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при регистрации нового юр лица от клиента Id: {CounterpartyId}:\n{ValidationResult}",
						dto.ErpCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}

				var result = _counterpartyModel.RegisterLegalCustomer(dto);
				
				if(!string.IsNullOrWhiteSpace(result.Message))
				{
					return BadRequest(result.Message);
				}
				
				return Ok(result.Data);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при регистрации нового юр лица от клиента Id: {CounterpartyId} с {Source}",
					dto.ErpCounterpartyId,
					dto.Source.GetEnumDisplayName());
				return Problem();
			}
		}

		/// <summary>
		/// Используется для создания связи между электронной почтой (которая потом будет использоваться для авторизации) и К/А,
		/// и присвоения пароля для учетной записи.
		/// </summary>
		/// <param name="dto">Детали запроса <see cref="ConnectingNewPhoneToLegalCustomerDto"/></param>
		/// <returns></returns>
		[HttpPost]
		public IActionResult LinkEmailToLegalCounterparty(LinkingLegalCounterpartyEmailToExternalUser dto)
		{
			_logger.LogInformation(
				"Поступил запрос на прикрепление электронки {Email}, к юрику с Id: {LegalId} от {NaturalUserId}",
				dto.Email,
				dto.ErpCounterpartyId,
				dto.ExternalCounterpartyId
			);
			
			try
			{
				var validationResult = _counterpartyModel.LinkingEmailToLegalCounterpartyValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при прикреплении почты {Email}, к юрику с Id: {LegalId} от {NaturalUserId}:\n{ValidationResult}",
						dto.Email,
						dto.ErpCounterpartyId,
						dto.ExternalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}
				
				var result = _counterpartyModel.LinkLegalCounterpartyEmailToExternalUser(dto);
				
				if(result.IsFailure)
				{
					return BadRequest(result.Errors.First().Message);
				}
				
				return Ok(result.Value);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при ри прикреплении телефона {Email}, к юрику с Id: {LegalId} от {NaturalUserId} с {Source}",
					dto.Email,
					dto.ErpCounterpartyId,
					dto.ExternalCounterpartyId,
					dto.Source.GetEnumDisplayName());
				return Problem();
			}
		}

		/// <summary>
		/// Получение списка телефонов, прикрепленных к юр лицу для возможности заказа в ИПЗ от его имени
		/// </summary>
		/// <param name="dto">детали запроса <see cref="GetPhonesConnectedToLegalCustomerDto"/></param>
		/// <returns></returns>
		[HttpGet]
		public IActionResult GetPhonesConnectedToLegalCustomer(GetPhonesConnectedToLegalCustomerDto dto)
		{
			_logger.LogInformation(
				"Поступил запрос на получение телефонов, прикрепленных к юрику с Id: {LegalId} от {NaturalId} ",
				dto.ErpLegalCounterpartyId,
				dto.ErpNaturalCounterpartyId
				);
			
			return Problem("Функция не реализована");
			
			try
			{
				var validationResult = _counterpartyModel.GetPhonesConnectedToLegalCustomerValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при получению телефонов, прикрепленных к юрику с Id: {LegalId} от {NaturalId}:\n{ValidationResult}",
						dto.ErpLegalCounterpartyId,
						dto.ErpNaturalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}
				
				var result = _counterpartyModel.GetPhonesConnectedToLegalCustomer(dto);
				
				if(!string.IsNullOrWhiteSpace(result.Message))
				{
					return BadRequest(result.Message);
				}
				
				return Ok(result.Data);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при прикреплении физика с Id {NaturalId} к юрику с Id: {LegalId} с {Source}",
					dto.ErpNaturalCounterpartyId,
					dto.ErpLegalCounterpartyId,
					dto.Source.GetEnumDisplayName());
				return Problem();
			}
		}

		/// <summary>
		/// Обновление связи телефона физика и юр лица (активация или блокировка привязки)
		/// </summary>
		/// <param name="dto">детали запроса <see cref="GetPhonesConnectedToLegalCustomerDto"/></param>
		/// <returns></returns>
		[HttpPost]
		public IActionResult UpdateConnectToLegalCustomerByPhone(UpdateConnectToLegalCustomerByPhoneDto dto)
		{
			_logger.LogInformation(
				"Поступил запрос на обновление связи телефона {PhoneId}, прикрепленного к юрику с Id: {LegalId} от {NaturalId} ",
				dto.ErpPhoneId,
				dto.ErpLegalCounterpartyId,
				dto.ErpNaturalCounterpartyId
			);

			return Problem("Функция не реализована");
			
			try
			{
				var validationResult = _counterpartyModel.UpdateConnectToLegalCustomerByPhoneValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию на обновление связи телефона {PhoneId}, прикрепленного к юрику с Id: {LegalId} от {NaturalId}:\n{ValidationResult}",
						dto.ErpPhoneId,
						dto.ErpLegalCounterpartyId,
						dto.ErpNaturalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}
				
				var result = _counterpartyModel.UpdateConnectToLegalCustomerByPhone(dto);
				
				if(!string.IsNullOrWhiteSpace(result))
				{
					return BadRequest(result);
				}
				
				return Ok();
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при обновлении связи телефона {PhoneId}, прикрепленного к юрику с Id: {LegalId} от {NaturalId} с {Source}",
					dto.ErpPhoneId,
					dto.ErpNaturalCounterpartyId,
					dto.ErpLegalCounterpartyId,
					dto.Source.GetEnumDisplayName());
				return Problem();
			}
		}
	}
}
