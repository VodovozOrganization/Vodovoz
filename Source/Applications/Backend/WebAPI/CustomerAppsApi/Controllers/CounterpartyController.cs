using System;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Counterparties;
using CustomerAppsApi.Library.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vodovoz.Extensions;

namespace CustomerAppsApi.Controllers
{
	[ApiController]
	[Route("/api/")]
	public class CounterpartyController : ControllerBase
	{
		private readonly ILogger<CounterpartyController> _logger;
		private readonly ICounterpartyModel _counterpartyModel;

		public CounterpartyController(
			ILogger<CounterpartyController> logger,
			ICounterpartyModel counterpartyModel)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_counterpartyModel = counterpartyModel ?? throw new ArgumentNullException(nameof(counterpartyModel));
		}

		[HttpPost]
		[Route("GetCounterparty")]
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
		
		[HttpPost]
		[Route("RegisterCounterparty")]
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
		
		[HttpPost]
		[Route("UpdateCounterpartyInfo")]
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
		
		[HttpGet]
		public IActionResult GetLegalCustomersByInn(GetLegalCustomersByInnDto dto)
		{
			_logger.LogInformation(
				"Поступил запрос на получение юр лиц по ИНН {INN} от {ExternalCounterpartyId}",
				dto.Inn,
				dto.ExternalCounterpartyId);
			
			try
			{
				var validationResult = _counterpartyModel.GetLegalCustomersDtoValidate(dto);
				
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

				if(result is null)
				{
					_logger.LogInformation(
						"Не нашли пользователя при получении юр лиц по ИНН {INN} от {ExternalCounterpartyId}",
						dto.Inn,
						dto.ExternalCounterpartyId);
					return BadRequest("Пользователь не найден");
				}
				
				return Ok(result);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при получении юр лиц по {INN} для {ExternalCounterpartyId}", dto.Inn, dto.ExternalCounterpartyId);
				return Problem();
			}
		}
		
		[HttpGet]
		public IActionResult GetLegalCustomers(GetNaturalCounterpartyLegalCustomersDto dto)
		{
			_logger.LogInformation(
				"Поступил запрос на получение юр лиц клиента Id: {CounterpartyId} от {ExternalCounterpartyId}",
				dto.ErpCounterpartyId,
				dto.ExternalCounterpartyId);
			
			try
			{
				var result = _counterpartyModel.GetLegalCustomers(dto);

				if(result is null)
				{
					_logger.LogInformation(
						"Не нашли пользователя при получении юр лиц клиента Id: {CounterpartyId} от {ExternalCounterpartyId}",
						dto.ErpCounterpartyId,
						dto.ExternalCounterpartyId);
					return BadRequest("Пользователь не найден");
				}
				
				return Ok(result);
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
			
		[HttpPost]
		public IActionResult RegisterLegalCustomer(RegisteringLegalCustomerDto dto)
		{
			_logger.LogInformation(
				"Поступил запрос на регистрацию нового юр лица от клиента Id: {CounterpartyId} с {Source}",
				dto.ErpCounterpartyId,
				dto.Source.GetEnumDisplayName());
			
			try
			{
				var validationResult = _counterpartyModel.RegisteringLegalCustomerValidate(dto);
				
				if(!string.IsNullOrWhiteSpace(validationResult))
				{
					_logger.LogInformation(
						"Не прошли валидацию при регистрации нового юр лица от клиента Id: {CounterpartyId} с {Source}:\n{ValidationResult}",
						dto.ErpCounterpartyId,
						dto.Source.GetEnumDisplayName(),
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
		
		[HttpPost]
		public IActionResult ConnectLegalCustomer(CounterpartyDto dto)
		{
			_logger.LogInformation(
				"Поступил запрос на прикрепление физика с Id {NaturalId} к юрику с Id: {LegalId} с {Source}",
				dto.ErpNaturalCounterpartyId,
				dto.ErpLegalCounterpartyId,
				dto.Source.GetEnumDisplayName());
			
			try
			{
				var result = _counterpartyModel.RegisterLegalCustomer(dto);
				
				if(!string.IsNullOrWhiteSpace(result.Message))
				{
					return BadRequest(result.Message);
				}
				
				return Ok(result.Data);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при регистрации юр лица от ");
				return Problem();
			}
		}
	}
}
