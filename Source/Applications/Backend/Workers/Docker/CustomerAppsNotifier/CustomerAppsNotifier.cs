using System;
using System.Threading;
using System.Threading.Tasks;
using CustomerAppsApi.Library.Dto.Counterparties;
using ExternalCounterpartyAssignNotifier.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Services;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;

namespace CustomerAppsNotifier
{
	public class CustomerAppsNotifier : BackgroundService
	{
		private readonly ILogger<CustomerAppsNotifier> _logger;
		private readonly IConfiguration _configuration;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IExternalSourceNotificationRepository _externalSourceNotificationRepository;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private const int _delayInSec = 20;

		public CustomerAppsNotifier(
			IUserService userService,
			ILogger<CustomerAppsNotifier> logger,
			IConfiguration configuration,
			IUnitOfWorkFactory unitOfWorkFactory,
			IExternalSourceNotificationRepository externalSourceNotificationRepository,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger;
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_externalSourceNotificationRepository =
				externalSourceNotificationRepository
				?? throw new ArgumentNullException(nameof(externalSourceNotificationRepository));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
				var pastDaysForSend = _configuration.GetValue<int>("PastDaysForSend");
				await NotifyAboutAssignedCounterpartyAsync(pastDaysForSend);
				await NotifyAboutDeletedExternalCounterpartiesAsync(pastDaysForSend);
				await NotifyAboutAssignedCounterpartyAsync(pastDaysForSend);
				await Task.Delay(1000 * _delayInSec, stoppingToken);
			}
		}

		private async Task NotifyAboutAssignedCounterpartyAsync(int pastDaysForSend)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var notificationsToSend =
					_externalSourceNotificationRepository.GetAssignedExternalCounterpartiesNotifications(uow, pastDaysForSend);

				using(var scope = _serviceScopeFactory.CreateScope())
				{
					var notificationService = scope.ServiceProvider.GetService<INotificationService>();
					
					foreach(var notification in notificationsToSend)
					{
						var httpCode = -1;
						try
						{
							_logger.LogInformation("Отправляем данные в ИПЗ");
							
							if(string.IsNullOrWhiteSpace(notification.PhoneNumber))
							{
								httpCode = 0;
							}
							else
							{
								httpCode = await notificationService.NotifyOfCounterpartyAssignAsync(
									GetRegisteredNaturalCounterpartyDto(notification), notification.CounterpartyFrom);
							}
						}
						catch(Exception e)
						{
							_logger.LogError(e, "Ошибка при отправке уведомления о ручном сопоставлении клиента в ИПЗ");
						}

						UpdateNotification(uow, notification, httpCode);
					}
				}
			}
		}
		
		private async Task NotifyAboutDeletedExternalCounterpartiesAsync(int pastDaysForSend)
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var notificationsToSend =
					_externalSourceNotificationRepository.GetDeletedExternalCounterpartiesNotifications(uow, pastDaysForSend);

				using(var scope = _serviceScopeFactory.CreateScope())
				{
					var notificationService = scope.ServiceProvider.GetService<INotificationService>();
					
					foreach(var notification in notificationsToSend)
					{
						var httpCode = -1;
						try
						{
							_logger.LogInformation("Отправляем данные в ИПЗ");
							httpCode = await notificationService.NotifyOfExternalCounterpartyDeleteAsync(
								DeletedExternalCounterparty.Create(notification.ExternalCounterpartyId, notification.ErpCounterpartyId),
								notification.CounterpartyFrom);
						}
						catch(Exception e)
						{
							_logger.LogError(e, "Ошибка при отправке уведомления об удаленном пользователе");
						}

						UpdateNotification(uow, notification, httpCode);
					}
				}
			}
		}

		private RegisteredNaturalCounterpartyDto GetRegisteredNaturalCounterpartyDto(ExternalCounterpartyAssignNotification notification)
		{
			return RegisteredNaturalCounterpartyDto.Create(
				notification.Counterparty,
				notification.ExternalCounterpartyId,
				notification.Email,
				notification.PhoneNumber);
		}
		
		private void UpdateNotification(IUnitOfWork uow, INotification notification, int httpCode)
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
