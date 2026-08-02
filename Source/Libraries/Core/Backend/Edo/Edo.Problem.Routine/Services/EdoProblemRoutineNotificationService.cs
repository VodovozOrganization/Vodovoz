using Edo.Problems.Validation;
using EdoNotifications.Contracts;
using Notifications.Infrastructure;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services
{
	public class EdoProblemRoutineNotificationService : IEdoProblemRoutineNotificationService
	{
		private readonly IOutboxNotificationPublisher<EdoNotificationMessage> _notificationPublisher;
		private readonly EdoProblemRoutineNotificationFactory _notificationFactory;

		public EdoProblemRoutineNotificationService(
			IOutboxNotificationPublisher<EdoNotificationMessage> notificationPublisher,
			EdoProblemRoutineNotificationFactory notificationFactory)
		{
			_notificationPublisher = notificationPublisher
				?? throw new ArgumentNullException(nameof(notificationPublisher));
			_notificationFactory = notificationFactory
				?? throw new ArgumentNullException(nameof(notificationFactory));
		}

		public Task<bool> NotifyAsync(
			IUnitOfWork unitOfWork,
			OrderEdoTask edoTask,
			EdoNotificationType notificationType,
			IEdoTaskValidator validator,
			CancellationToken cancellationToken)
		{
			if(unitOfWork == null)
			{
				throw new ArgumentNullException(nameof(unitOfWork));
			}

			var notification = _notificationFactory.Create(
				edoTask,
				notificationType,
				validator);

			return _notificationPublisher.TryPublishAsync(
				unitOfWork,
				notification,
				cancellationToken);
		}
	}
}
