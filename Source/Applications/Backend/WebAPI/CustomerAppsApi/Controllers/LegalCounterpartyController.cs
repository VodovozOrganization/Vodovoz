using System;
using System.Linq;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.Dto.Counterparties;
using CustomerAppsApi.Library.Services;
using CustomerAppsApi.Library.Validators;
using Microsoft.AspNetCore.Http;
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
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult CheckPassword([FromBody] CheckPasswordRequest dto)
		{
			var source = dto.Source.GetEnumDisplayName();
			
			_logger.LogInformation(
				"Поступил запрос на проверку пароля почты {Email} от {ExternalUserId} {Source}",
				dto.Email,
				dto.ExternalCounterpartyId,
				source);
			
			try
			{
				var validationResult = _requestDataValidator.CheckPasswordValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при проверке пароля от {ExternalUserId}:\n{ValidationResult}",
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
					"Ошибка при проверке пароля почты {Email} от {ExternalUserId} {Source}",
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
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult GetLegalCustomersByInn([FromBody] LegalCustomersByInnRequest dto)
		{
			var source = dto.Source.GetEnumDisplayName();
			
			_logger.LogInformation(
				"Поступил запрос на получение юр лиц по ИНН {INN} от {ExternalUserId} {Source}",
				dto.Inn,
				dto.ExternalCounterpartyId,
				source);
			
			try
			{
				var validationResult = _requestDataValidator.LegalCustomersByInnValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при получении юр лиц по ИНН {INN} от {ExternalUserId}:\n{ValidationResult}",
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
					"Ошибка при получении юр лиц по ИНН {INN} для {ExternalUserId}", dto.Inn, dto.ExternalCounterpartyId);
				return Problem();
			}
		}
		
		/// <summary>
		/// Получение идентификатора юр лица с активной электронной почтой(по ней подключен пользователь физик)
		/// </summary>
		/// <param name="dto">Детали запроса <see cref="CompanyWithActiveEmailRequest"/></param>
		/// <returns></returns>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult GetCompanyList([FromBody] CompanyWithActiveEmailRequest dto)
		{
			var source = dto.Source.GetEnumDisplayName();
			
			_logger.LogInformation(
				"Поступил запрос на получение идентификатора юр лица с активной почтой {Email} от пользователя {ExternalUserId} {Source}",
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
						" от пользователя {ExternalUserId}:\n{ValidationResult}",
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
					"404" => Problem(error.Message, statusCode: int.Parse(error.Code)),
					_ => Problem(error.Message)
				};
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при получении идентификатора юр лиц с активной почтой {Email} от пользователя {ExternalUserId} {Source}",
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
		/// <param name="cancellationToken">Токен для отмены опреации</param>
		/// <returns></returns>
		[HttpGet]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public async Task<IActionResult> GetCompanyInfo([FromBody] CompanyInfoRequest dto, CancellationToken cancellationToken)
		{
			var source = dto.Source.GetEnumDisplayName();
			
			_logger.LogInformation(
				"Поступил запрос на получение данных о юр лице от пользователя {ExternalUserId} {Source}",
				dto.ExternalCounterpartyId,
				source);
			
			try
			{
				var validationResult = _requestDataValidator.CompanyInfoRequestDataValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при получении данных о юр лице от пользователя {ExternalUserId}:\n{ValidationResult}",
						dto.ExternalCounterpartyId,
						validationResult);
					return ValidationProblem(validationResult);
				}
				
				var result = await _legalCounterpartyService.GetCompanyInfo(dto, cancellationToken);

				if(result.IsSuccess)
				{
					return Ok(result.Value);
				}

				var error = result.Errors.First();

				return error.Code switch
				{
					"404" => Problem(error.Message, statusCode: int.Parse(error.Code)),
					_ => Problem(error.Message)
				};
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при получении данных о юр лице от пользователя {ExternalUserId} {Source}",
					dto.ExternalCounterpartyId,
					source);
				
				return Problem();
			}
		}
		
		/// <summary>
		/// Регистрация нового юр лица
		/// </summary>
		/// <param name="dto">Детали запроса <see cref="RegisteringLegalCustomerDto"/></param>
		/// <returns></returns>
		[HttpPost]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult RegisterLegalCustomer(RegisteringLegalCustomerDto dto)
		{
			var source = dto.Source.GetEnumDisplayName();
			
			_logger.LogInformation(
				"Поступил запрос на регистрацию нового юр лица с ИНН {INN} от пользователя: {ExternalUserId} с {Source}",
				dto.Inn,
				dto.ExternalCounterpartyId,
				source);
			
			try
			{
				var validationResult = _requestDataValidator.RegisteringLegalCustomerValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при регистрации нового юр лица от: {ExternalUserId}:\n{ValidationResult}",
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
					"Ошибка при регистрации нового юр лица с ИНН {INN} от: {ExternalUserId} с {Source}",
					dto.Inn,
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
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult LinkEmailToLegalCounterparty(LinkingLegalCounterpartyEmailToExternalUser dto)
		{
			_logger.LogInformation(
				"Поступил запрос на прикрепление электронки {Email}, к юрику с Id: {LegalId} от {ExternalUserId}",
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
						"Не прошли валидацию при прикреплении почты {Email}, к юрику с Id: {LegalId} от {ExternalUserId}:\n{ValidationResult}",
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
					"Ошибка при прикреплении телефона {Email}, к юрику с Id: {LegalId} от {ExternalUserId} с {Source}",
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
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult GetLegalCounterpartyContactList([FromBody] LegalCounterpartyContactListRequest dto)
		{
			var source = dto.Source.GetEnumDisplayName();
			_logger.LogInformation(
				"Поступил запрос на получение контактов, прикрепленных к юрику с Id: {LegalId} от пользователя {ExternalUserId} {Source}",
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
						"Не прошли валидацию при получении контактов, прикрепленных к юрику с Id: {LegalId} от пользователя {ExternalUserId}:\n{ValidationResult}",
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
					"Ошибка при получении контактов, прикрепленных к юрику с Id: {LegalId} от пользователя {ExternalUserId} {Source}",
					dto.ErpCounterpartyId,
					dto.ExternalCounterpartyId,
					source);
				return Problem();
			}
		}
	}
}
