using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.Operations;

namespace CustomerAppsApi.Repositories
{
	public class CachedBottlesDebtRepository : ICachedBottlesDebtRepository
	{
		private readonly ILogger<CachedBottlesDebtRepository> _logger;
		private readonly IBottlesRepository _bottlesRepository;
		private readonly IDistributedCache _distributedCache;

		public CachedBottlesDebtRepository(
			ILogger<CachedBottlesDebtRepository> logger,
			IBottlesRepository bottlesRepository,
			IDistributedCache distributedCache)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_bottlesRepository = bottlesRepository ?? throw new ArgumentNullException(nameof(bottlesRepository));
			_distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
		}

		public async Task<int> GetCounterpartyBottlesDebt(IUnitOfWork uow, int counterpartyId, CancellationToken cancellationToken = default)
		{
			string bottlesDebt = null;

			try
			{
				bottlesDebt = await _distributedCache.GetStringAsync(counterpartyId.ToString(), cancellationToken);
			}
			catch
			{
				_logger.LogError("Не удалось получить данные из кэша, отправляем данные из БД");
				return _bottlesRepository.GetBottlesDebtAtCounterparty(uow, counterpartyId);
			}
			
			var result = default(int);
			
			if(string.IsNullOrWhiteSpace(bottlesDebt))
			{
				_logger.LogInformation("Получаем данные из БД");
				result = _bottlesRepository.GetBottlesDebtAtCounterparty(uow, counterpartyId);

				_logger.LogInformation("Обновляем кэш");
				await _distributedCache.SetStringAsync(
					counterpartyId.ToString(),
					result.ToString(),
					new DistributedCacheEntryOptions
					{
						AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
					},
					cancellationToken
				);
			}
			else
			{
				_logger.LogInformation("Получаем данные из кэша");
				result = int.Parse(bottlesDebt);
			}
			
			return result;
		}
	}
}
