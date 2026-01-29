using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppNotifier.Options;
using CustomerAppNotifier.Services;
using CustomerAppsApi.Library.Dto.Counterparties;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QS.DomainModel.UoW;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Zabbix.Sender;
using VodovozBusiness.EntityRepositories.Counterparties;

namespace CustomerAppNotifier
{
	public class CustomerAppEventsSender : BackgroundService
	{
		private readonly ILogger<CustomerAppEventsSender> _logger;
		private readonly IConfiguration _configuration;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IExternalCounterpartyAssignNotificationRepository _externalCounterpartyAssignNotificationRepository;
		private readonly ILogoutLegalAccountEventRepository _logoutEventsRepository;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IOptionsMonitor<LogoutEventSendScheduleOptions> _logoutEventsOptionsMonitor;
		private readonly IZabbixSender _zabbixSender;
		private const int _delayInSec = 10;

		public CustomerAppEventsSender(
			IUserService userService,
			ILogger<CustomerAppEventsSender> logger,
			IConfiguration configuration,
			IUnitOfWorkFactory unitOfWorkFactory,
			IExternalCounterpartyAssignNotificationRepository externalCounterpartyAssignNotificationRepository,
			ILogoutLegalAccountEventRepository logoutEventsRepository,
			IServiceScopeFactory serviceScopeFactory,
			IOptionsMonitor<LogoutEventSendScheduleOptions> logoutEventsOptionsMonitor,
			IZabbixSender zabbixSender)
		{
			_logger = logger;
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_externalCounterpartyAssignNotificationRepository =
				externalCounterpartyAssignNotificationRepository
				?? throw new ArgumentNullException(nameof(externalCounterpartyAssignNotificationRepository));
			_logoutEventsRepository = logoutEventsRepository ?? throw new ArgumentNullException(nameof(logoutEventsRepository));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_logoutEventsOptionsMonitor = logoutEventsOptionsMonitor ?? throw new ArgumentNullException(nameof(logoutEventsOptionsMonitor));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
				try
				{
					var pastDaysForSend = _configuration.GetValue<int>("PastDaysForSend");
					await CounterpartyAssignNotifyAsync(pastDaysForSend, stoppingToken);
					await SendLogoutEvents(stoppingToken);
				}
				catch(Exception e)
				{
					_logger.LogError(e, "Произошла ошибка при обработке отправляемых событий в ИПЗ");
				}
				
				await Task.Delay(1000 * _delayInSec, stoppingToken);
			}
		}

		private async Task CounterpartyAssignNotifyAsync(int pastDaysForSend, CancellationToken stoppingToken)
		{
			_logger.LogInformation("Запущен метод отправки уведомлений");

			using var uow = _unitOfWorkFactory.CreateWithoutRoot();
			_logger.LogInformation("Получение списка уведомлений для отправки");

			var notificationsToSend =
				_externalCounterpartyAssignNotificationRepository.GetNotificationsForSend(uow, pastDaysForSend);

			_logger.LogInformation("Подготовка к отправке");

			using(var scope = _serviceScopeFactory.CreateScope())
			{
				var notificationService = scope.ServiceProvider.GetService<INotificationService>();
					
				foreach(var notification in notificationsToSend)
				{
					var httpCode = -1;
					try
					{
						_logger.LogInformation("Отправляем данные в ИПЗ");
						httpCode = await notificationService.NotifyOfCounterpartyAssignAsync(
							GetRegisteredNaturalCounterpartyDto(notification), notification.ExternalCounterparty.CounterpartyFrom);

						_logger.LogInformation("Данные отправлены");
					}
					catch(Exception e)
					{
						_logger.LogError(e, "Ошибка при отправке уведомления о ручном сопоставлении клиента в ИПЗ");
					}

					UpdateNotification(uow, notification, httpCode);
				}
			}

			await _zabbixSender.SendIsHealthyAsync(stoppingToken);
		}

		private RegisteredNaturalCounterpartyDto GetRegisteredNaturalCounterpartyDto(ExternalCounterpartyAssignNotification notification)
		{
			var counterparty = notification.ExternalCounterparty.Phone.Counterparty;

			return new RegisteredNaturalCounterpartyDto
			{
				ErpCounterpartyId = counterparty.Id,
				Email = notification.ExternalCounterparty.Email?.Address,
				ExternalCounterpartyId = notification.ExternalCounterparty.ExternalCounterpartyId,
				FirstName = counterparty.FirstName,
				Surname = counterparty.Surname,
				Patronymic = counterparty.Patronymic,
				PhoneNumber = $"+7{notification.ExternalCounterparty.Phone.DigitsNumber}"
			};
		}
		
		private void UpdateNotification(IUnitOfWork uow, ExternalCounterpartyAssignNotification notification, int httpCode)
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
				_logger.LogError(e,"Ошибка при обновлении уведомления ИПЗ");
			}
		}
		
		private async Task SendLogoutEvents(CancellationToken stoppingToken)
		{
			_logger.LogInformation("Запущен метод отправки событий разлогинивания");
			using var uow = _unitOfWorkFactory.CreateWithoutRoot();
			var eventsToSend = _logoutEventsRepository.GetLogoutLegalAccountEventsToSend(uow);

			using var scope = _serviceScopeFactory.CreateScope();
			var notificationService = scope.ServiceProvider.GetService<INotificationService>();

			_logger.LogInformation("Всего на отправку: {EventsCount} событий разлогинивания", eventsToSend.Count());
			foreach(var eventToSend in eventsToSend)
			{
				foreach(var sentData in eventToSend.SentData)
				{
					if(sentData.Delivered)
					{
						continue;
					}
					
					if(sentData.LastSentDateTime.HasValue)
					{
						var sendSchedule = _logoutEventsOptionsMonitor.CurrentValue;
						
						if(sendSchedule is null || !sendSchedule.Any())
						{
							_logger.LogCritical("Нет данных по какому графику отправлять события для разлогинивания!!!");
							throw new InvalidOperationException();
						}

						var timeSec = TryGetTimeValueToSendLogoutEvent(sendSchedule, sentData.SentEventsCount);
						if(timeSec == 0)
						{
							_logger.LogCritical("Неправильно сделана настройка повторных отправок!!!");
							throw new InvalidOperationException();
						}

						if((DateTime.Now - sentData.LastSentDateTime.Value).TotalSeconds < timeSec)
						{
							continue;
						}
					}
					
					sentData.Delivered = await notificationService.SendLogoutEventAsync(eventToSend, sentData.Source);
					_logger.LogInformation("Отправили событие разлогинивания в ИПЗ {Source} {SentResult}", sentData.Source, sentData.Delivered);
					sentData.SentEventsCount += 1;
					sentData.LastSentDateTime = DateTime.Now;
					
					await uow.SaveAsync(sentData, cancellationToken: stoppingToken);
					await uow.CommitAsync(stoppingToken);
				}
			}
		}

		private int TryGetTimeValueToSendLogoutEvent(Dictionary<string, int> sendScheduleSendsSchedule, int sentDataSentEventsCount)
		{
			if(sentDataSentEventsCount == 0)
			{
				sentDataSentEventsCount = 1;
			}

			if(sendScheduleSendsSchedule.TryGetValue(sentDataSentEventsCount.ToString(), out var value))
			{
				return value;
			}
			
			return 0;
		}
	}
}
