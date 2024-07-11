﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerOnlineOrdersStatusUpdateNotifier.Contracts;
using CustomerOnlineOrdersStatusUpdateNotifier.Converters;
using CustomerOnlineOrdersStatusUpdateNotifier.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Services;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.Zabbix.Sender;

namespace CustomerOnlineOrdersStatusUpdateNotifier
{
	public class OnlineOrdersStatusUpdatedNotifier : BackgroundService
	{
		private readonly ILogger<OnlineOrdersStatusUpdatedNotifier> _logger;
		private readonly IConfiguration _configuration;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOnlineOrderStatusUpdatedNotificationRepository _notificationRepository;
		private readonly IExternalOrderStatusConverter _externalOrderStatusConverter;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IZabbixSender _zabbixSender;
		private int _delayInSec = 10;

		public OnlineOrdersStatusUpdatedNotifier(
			IUserService userService,
			ILogger<OnlineOrdersStatusUpdatedNotifier> logger,
			IConfiguration configuration,
			IUnitOfWorkFactory unitOfWorkFactory,
			IOnlineOrderStatusUpdatedNotificationRepository notificationRepository,
			IExternalOrderStatusConverter externalOrderStatusConverter,
			IServiceScopeFactory serviceScopeFactory,
			IZabbixSender zabbixSender
			)
		{
			_logger = logger;
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
			_externalOrderStatusConverter =
				externalOrderStatusConverter ?? throw new ArgumentNullException(nameof(externalOrderStatusConverter));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
				var pastDaysForSend = _configuration.GetValue<int>("PastDaysForSend");

				try
				{
					await NotifyAsync(pastDaysForSend);

					await _zabbixSender.SendIsHealthyAsync(stoppingToken);
				}
				catch
				{
					throw;
				}
				
				await Task.Delay(1000 * _delayInSec, stoppingToken);
			}
		}

		private async Task NotifyAsync(int pastDaysForSend)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var notificationsToSend =
					_notificationRepository.GetNotificationsForSend(uow, pastDaysForSend);

				if(!notificationsToSend.Any())
				{
					return;
				}

				using(var scope = _serviceScopeFactory.CreateScope())
				{
					var notificationService = scope.ServiceProvider.GetService<IOnlineOrdersStatusUpdatedNotificationService>();
					
					foreach(var notification in notificationsToSend)
					{
						var httpCode = -1;
						var onlineOrderId = notification.OnlineOrder.Id;
						
						try
						{
							_logger.LogInformation("Отправляем данные в ИПЗ по онлайн заказу {OnlineOrderId}", onlineOrderId);
							httpCode = await notificationService.NotifyOfOnlineOrderStatusUpdatedAsync(
								GetOnlineOrderStatusUpdatedDto(notification), notification.OnlineOrder.Source);
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

		private OnlineOrderStatusUpdatedDto GetOnlineOrderStatusUpdatedDto(OnlineOrderStatusUpdatedNotification notification)
		{
			var onlineOrder = notification.OnlineOrder;

			return new OnlineOrderStatusUpdatedDto
			{
				ExternalOrderId = onlineOrder.ExternalOrderId,
				OnlineOrderId = onlineOrder.Id,
				OrderId = onlineOrder.Order?.Id,
				DeliveryDate = onlineOrder.OnlineOrderStatus != OnlineOrderStatus.Canceled ? onlineOrder.DeliveryDate : null,
				DeliveryScheduleId = onlineOrder.OnlineOrderStatus != OnlineOrderStatus.Canceled ? onlineOrder.DeliveryScheduleId : null,
				OrderStatus = _externalOrderStatusConverter.GetExternalOrderStatus(onlineOrder)
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
			}
			catch(Exception e)
			{
				_logger.LogError(e,"Ошибка при обновлении уведомления ИПЗ");
			}
		}
	}
}
