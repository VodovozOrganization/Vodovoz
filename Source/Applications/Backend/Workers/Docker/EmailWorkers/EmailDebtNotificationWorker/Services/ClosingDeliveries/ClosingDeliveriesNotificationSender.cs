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
		// UoW и IPublishEndpoint из одного DI-scope (прохода воркера): письма сохраняются в outbox при коммите этого UoW
		private readonly IUnitOfWork _unitOfWork;
		private readonly IClientClosingDeliveriesEmailPreparer _сlientClosingDeliveriesEmailPreparer;
		private readonly ISummaryClosingDeliveriesEmailPreparer _summaryClosingDeliveriesEmailPreparer;
		private readonly IPublishEndpoint _publishEndpoint;

		public ClosingDeliveriesNotificationSender(
			IUnitOfWork unitOfWork,
			IClientClosingDeliveriesEmailPreparer сlientClosingDeliveriesEmailPreparer,
			ISummaryClosingDeliveriesEmailPreparer summaryClosingDeliveriesEmailPreparer,
			IPublishEndpoint publishEndpoint)
		{
			_unitOfWork = unitOfWork ?? throw new System.ArgumentNullException(nameof(unitOfWork));
			_сlientClosingDeliveriesEmailPreparer = сlientClosingDeliveriesEmailPreparer ?? throw new System.ArgumentNullException(nameof(сlientClosingDeliveriesEmailPreparer));
			_summaryClosingDeliveriesEmailPreparer = summaryClosingDeliveriesEmailPreparer ?? throw new System.ArgumentNullException(nameof(summaryClosingDeliveriesEmailPreparer));
			_publishEndpoint = publishEndpoint ?? throw new System.ArgumentNullException(nameof(publishEndpoint));
		}

		public async Task SendNotifications(
			IReadOnlyCollection<OrderWithoutShipmentForDebtNotificationInfo> notificationInfos,
			CancellationToken cancellationToken)
		{
			// Каждому клиенту — свой коммит: запись о письме и письмо в outbox сохраняются вместе.
			// Письма с вложениями крупные, сотни их в одной транзакции не помещаются в пакет MySQL,
			// а сбой одного письма откатил бы все остальные
			foreach(var notificationInfo in notificationInfos)
			{
				var clientSendEmailMessages = await _сlientClosingDeliveriesEmailPreparer.PrepareClientEmails(
					_unitOfWork,
					new[] { notificationInfo },
					cancellationToken);

				foreach(var clientSendEmailMessage in clientSendEmailMessages)
				{
					await _publishEndpoint.Publish(clientSendEmailMessage, cancellationToken);
				}

				await _unitOfWork.CommitAsync(cancellationToken);
			}

			var summarySendEmailMessages = await _summaryClosingDeliveriesEmailPreparer.PrepareSummaryEmails(
				_unitOfWork,
				notificationInfos,
				cancellationToken);

			foreach(var summarySendEmailMessage in summarySendEmailMessages)
			{
				await _publishEndpoint.Publish(summarySendEmailMessage, cancellationToken);
			}

			await _unitOfWork.CommitAsync(cancellationToken);
		}
	}
}
