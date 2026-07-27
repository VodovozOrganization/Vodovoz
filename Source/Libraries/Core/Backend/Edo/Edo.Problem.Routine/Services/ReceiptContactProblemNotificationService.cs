using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using EdoNotifications.Contracts;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services
{
	public class ReceiptContactProblemNotificationService : IReceiptContactProblemNotificationService
	{
		private readonly IOutboxNotificationPublisher<EdoNotificationMessage> _notificationPublisher;

		public ReceiptContactProblemNotificationService(
			IOutboxNotificationPublisher<EdoNotificationMessage> notificationPublisher)
		{
			_notificationPublisher = notificationPublisher
				?? throw new ArgumentNullException(nameof(notificationPublisher));
		}

		public Task<bool> TryNotifyAsync(
			IUnitOfWork unitOfWork,
			ReceiptEdoTask receiptTask,
			EdoTaskProblem problem,
			int retryCount,
			CancellationToken cancellationToken)
		{
			if(unitOfWork == null)
			{
				throw new ArgumentNullException(nameof(unitOfWork));
			}

			if(receiptTask == null)
			{
				throw new ArgumentNullException(nameof(receiptTask));
			}

			if(problem == null)
			{
				throw new ArgumentNullException(nameof(problem));
			}

			var notification = EdoNotificationMessage.Create(
				EdoNotificationType.ReceiptContactInvalid,
				("OrderId", receiptTask.FormalEdoRequest.Order.Id.ToString(CultureInfo.InvariantCulture)),
				("EdoTaskId", receiptTask.Id.ToString(CultureInfo.InvariantCulture)),
				("ProblemId", problem.Id.ToString(CultureInfo.InvariantCulture)),
				("RetryCount", retryCount.ToString(CultureInfo.InvariantCulture)));

			return _notificationPublisher.TryPublishAsync(unitOfWork, notification, cancellationToken);
		}
	}
}
