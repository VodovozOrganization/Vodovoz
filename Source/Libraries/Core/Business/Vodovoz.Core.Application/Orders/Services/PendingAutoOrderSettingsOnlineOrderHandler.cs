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
					TransferToManualProcessing(onlineOrder, onlineOrderTimers);
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

		private void TransferToManualProcessing(OnlineOrder onlineOrder, OnlineOrderTimers orderTimers)
		{
			if(TryMoveToManualProcessingWithoutTemplate(onlineOrder, orderTimers))
			{
				_logger.LogInformation("Переводим на ручное онлайн заказ №{PendingAutoOrderSettingsOrderId}", onlineOrder.Id);
			}
		}
		
		private bool TryMoveToManualProcessingWithoutTemplate(
			OnlineOrder onlineOrder,
			OnlineOrderTimers orderTimers)
		{
			if(onlineOrder.Template != null && onlineOrder.OnlineOrderStatus == OnlineOrderStatus.WaitingForPayment)
			{
				_logger.LogWarning(
					"Почему-то заказ с шаблоном, но остался в статусе {OnlineOrderStatus}! Переводим на ручное",
					onlineOrder.OnlineOrderStatus
					);
				
				onlineOrder.OnlineOrderStatus = OnlineOrderStatus.New;
				return true;
			}
			
			if((DateTime.Now - onlineOrder.Created).TotalSeconds >= orderTimers.TimeForTransferAutoOrderToManualProcessingFromSettings.TotalSeconds)
			{
				onlineOrder.OnlineOrderStatus = OnlineOrderStatus.New;
				return true;
			}
			
			return false;
		}
	}
}
