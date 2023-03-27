using System;
using System.Threading;
using System.Threading.Tasks;
using ExternalCounterpartyAssignNotifier.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Counterparties;

namespace ExternalCounterpartyAssignNotifier
{
	public class ExternalCounterpartyAssignNotifier : BackgroundService
	{
		private readonly ILogger<ExternalCounterpartyAssignNotifier> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IExternalCounterpartyAssignNotificationRepository _externalCounterpartyAssignNotificationRepository;
		private readonly INotificationService _notificationService;
		private const int _delayInSec = 20;

		public ExternalCounterpartyAssignNotifier(
			ILogger<ExternalCounterpartyAssignNotifier> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IExternalCounterpartyAssignNotificationRepository externalCounterpartyAssignNotificationRepository,
			INotificationService notificationService)
		{
			_logger = logger;
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_externalCounterpartyAssignNotificationRepository =
				externalCounterpartyAssignNotificationRepository
				?? throw new ArgumentNullException(nameof(externalCounterpartyAssignNotificationRepository));
			_notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while(!stoppingToken.IsCancellationRequested)
			{
				_logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
				await NotifyAsync();
				await Task.Delay(1000 * _delayInSec, stoppingToken);
			}
		}

		private async Task NotifyAsync()
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var notificationsToSend = _externalCounterpartyAssignNotificationRepository.GetNotificationsForSend(uow, 3);

				foreach(var notification in notificationsToSend)
				{
					var httpCode = await _notificationService.NotifyOfCounterpartyAssignAsync(
						GetRegisteredNaturalCounterpartyDto(notification), notification.ExternalCounterparty.CounterpartyFrom);

					notification.HttpCode = httpCode;
					uow.Save(notification);
					uow.Commit();
				}
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
				Patronymic = counterparty.Patronymic
			};
		}
	}
	
	public class RegisteredNaturalCounterpartyDto
	{
		public Guid ExternalCounterpartyId { get; set; }
		public int ErpCounterpartyId { get; set; }
		public string FirstName { get; set; }
		public string Surname { get; set; }
		public string Patronymic { get; set; }
		public string Email { get; set; }
	}
}
