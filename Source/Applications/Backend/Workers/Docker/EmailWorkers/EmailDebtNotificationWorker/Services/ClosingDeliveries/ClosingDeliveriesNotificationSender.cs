using EmailDebtNotificationWorker.DTO;
using MassTransit;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EmailDebtNotificationWorker.Services.ClosingDeliveries
{
	public class ClosingDeliveriesNotificationSender : IClosingDeliveriesNotificationSender
	{
		private readonly IClientClosingDeliveriesEmailPreparer _сlientClosingDeliveriesEmailPreparer;
		private readonly ISummaryClosingDeliveriesEmailPreparer _summaryClosingDeliveriesEmailPreparer;
		private readonly IBus _bus;

		public ClosingDeliveriesNotificationSender(
			IClientClosingDeliveriesEmailPreparer сlientClosingDeliveriesEmailPreparer,
			ISummaryClosingDeliveriesEmailPreparer summaryClosingDeliveriesEmailPreparer,
			IBus bus)
		{
			_сlientClosingDeliveriesEmailPreparer = сlientClosingDeliveriesEmailPreparer ?? throw new System.ArgumentNullException(nameof(сlientClosingDeliveriesEmailPreparer));
			_summaryClosingDeliveriesEmailPreparer = summaryClosingDeliveriesEmailPreparer ?? throw new System.ArgumentNullException(nameof(summaryClosingDeliveriesEmailPreparer));
			_bus = bus ?? throw new System.ArgumentNullException(nameof(bus));
		}

		public async Task SendNotifications(
			IUnitOfWork uow,
			IReadOnlyCollection<OrderWithoutShipmentForDebtNotificationInfo> notificationInfos,
			CancellationToken cancellationToken)
		{
			var clientSendEmailMessages = await _сlientClosingDeliveriesEmailPreparer.PrepareClientEmails(
				uow,
				notificationInfos,
				cancellationToken);

			foreach(var clientSendEmailMessage in clientSendEmailMessages)
			{
				await _bus.Publish(clientSendEmailMessage, cancellationToken);
			}

			var summarySendEmailMessages = await _summaryClosingDeliveriesEmailPreparer.PrepareSummaryEmails(
				uow,
				notificationInfos,
				cancellationToken);

			foreach(var summarySendEmailMessage in summarySendEmailMessages)
			{
				await _bus.Publish(summarySendEmailMessage, cancellationToken);
			}
		}
	}
}
