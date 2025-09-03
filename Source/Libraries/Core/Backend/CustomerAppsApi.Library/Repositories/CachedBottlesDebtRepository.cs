using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Operations;

namespace CustomerAppsApi.Library.Repositories
{
	public class CachedBottlesDebtRepository : ICachedBottlesDebtRepository
	{
		private readonly ILogger<CachedBottlesDebtRepository> _logger;
		private readonly IBottlesRepository _bottlesRepository;
		private readonly IMemoryCache _memoryCache;

		public CachedBottlesDebtRepository(
			ILogger<CachedBottlesDebtRepository> logger,
			IBottlesRepository bottlesRepository,
			IMemoryCache memoryCache)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_bottlesRepository = bottlesRepository ?? throw new ArgumentNullException(nameof(bottlesRepository));
			_memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
		}

		public int GetCounterpartyBottlesDebt(IUnitOfWork uow, int counterpartyId, int counterpartyDebtCacheMinutes)
		{
			var bottlesDebt = 0;
			var counterparty = new Counterparty
			{
				Id = counterpartyId
			};

			try
			{
				if(_memoryCache.TryGetValue(counterpartyId, out bottlesDebt))
				{
					_logger.LogInformation("Получили данные из кэша по клиенту {CounterpartyId}", counterpartyId);
					return bottlesDebt;
				}
			}
			catch
			{
				_logger.LogError(
					"Не удалось получить данные из кэша по клиенту {CounterpartyId}, отправляем данные из БД",
					counterpartyId);
				return _bottlesRepository.GetBottlesDebtAtCounterparty(uow, counterparty);
			}

			_logger.LogInformation("Получаем данные по клиенту {CounterpartyId} из БД", counterpartyId);
			bottlesDebt = _bottlesRepository.GetBottlesDebtAtCounterparty(uow, counterparty);

			_logger.LogInformation("Обновляем кэш по клиенту {CounterpartyId}", counterpartyId);
			_memoryCache.Set(
				counterpartyId,
				bottlesDebt,
				TimeSpan.FromMinutes(counterpartyDebtCacheMinutes));
			
			return bottlesDebt;
		}
	}
}
