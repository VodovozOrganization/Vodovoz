using EmailDebtNotificationWorker.Builders;
using EmailDebtNotificationWorker.DTO;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmailDebtNotificationWorker.Services
{
	public class ClosingDeliveriesNotificationService : IClosingDeliveriesNotificationService
	{
		private readonly IClientClosingDeliveriesEmailBuilder _clientDebtEmailBuilder;
		private readonly ISummaryClosingDeliveriesEmailBuilder _summaryClosingDeliveriesEmailBuilder;
		private readonly IEmailSender _emailSender;

		public ClosingDeliveriesNotificationService(
			IClientClosingDeliveriesEmailBuilder clientDebtEmailBuilder,
			ISummaryClosingDeliveriesEmailBuilder summaryClosingDeliveriesEmailBuilder,
			IEmailSender emailSender)
		{
			_clientDebtEmailBuilder = clientDebtEmailBuilder ?? throw new System.ArgumentNullException(nameof(clientDebtEmailBuilder));
			_summaryClosingDeliveriesEmailBuilder = summaryClosingDeliveriesEmailBuilder ?? throw new System.ArgumentNullException(nameof(summaryClosingDeliveriesEmailBuilder));
			_emailSender = emailSender ?? throw new System.ArgumentNullException(nameof(emailSender));
		}

		public async Task SendNotifications(
			IUnitOfWork uow,
			IReadOnlyCollection<OrderWithoutShipmentForDebtNotificationInfo> notificationInfos,
			CancellationToken cancellationToken)
		{
			var clientEmailMessages = await _clientDebtEmailBuilder.Build(
				uow,
				notificationInfos,
				cancellationToken);

			foreach(var email in clientEmailMessages)
			{
				await _emailSender.Send(email, cancellationToken);
			}

			var managerEmailMessages = await _summaryClosingDeliveriesEmailBuilder.Build(
				uow,
				notificationInfos,
				cancellationToken);

			foreach(var managerMessage in managerEmailMessages)
			{
				await _emailSender.Send(managerMessage, cancellationToken);
			}
		}
	}
}
