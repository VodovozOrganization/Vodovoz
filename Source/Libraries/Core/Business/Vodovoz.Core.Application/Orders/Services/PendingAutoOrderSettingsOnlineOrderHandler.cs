using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using VodovozBusiness.Services.Orders;

namespace Vodovoz.Core.Application.Orders.Services
{
	public class PendingAutoOrderSettingsOnlineOrderHandler : IPendingAutoOrderSettingsOnlineOrderHandler
	{
		private readonly ILogger<PendingAutoOrderSettingsOnlineOrderHandler> _logger;
		private readonly IOnlineOrderRepository _onlineOrderRepository;

		public PendingAutoOrderSettingsOnlineOrderHandler(
			ILogger<PendingAutoOrderSettingsOnlineOrderHandler> logger,
			IOnlineOrderRepository onlineOrderRepository
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
		}
		
		public async Task TryMoveToManualProcessingPendingAutoOrderSettingsOnlineOrders(
			IUnitOfWork uow,
			CancellationToken cancellationToken
			)
		{
			_logger.LogInformation("Проверяем онлайн заказы, ожидающих настройку автозаказа...");

			try
			{
				var pendingAutoOrderSettingsOrders = _onlineOrderRepository.GetPendingAutoOrderSettingsOnlineOrders(uow);
				_logger.LogInformation("Найдено {PendingAutoOrderSettingsOrdersCount} онлайн заказов", pendingAutoOrderSettingsOrders.Count());

				if(!pendingAutoOrderSettingsOrders.Any())
				{
					return;
				}

				var onlineOrderTimers = uow.GetAll<OnlineOrderTimers>().FirstOrDefault();

				if(onlineOrderTimers is null)
				{
					_logger.LogWarning("Не найдены таймеры онлайн заказов");
					return;
				}

				foreach(var onlineOrder in pendingAutoOrderSettingsOrders)
				{
					await TransferToManualProcessingAsync(uow, onlineOrder, onlineOrderTimers, cancellationToken);
					await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken);
				}

				_logger.LogInformation("Сохраняем обработанные заказы");
				await uow.CommitAsync(cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке онлайн заказов, ожидающих настройку автозаказа");
			}
		}

		private async Task TransferToManualProcessingAsync(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			OnlineOrderTimers orderTimers,
			CancellationToken cancellationToken
			)
		{
			if(await TryMoveToManualProcessingWithoutTemplateAsync(uow, onlineOrder, orderTimers, cancellationToken))
			{
				_logger.LogInformation("Переводим на ручное онлайн заказ №{PendingAutoOrderSettingsOrderId}", onlineOrder.Id);
				await uow.CommitAsync(cancellationToken);
			}
		}
		
		private async Task<bool> TryMoveToManualProcessingWithoutTemplateAsync(
			IUnitOfWork uow,
			OnlineOrder onlineOrder,
			OnlineOrderTimers orderTimers,
			CancellationToken cancellationToken
			)
		{
			var timeToTransfer = orderTimers.TimeForTransferAutoOrderToManualProcessingFromSettings;
			
			if((DateTime.Now - onlineOrder.Created).TotalSeconds >= timeToTransfer.TotalSeconds)
			{
				onlineOrder.OnlineOrderStatus = OnlineOrderStatus.New;

				if(onlineOrder.Template != null)
				{
					_logger.LogInformation(
						"Почему-то онлайн заказ остался в настройке автозаказа, хотя он уже прикреплен к нему");
					
					var template = onlineOrder.Template;
					await uow.SaveAsync(template, cancellationToken: cancellationToken);
				}
				
				await uow.SaveAsync(onlineOrder, cancellationToken: cancellationToken);
				return true;
			}
			
			return false;
		}
	}
}
