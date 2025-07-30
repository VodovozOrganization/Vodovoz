using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Interfaces.Orders;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;

namespace Vodovoz.Application.Orders.Services
{
	public class UnPaidOnlineOrderHandler : IUnPaidOnlineOrderHandler
	{
		private readonly ILogger<UnPaidOnlineOrderHandler> _logger;
		private readonly IUnitOfWork _uow;
		private readonly IOnlineOrderRepository _onlineOrderRepository;

		public UnPaidOnlineOrderHandler(
			ILogger<UnPaidOnlineOrderHandler> logger,
			IUnitOfWork uow,
			IOnlineOrderRepository onlineOrderRepository
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_onlineOrderRepository = onlineOrderRepository ?? throw new ArgumentNullException(nameof(onlineOrderRepository));
		}

		public async Task TryMoveToManualProcessingWaitingForPaymentOnlineOrders()
		{
			_logger.LogInformation("Проверяем онлайн заказы, ожидающих оплаты...");

			try
			{
				var waitingForPaymentOnlineOrders = _onlineOrderRepository.GetWaitingForPaymentOnlineOrders(_uow);
				_logger.LogInformation("Найдено {WaitingForPaymentCount} онлайн заказов", waitingForPaymentOnlineOrders.Count());

				if(!waitingForPaymentOnlineOrders.Any())
				{
					return;
				}

				var onlineOrderTimers = _uow.GetAll<OnlineOrderTimers>().FirstOrDefault();

				if(onlineOrderTimers is null)
				{
					_logger.LogWarning("Не найдены таймеры онлайн заказов");
					return;
				}

				foreach(var onlineOrder in waitingForPaymentOnlineOrders)
				{
					TransferToManualProcessing(onlineOrder,
						onlineOrder.IsFastDelivery
							? onlineOrderTimers.TimeForTransferToManualProcessingWithFastDelivery
							: onlineOrderTimers.TimeForTransferToManualProcessingWithoutFastDelivery);

					await _uow.SaveAsync(onlineOrder);
				}

				_logger.LogInformation("Сохраняем обработанные заказы");
				await _uow.CommitAsync();
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обработке онлайн заказов, ожидающих оплаты");
			}
		}

		private void TransferToManualProcessing(
			Domain.Orders.OnlineOrder onlineOrder,
			TimeSpan timeForTransfer)
		{
			if((DateTime.Now - onlineOrder.Created).TotalSeconds >= timeForTransfer.TotalSeconds)
			{
				_logger.LogInformation("Переводим на ручное онлайн заказ №{WaitingForPaymentOrderId}", onlineOrder.Id);
				onlineOrder.MoveToManualProcessing("Заказ не был оплачен в отведенный срок");
			}
		}
	}
}
