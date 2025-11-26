using System;
using System.Linq;
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
		private readonly ICounterpartyModel _counterpartyModel;

		public CounterpartyController(
			ILogger<CounterpartyController> logger,
			ICounterpartyModel counterpartyModel)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
	}
}
