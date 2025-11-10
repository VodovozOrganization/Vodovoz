using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerOnlineOrdersStatusUpdateNotifier.Configs;
using CustomerOnlineOrdersStatusUpdateNotifier.Contracts;
using CustomerOnlineOrdersStatusUpdateNotifier.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using QS.Services;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Zabbix.Sender;
using VodovozBusiness.Extensions;

namespace CustomerOnlineOrdersStatusUpdateNotifier
{
	public class OnlineOrdersStatusUpdatedNotifier : BackgroundService
	{
		private readonly ILogger<OnlineOrdersStatusUpdatedNotifier> _logger;
		private readonly IConfiguration _configuration;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOnlineOrderStatusUpdatedNotificationRepository _notificationRepository;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IOptionsMonitor<NotifierOptions> _options;
		private readonly IZabbixSender _zabbixSender;

		public OnlineOrdersStatusUpdatedNotifier(
			IUserService userService,
			ILogger<OnlineOrdersStatusUpdatedNotifier> logger,
			IConfiguration configuration,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOnlineOrderStatusUpdatedNotificationRepository notificationRepository,
			IServiceScopeFactory serviceScopeFactory,
			IOptionsMonitor<NotifierOptions> options,
			IZabbixSender zabbixSender
			)
		{
			_logger = logger;
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_options = options ?? throw new ArgumentNullException(nameof(options));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

				try
				{
					await NotifyAsync();

					await _zabbixSender.SendIsHealthyAsync(stoppingToken);
				}
				catch
				{
					throw;
				}
				
				await Task.Delay(TimeSpan.FromSeconds(_options.CurrentValue.DelayInSeconds), stoppingToken);
			}
		}

		private async Task NotifyAsync()
		{
			_logger.LogInformation("Запущен метод отправки уведомлений");

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				_logger.LogInformation("Получение списка уведомлений для отправки");

				var currentOptions = _options.CurrentValue;
				var notificationsToSend =
					_notificationRepository.GetNotificationsForSend(
						uow, currentOptions.PastDaysForSend, currentOptions.NotificationCountInSession);

				if(!notificationsToSend.Any())
				{
					return;
				}

				_logger.LogInformation("Подготовка к отправке");

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

		private OnlineOrderStatusUpdatedDto GetOnlineOrderStatusUpdatedDto(
			IUnitOfWork uow,
			IOnlineOrdersStatusUpdatedNotificationService notificationService,
			OnlineOrderStatusUpdatedNotification notification)
		{
			var onlineOrder = notification.OnlineOrder;

			var notificationText = notificationService.GetPushText(
				uow,
				_notificationRepository,
				onlineOrder.GetExternalOrderStatus(),
				onlineOrder.Id,
				onlineOrder.DeliverySchedule?.From);

			return new OnlineOrderStatusUpdatedDto
			{
				ExternalOrderId = onlineOrder.ExternalOrderId,
				OnlineOrderId = onlineOrder.Id,
				DeliveryDate = onlineOrder.OnlineOrderStatus != OnlineOrderStatus.Canceled ? onlineOrder.DeliveryDate : null,
				DeliveryScheduleId = onlineOrder.OnlineOrderStatus != OnlineOrderStatus.Canceled ? onlineOrder.DeliveryScheduleId : null,
				OrderStatus = onlineOrder.GetExternalOrderStatus(),
				PushText = notificationText
			};
		}
		
		private void UpdateNotification(IUnitOfWork uow, OnlineOrderStatusUpdatedNotification notification, int httpCode)
		{
			_logger.LogInformation("Обновляем данные");
			try
			{
				notification.HttpCode = httpCode;
				notification.SentDate = DateTime.Now;
				uow.Save(notification);
				uow.Commit();

				_logger.LogInformation("Данные обновлены");
			}
			catch(Exception e)
			{
				_logger.LogError(
					e,
					"Ошибка при обновлении уведомления ИПЗ по онлайн заказу {OnlineOrderId}",
					notification.OnlineOrder.Id);
			}
		}
	}
}
