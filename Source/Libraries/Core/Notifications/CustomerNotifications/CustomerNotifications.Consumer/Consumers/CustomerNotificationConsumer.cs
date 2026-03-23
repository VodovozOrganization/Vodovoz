using CustomerNotifications.Consumer.Contracts;
using CustomerNotifications.Consumer.Services;
using CustomerNotifications.Contracts.Messages;
using CustomerNotifications.Contracts.Providers;
using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading.Tasks;
using Vodovoz.Domain.Orders;
using VodovozBusiness.Extensions;

namespace CustomerNotifications.Consumer.Consumers
{
	public class CustomerNotificationConsumer : IConsumer<CustomerNotificationMessage>
	{
		private readonly ILogger<CustomerNotificationConsumer> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOnlineOrdersStatusUpdatedNotificationService _onlineOrdersStatusUpdatedNotificationService;
		private readonly IOnlineOrderNotificationSettingsProvider _onlineOrderNotificationSettingsProvider;

		public CustomerNotificationConsumer(
			ILogger<CustomerNotificationConsumer> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOnlineOrdersStatusUpdatedNotificationService onlineOrdersStatusUpdatedNotificationService,
			IOnlineOrderNotificationSettingsProvider onlineOrderNotificationSettingsProvider)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_onlineOrdersStatusUpdatedNotificationService =
				onlineOrdersStatusUpdatedNotificationService ?? throw new ArgumentNullException(nameof(onlineOrdersStatusUpdatedNotificationService));
			_onlineOrderNotificationSettingsProvider =
				onlineOrderNotificationSettingsProvider ?? throw new ArgumentNullException(nameof(onlineOrderNotificationSettingsProvider));
		}

		public async Task Consume(ConsumeContext<CustomerNotificationMessage> context)
		{
			var message = context.Message;

			_logger.LogInformation(
				"Получено уведомление для онлайн заказа {OnlineOrderId}",
				message.OnlineOrderId);

			var httpCode = -1;
			var onlineOrderId = message.OnlineOrderId;

			try
			{
				OnlineOrder onlineOrder;
				OnlineOrderStatusUpdatedDto dto;

				using(var unitOfWork = _unitOfWorkFactory.CreateWithoutRoot(nameof(CustomerNotificationConsumer)))
				{
					onlineOrder = unitOfWork.GetById<OnlineOrder>(onlineOrderId);
					dto = GetOnlineOrderStatusUpdatedDto(unitOfWork, message, onlineOrder);
				}

				_logger.LogInformation("Отправляем данные в ИПЗ по онлайн заказу {OnlineOrderId}: {@Notification}",
					onlineOrderId,
					dto);

				httpCode = await _onlineOrdersStatusUpdatedNotificationService.NotifyOfOnlineOrderStatusUpdatedAsync(dto, onlineOrder.Source);

				_logger.LogInformation("Ответ по отправке уведомления по заказу {OnlineOrderId}: {HttpCode}",
					onlineOrderId,
					httpCode);
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при отправке уведомления об изменении статуса онлайн заказа {OnlineOrderId}",
					onlineOrderId);

				throw;
			}
		}

		private OnlineOrderStatusUpdatedDto GetOnlineOrderStatusUpdatedDto(
			IUnitOfWork unitOfWork,
			CustomerNotificationMessage message,
			OnlineOrder onlineOrder)
		{
			if(onlineOrder is null)
			{
				throw new InvalidOperationException($"Онлайн заказ с Id {onlineOrder.Id} не найден");
			}

			var notificationText = _onlineOrdersStatusUpdatedNotificationService.GetPushText(
				unitOfWork,
				_onlineOrderNotificationSettingsProvider,
				message,
				onlineOrder);

			return new OnlineOrderStatusUpdatedDto
			{
				ExternalOrderId = onlineOrder.ExternalOrderId,
				OnlineOrderId = onlineOrder.Id,
				DeliveryDate = onlineOrder.OnlineOrderStatus != OnlineOrderStatus.Canceled ? (DateTime?)onlineOrder.DeliveryDate : null,
				DeliveryScheduleId = onlineOrder.OnlineOrderStatus != OnlineOrderStatus.Canceled ? onlineOrder.DeliveryScheduleId : null,
				OrderStatus = onlineOrder.GetExternalOrderStatus(),
				PushText = notificationText
			};
		}
	}
}
