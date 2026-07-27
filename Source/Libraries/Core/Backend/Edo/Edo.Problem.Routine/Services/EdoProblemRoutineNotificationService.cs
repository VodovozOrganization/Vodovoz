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
	public class EdoProblemRoutineNotificationService
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IOutboxNotificationPublisher<EdoNotificationMessage> _notificationPublisher;
		private readonly EdoProblemRoutineNotificationFactory _notificationFactory;

		public EdoProblemRoutineNotificationService(
			IUnitOfWorkFactory unitOfWorkFactory,
			IOutboxNotificationPublisher<EdoNotificationMessage> notificationPublisher,
			EdoProblemRoutineNotificationFactory notificationFactory)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_notificationPublisher = notificationPublisher
				?? throw new ArgumentNullException(nameof(notificationPublisher));
			_notificationFactory = notificationFactory
				?? throw new ArgumentNullException(nameof(notificationFactory));
		}

		public async Task<bool> NotifyAsync(
			OrderEdoTask edoTask,
			EdoNotificationType notificationType,
			IEdoTaskValidator validator,
			CancellationToken cancellationToken)
		{
			var notification = _notificationFactory.Create(
				edoTask,
				notificationType,
				validator);

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot(nameof(EdoProblemRoutineNotificationService)))
			{
				var published = await _notificationPublisher.TryPublishAsync(uow, notification, cancellationToken);

				if(published)
				{
					await uow.CommitAsync(cancellationToken);
				}

				return published;
			}
		}
	}
}
