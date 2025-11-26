using System;
using System.Linq;
using CustomerAppsApi.Library.Dto.Counterparties;
using CustomerAppsApi.Library.Services;
using CustomerAppsApi.Library.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Extensions;
using Vodovoz.Presentation.WebApi.Common;

namespace CustomerAppsApi.Controllers
{
	/// <summary>
	/// Контроллер для работы по юр лицам
	/// </summary>
	[ApiController]
	[Route("/api/[action]")]
	public class LegalCounterpartyController : ApiControllerBase
	{
		private readonly ICounterpartyRequestDataValidator _requestDataValidator;
		private readonly LegalCounterpartyService _legalCounterpartyService;

		public LegalCounterpartyController(
			ILogger<LegalCounterpartyController> logger,
			ICounterpartyRequestDataValidator requestDataValidator,
			LegalCounterpartyService legalCounterpartyService) : base(logger)
		{
			_requestDataValidator = requestDataValidator ?? throw new ArgumentNullException(nameof(requestDataValidator));
			_legalCounterpartyService = legalCounterpartyService ?? throw new ArgumentNullException(nameof(legalCounterpartyService));
		}
		
		/// <summary>
		/// Проверка пароля учетной записи
		/// </summary>
		/// <param name="dto">Детали запроса <see cref="CheckPasswordRequest"/></param>
		/// <returns></returns>
		[HttpGet]
		public IActionResult CheckPassword(CheckPasswordRequest dto)
		{
			var source = dto.Source.GetEnumDisplayName();
			
			_logger.LogInformation(
				"Поступил запрос на проверку пароля почты {Email} от {ExternalCounterpartyId} {Source}",
				dto.Email,
				dto.ExternalCounterpartyId,
				source);
			
			try
			{
				var validationResult = _requestDataValidator.CheckPasswordValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при проверке пароля от {ExternalCounterpartyId}:\n{ValidationResult}",
						dto.ExternalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}
				
				var result = _legalCounterpartyService.CheckPassword(dto);

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
					"Ошибка при при проверке пароля почты {Email} от {ExternalCounterpartyId} {Source}",
					dto.Email,
					dto.ExternalCounterpartyId,
					source);
				return Problem();
			}
		}
		
		/// <summary>
		/// Получение юр лиц по ИНН
		/// </summary>
		/// <param name="dto">Детали запроса <see cref="LegalCustomersByInnRequest"/></param>
		/// <returns></returns>
		[HttpGet]
		public IActionResult GetLegalCustomersByInn(LegalCustomersByInnRequest dto)
		{
			var source = dto.Source.GetEnumDisplayName();
			
			_logger.LogInformation(
				"Поступил запрос на получение юр лиц по ИНН {INN} от {ExternalCounterpartyId} {Source}",
				dto.Inn,
				dto.ExternalCounterpartyId,
				source);
			
			try
			{
				var validationResult = _requestDataValidator.LegalCustomersByInnValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при получении юр лиц по ИНН {INN} от {ExternalCounterpartyId}:\n{ValidationResult}",
						dto.Inn,
						dto.ExternalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}
				
				var result = _legalCounterpartyService.GetLegalCustomersByInn(dto);

				if(result.IsSuccess)
				{
					return Ok(result.Value);
				}

				var error = result.Errors.First();

				return error.Code switch
				{
					"404" => NotFound(),
					_ => Problem(error.Message)
				};
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
		/// Получение идентификатора юр лица с активной электронной почтой(по ней подключен пользователь физик)
		/// </summary>
		/// <param name="dto">Детали запроса <see cref="CompanyWithActiveEmailRequest"/></param>
		/// <returns></returns>
		[HttpGet]
		public IActionResult GetCompanyList(CompanyWithActiveEmailRequest dto)
		{
			var source = dto.Source.GetEnumDisplayName();
			
			_logger.LogInformation(
				"Поступил запрос на получение идентификатора юр лица с активной почтой {Email} от пользователя {ExternalCounterpartyId} {Source}",
				dto.Email,
				dto.ExternalCounterpartyId,
				source);
			
			try
			{
				var validationResult = _requestDataValidator.CompanyWithActiveEmailValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при получении идентификатора юр лица с активной почтой {Email}" +
						" от пользователя {ExternalCounterpartyId}:\n{ValidationResult}",
						dto.Email,
						dto.ExternalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}
				
				var result = _legalCounterpartyService.GetCompanyWithActiveEmail(dto);

				if(result.IsSuccess)
				{
					return Ok(result.Value);
				}

				var error = result.Errors.First();

				return error.Code switch
				{
					"404" => NotFound(),
					_ => Problem(error.Message)
				};
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при получении идентификатора юр лиц с активной почтой {Email} от пользователя {ExternalCounterpartyId} {Source}",
					dto.Email,
					dto.ExternalCounterpartyId,
					source);
				
				return Problem();
			}
		}
		
		/// <summary>
		/// Получение основной информации об учетной записи юр. лица или ИП
		/// </summary>
		/// <param name="dto">Детали запроса <see cref="CompanyInfoRequest"/></param>
		/// <returns></returns>
		[HttpGet]
		public IActionResult GetCompanyInfo(CompanyInfoRequest dto)
		{
			var source = dto.Source.GetEnumDisplayName();
			
			_logger.LogInformation(
				"Поступил запрос на получение данных о юр лице от пользователя {ExternalCounterpartyId} {Source}",
				dto.ExternalCounterpartyId,
				source);
			
			try
			{
				var validationResult = _requestDataValidator.CompanyInfoRequestDataValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при получении данных о юр лице от пользователя {ExternalCounterpartyId}:\n{ValidationResult}",
						dto.ExternalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}
				
				var result = _legalCounterpartyService.GetCompanyInfo(dto);

				if(result.IsSuccess)
				{
					return Ok(result.Value);
				}

				var error = result.Errors.First();

				return error.Code switch
				{
					"404" => NotFound(),
					_ => Problem(error.Message)
				};
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при получении данных о юр лице от пользователя {ExternalCounterpartyId} {Source}",
					dto.ExternalCounterpartyId,
					source);
				
				return Problem();
			}
		}
		
		/// <summary>
		/// Получение списка юр лиц доступных физику для заказа
		/// </summary>
		/// <param name="dto">Детали запроса <see cref="GetNaturalCounterpartyLegalCustomersDto"/></param>
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
				var validationResult = _requestDataValidator.GetNaturalCounterpartyLegalCustomersDtoValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при получении юр лиц клиента Id: {CounterpartyId} от {ExternalCounterpartyId}:\n{ValidationResult}",
						dto.ErpCounterpartyId,
						dto.ExternalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}
				
				var result = _legalCounterpartyService.GetNaturalCounterpartyLegalCustomers(dto);

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
			var source = dto.Source.GetEnumDisplayName();
			
			_logger.LogInformation(
				"Поступил запрос на регистрацию нового юр лица от пользователя: {ExternalCounterpartyId} с {Source}",
				dto.ExternalCounterpartyId,
				source);
			
			try
			{
				var validationResult = _requestDataValidator.RegisteringLegalCustomerValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при регистрации нового юр лица от: {ExternalCounterpartyId}:\n{ValidationResult}",
						dto.ExternalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}

				var result = _legalCounterpartyService.RegisterLegalCustomer(dto);
				
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
					"Ошибка при регистрации нового юр лица от: {ExternalCounterpartyId} с {Source}",
					dto.ExternalCounterpartyId,
					source);
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
				var validationResult = _requestDataValidator.LinkingEmailToLegalCounterpartyValidate(dto);
				
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
				
				var result = _legalCounterpartyService.LinkLegalCounterpartyEmailToExternalUser(dto);
				
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
		/// Получение списка контактов юр лица
		/// </summary>
		/// <param name="dto">Детали запроса <see cref="LegalCounterpartyContactListRequest"/></param>
		/// <returns></returns>
		[HttpGet]
		public IActionResult GetLegalCounterpartyContactList(LegalCounterpartyContactListRequest dto)
		{
			var source = dto.Source.GetEnumDisplayName();
			_logger.LogInformation(
				"Поступил запрос на получение контактов, прикрепленных к юрику с Id: {LegalId} от пользователя {ExternalCounterpartyId} {Source}",
				dto.ErpCounterpartyId,
				dto.ExternalCounterpartyId,
				source
			);
			
			try
			{
				var validationResult = _requestDataValidator.GetLegalCustomerContactsValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при получении контактов, прикрепленных к юрику с Id: {LegalId} от пользователя {ExternalCounterpartyId}:\n{ValidationResult}",
						dto.ErpCounterpartyId,
						dto.ExternalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}
				
				var result = _legalCounterpartyService.GetLegalCustomerContacts(dto);
				
				if(result.IsFailure)
				{
					return NotFound(result.Errors.First().Message);
				}
				
				return Ok(result.Value);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e, 
					"Ошибка при получении контактов, прикрепленных к юрику с Id: {LegalId} от пользователя {ExternalCounterpartyId} {Source}",
					dto.ErpCounterpartyId,
					dto.ExternalCounterpartyId,
					source);
				return Problem();
			}
		}
	}
}
