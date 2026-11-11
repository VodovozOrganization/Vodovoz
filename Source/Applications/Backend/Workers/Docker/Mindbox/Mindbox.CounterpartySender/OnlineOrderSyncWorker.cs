using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mindbox.Library.Services;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Clients.Mindbox;
using Vodovoz.Domain.Orders;

namespace Mindbox.CounterpartySender
{
	/// <summary>
	/// Периодически ставит зарегистрированные в Mindbox онлайн-заказы в очередь проверки изменений
	/// </summary>
	public sealed class OnlineOrderSyncWorker : BackgroundService
	{
		private static readonly TimeSpan Delay = TimeSpan.FromMinutes(1);
		private const int BatchSize = 100;

		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<OnlineOrderSyncWorker> _logger;

		/// <summary>
		/// Создаёт воркер постановки онлайн-заказов в очередь Mindbox
		/// </summary>
		/// <param name="scopeFactory">Фабрика DI-областей</param>
		/// <param name="logger">Логгер</param>
		public OnlineOrderSyncWorker(
			IServiceScopeFactory scopeFactory,
			ILogger<OnlineOrderSyncWorker> logger)
		{
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		/// <summary>
		/// Запускает цикл постановки изменённых заказов в очередь
		/// </summary>
		/// <param name="stoppingToken">Токен остановки воркера</param>
		/// <returns>Задача работы воркера</returns>
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var scope = _scopeFactory.CreateScope();
					var unitOfWorkFactory = scope.ServiceProvider.GetRequiredService<IUnitOfWorkFactory>();
					var enqueuer = scope.ServiceProvider.GetRequiredService<IMindboxOnlineOrderEnqueuer>();

					using var unitOfWork = unitOfWorkFactory.CreateWithoutRoot("Постановка онлайн-заказов в очередь Mindbox");
					var orders = unitOfWork.Session.Query<OnlineOrder>()
						.Where(order => order.MindboxStatus == MindboxCounterpartyOutboxStatus.Sent)
						.OrderBy(order => order.Id)
						.Take(BatchSize)
						.ToList();

					foreach(var order in orders)
					{
						await enqueuer.EnqueueAsync(
							unitOfWork,
							order,
							order.MindboxLastChangeAt ?? DateTime.UtcNow,
							stoppingToken).ConfigureAwait(false);
					}

					if(orders.Count > 0)
					{
						await unitOfWork.CommitAsync(stoppingToken).ConfigureAwait(false);
					}
				}
				catch(OperationCanceledException)
				{
					break;
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка постановки онлайн-заказов в очередь Mindbox");
				}

				try
				{
					await Task.Delay(Delay, stoppingToken).ConfigureAwait(false);
				}
				catch(OperationCanceledException)
				{
					break;
				}
			}
		}
	}
}
