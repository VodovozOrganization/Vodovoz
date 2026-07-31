using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services.ReceiptContactProblem
{
	public class ReceiptContactProblemNotificationService : IReceiptContactProblemNotificationService
	{
		private readonly IOutboxNotificationPublisher<EdoNotificationMessage> _notificationPublisher;
		private readonly IEdoNotificationMessageFactory _notificationMessageFactory;
		private readonly IReceiptContactProblemSourceProvider _problemSourceProvider;

		public ReceiptContactProblemNotificationService(
			IOutboxNotificationPublisher<EdoNotificationMessage> notificationPublisher,
			IEdoNotificationMessageFactory notificationMessageFactory,
			IReceiptContactProblemSourceProvider problemSourceProvider)
		{
			_notificationPublisher = notificationPublisher
				?? throw new ArgumentNullException(nameof(notificationPublisher));
			_notificationMessageFactory = notificationMessageFactory
				?? throw new ArgumentNullException(nameof(notificationMessageFactory));
			_problemSourceProvider = problemSourceProvider
				?? throw new ArgumentNullException(nameof(problemSourceProvider));
		}

		public Task<bool> TryNotifyAsync(
			IUnitOfWork unitOfWork,
			ReceiptEdoTask receiptTask,
			EdoTaskProblem problem,
			int orderId,
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

			var notification = _notificationMessageFactory.Create(
				EdoNotificationType.ReceiptContactInvalid,
				("ProblemName", _problemSourceProvider.GetNotificationName(problem.SourceName)),
				("OrderId", orderId.ToString(CultureInfo.InvariantCulture)),
				("EdoTaskId", receiptTask.Id.ToString(CultureInfo.InvariantCulture)),
				("ProblemId", problem.Id.ToString(CultureInfo.InvariantCulture)),
				("RetryCount", retryCount.ToString(CultureInfo.InvariantCulture)));

			return _notificationPublisher.TryPublishAsync(unitOfWork, notification, cancellationToken);
		}
	}
}
