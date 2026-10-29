using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.V2.Dto.Counterparties;
using CustomerAppsApi.Library.V2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VodovozHealthCheck.Helpers;

namespace CustomerAppsApi.V2.Controllers
{
	[Authorize]
	[ApiVersion("2.0")]
	public class CounterpartyController : VersionedController
	{
		private readonly ICounterpartyModel _counterpartyModel;
		private readonly ICounterpartyIdentificationService _counterpartyIdentificationService;

		public CounterpartyController(
			ILogger<CounterpartyController> logger,
			ICounterpartyModel counterpartyModel,
			ICounterpartyIdentificationService counterpartyIdentificationService) : base(logger)
		{
			_counterpartyModel = counterpartyModel ?? throw new ArgumentNullException(nameof(counterpartyModel));
			_counterpartyIdentificationService =
				counterpartyIdentificationService ?? throw new ArgumentNullException(nameof(counterpartyIdentificationService));
		}

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
		
		[HttpPost]
		public CounterpartyRegistrationDto RegisterCounterparty(CounterpartyDto counterpartyDto)
		{
			try
			{
				var isDryRun = HttpResponseHelper.IsHealthCheckRequest(Request);
				
				return _counterpartyModel.RegisterCounterparty(counterpartyDto, isDryRun);
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
		public CounterpartyUpdateDto UpdateCounterpartyInfo(CounterpartyDto counterpartyDto)
		{
			try
			{
				var isDryRun = HttpResponseHelper.IsHealthCheckRequest(Request);
				
				return _counterpartyModel.UpdateCounterpartyInfo(counterpartyDto, isDryRun);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при обновлении данных контрагента");
				throw;
			}
		}

		/// <summary>
		/// Поиск контрагента по номеру телефона
		/// </summary>
		/// <param name="request">Номер телефона в любом формате</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат поиска</returns>
		[HttpPost]
		public async Task<CounterpartySearchResultDto> SearchCounterpartiesByPhone(
			CounterpartyPhoneSearchDto request,
			CancellationToken cancellationToken)
		{
			try
			{
				return await _counterpartyIdentificationService.SearchByPhone(request, cancellationToken);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при поиске контрагента по телефону");
				throw;
			}
		}

		/// <summary>
		/// Поиск контрагента по ФИО
		/// </summary>
		/// <param name="request">ФИО</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат поиска</returns>
		[HttpPost]
		public async Task<CounterpartySearchResultDto> SearchCounterpartiesByName(
			CounterpartyNameSearchDto request,
			CancellationToken cancellationToken)
		{
			try
			{
				return await _counterpartyIdentificationService.SearchByName(request, cancellationToken);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при поиске контрагента по ФИО");
				throw;
			}
		}

		/// <summary>
		/// Поиск контрагента по адресу
		/// </summary>
		/// <param name="request">Адрес с обязательным указанием помещения</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Результат поиска</returns>
		[HttpPost]
		public async Task<CounterpartySearchResultDto> SearchCounterpartiesByAddress(
			CounterpartyAddressSearchDto request,
			CancellationToken cancellationToken)
		{
			try
			{
				return await _counterpartyIdentificationService.SearchByAddress(request, cancellationToken);
			}
			catch(Exception e)
			{
				_logger.LogError(e, "Ошибка при поиске контрагента по адресу");
				throw;
			}
		}

		/// <summary>
		/// Получение истории заказов контрагента
		/// </summary>
		/// <param name="request">Идентификатор контрагента</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>История заказов</returns>
		[HttpPost]
		public async Task<CounterpartyOrderHistoryDto> GetCounterpartyOrderHistory(
			CounterpartyOrderHistoryRequestDto request,
			CancellationToken cancellationToken)
		{
			try
			{
				return await _counterpartyIdentificationService.GetOrderHistory(request, cancellationToken);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при получении истории заказов контрагента {CounterpartyId}",
					request?.ErpCounterpartyId);
				throw;
			}
		}
	}
}
