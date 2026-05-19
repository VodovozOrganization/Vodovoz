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
		private readonly IClientClosingDeliveriesEmailBuilder _сlientClosingDeliveriesEmailBuilder;
		private readonly ISummaryClosingDeliveriesEmailBuilder _summaryClosingDeliveriesEmailBuilder;
		private readonly IEmailSender _emailSender;

		public ClosingDeliveriesNotificationService(
			IClientClosingDeliveriesEmailBuilder сlientClosingDeliveriesEmailBuilder,
			ISummaryClosingDeliveriesEmailBuilder summaryClosingDeliveriesEmailBuilder,
			IEmailSender emailSender)
		{
			_сlientClosingDeliveriesEmailBuilder = сlientClosingDeliveriesEmailBuilder ?? throw new System.ArgumentNullException(nameof(сlientClosingDeliveriesEmailBuilder));
			_summaryClosingDeliveriesEmailBuilder = summaryClosingDeliveriesEmailBuilder ?? throw new System.ArgumentNullException(nameof(summaryClosingDeliveriesEmailBuilder));
			_emailSender = emailSender ?? throw new System.ArgumentNullException(nameof(emailSender));
		}

		public async Task SendNotifications(
			IUnitOfWork uow,
			IReadOnlyCollection<OrderWithoutShipmentForDebtNotificationInfo> notificationInfos,
			CancellationToken cancellationToken)
		{
			var clientSendEmailMessages = await _сlientClosingDeliveriesEmailBuilder.Build(
				uow,
				notificationInfos,
				cancellationToken);

			foreach(var sendEmailMessage in clientSendEmailMessages)
			{
				await _emailSender.Send(sendEmailMessage, cancellationToken);
			}

			var summarySendEmailMessages = await _summaryClosingDeliveriesEmailBuilder.Build(
				uow,
				notificationInfos,
				cancellationToken);

			foreach(var summarySendEmailMessage in summarySendEmailMessages)
			{
				await _emailSender.Send(summarySendEmailMessage, cancellationToken);
			}
		}
	}
}
