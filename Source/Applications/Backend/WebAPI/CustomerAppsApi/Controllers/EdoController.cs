using System;
using System.Linq;
using System.Net.Mime;
using CustomerAppsApi.Library.Dto.Edo;
using CustomerAppsApi.Library.Services;
using CustomerAppsApi.Library.Validators;
using Microsoft.AspNetCore.Http;
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
		private readonly ILogger<EdoController> _logger;
		private readonly ICounterpartyRequestDataValidator _requestDataValidator;
		private readonly CustomerAppEdoService _customerAppEdoService;

		public EdoController(
			ILogger<EdoController> logger,
			ICounterpartyRequestDataValidator requestDataValidator,
			CustomerAppEdoService customerAppEdoService
		)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_requestDataValidator = requestDataValidator ?? throw new ArgumentNullException(nameof(requestDataValidator));
			_customerAppEdoService = customerAppEdoService ?? throw new ArgumentNullException(nameof(customerAppEdoService));
		}
		
		/// <summary>
		/// Добавление ЭДО аккаунта
		/// </summary>
		/// <param name="dto">Детали запроса <see cref="AddingEdoAccount"/></param>
		/// <returns></returns>
		[HttpPost]
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		[ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult AddEdoAccount(AddingEdoAccount dto)
		{
			var source = dto.Source.GetEnumDisplayName();
			
			_logger.LogInformation(
				"Поступил запрос на добавление ЭДО аккаунта {EdoAccount} клиенту {CounterpartyId} от пользователя: {ExternalCounterpartyId} с {Source}",
				dto.EdoAccount,
				dto.ErpCounterpartyId,
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
						dto.ErpCounterpartyId,
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
					dto.ErpCounterpartyId,
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
		[Produces(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status500InternalServerError)]
		public IActionResult GetEdoOperators([FromBody] GetEdoOperatorsRequest request)
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
				
				return Ok(_customerAppEdoService.GetEdoOperators());
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
