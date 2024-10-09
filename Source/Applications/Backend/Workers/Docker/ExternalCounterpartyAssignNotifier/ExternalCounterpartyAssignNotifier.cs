using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.Dto;
using CustomerAppsApi.Library.Dto.Counterparties;
using ExternalCounterpartyAssignNotifier.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Services;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;
using Vodovoz.Zabbix.Sender;

namespace ExternalCounterpartyAssignNotifier
{
	public class ExternalCounterpartyAssignNotifier : BackgroundService
	{
		private readonly ILogger<ExternalCounterpartyAssignNotifier> _logger;
		private readonly IConfiguration _configuration;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IExternalCounterpartyAssignNotificationRepository _externalCounterpartyAssignNotificationRepository;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IZabbixSender _zabbixSender;
		private const int _delayInSec = 20;

		public ExternalCounterpartyAssignNotifier(
			IUserService userService,
			ILogger<ExternalCounterpartyAssignNotifier> logger,
			IConfiguration configuration,
			IUnitOfWorkFactory unitOfWorkFactory,
			IExternalCounterpartyAssignNotificationRepository externalCounterpartyAssignNotificationRepository,
			IServiceScopeFactory serviceScopeFactory,
			IZabbixSender zabbixSender)
		{
			_logger = logger;
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_externalCounterpartyAssignNotificationRepository =
				externalCounterpartyAssignNotificationRepository
				?? throw new ArgumentNullException(nameof(externalCounterpartyAssignNotificationRepository));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
			_zabbixSender = zabbixSender ?? throw new ArgumentNullException(nameof(zabbixSender));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
				var pastDaysForSend = _configuration.GetValue<int>("PastDaysForSend");
				await NotifyAsync(pastDaysForSend, stoppingToken);
				await Task.Delay(1000 * _delayInSec, stoppingToken);
			}
		}

		private async Task NotifyAsync(int pastDaysForSend, CancellationToken stoppingToken)
		{
			_logger.LogInformation("Запущен метод отправки уведомлений");

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
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
	}
}
