using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerOnlineOrdersStatusUpdateNotifier.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;

namespace CustomerOnlineOrdersStatusUpdateNotifier
{
	public class OnlineOrderTemplateChangedNotifier : BackgroundService
	{
		private readonly ILogger<OnlineOrderTemplateChangedNotifier> _logger;
		private readonly IOptionsMonitor<> _options;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;

		public OnlineOrderTemplateChangedNotifier(
			ILogger<OnlineOrderTemplateChangedNotifier> logger,
			IOptionsMonitor<> options,
			IUnitOfWorkFactory unitOfWorkFactory,
			IServiceScopeFactory serviceScopeFactory
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
		}
		
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

				try
				{
					await NotifyAsync(stoppingToken);
					await _zabbixSender.SendIsHealthyAsync(stoppingToken);
				}
				catch
				{
					throw;
				}
				
				await Task.Delay(TimeSpan.FromSeconds(_options.CurrentValue.DelayInSeconds), stoppingToken);
			}
		}

		private async Task NotifyAsync(CancellationToken stoppingToken)
		{
			using var uow = _unitOfWorkFactory.CreateWithoutRoot("Отправка уведомлений об изменившихся шаблонах автозаказов");
			_logger.LogInformation("Получение списка уведомлений для отправки");

			var currentOptions = _options.CurrentValue;
			var notificationsToSend =
				_notificationRepository.GetNotificationsForSend(
					uow, currentOptions.PastDaysForSend, currentOptions.NotificationCountInSession);

			if(!notificationsToSend.Any())
			{
				return;
			}

			_logger.LogInformation("Подготовка к отправке. Всего {NotificationsCount}", notificationsToSend.Count());

			using(var scope = _serviceScopeFactory.CreateScope())
			{
				var notificationService = scope.ServiceProvider.GetService<IOnlineOrdersStatusUpdatedNotificationService>();
					
				foreach(var notification in notificationsToSend)
				{
					var httpCode = -1;
					var onlineOrderId = notification.OnlineOrder.Id;
						
					try
					{
						var dto = GetOnlineOrderStatusUpdatedDto(uow, notificationService, notification);

						_logger.LogInformation("Отправляем данные в ИПЗ по онлайн заказу {OnlineOrderId}: {@Notification}",
							onlineOrderId,
							dto);

						httpCode = await notificationService.NotifyOfOnlineOrderStatusUpdatedAsync(dto, notification.OnlineOrder.Source);
							
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
					}

					UpdateNotification(uow, notification, httpCode);
				}
			}
		}
	}
}
