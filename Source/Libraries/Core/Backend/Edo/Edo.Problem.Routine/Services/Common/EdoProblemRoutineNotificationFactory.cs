using Edo.Problems.Validation;
using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using System;
using System.Globalization;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problem.Routine.Services.Common
{
	public class EdoProblemRoutineNotificationFactory
	{
		private readonly IEdoNotificationMessageFactory _notificationMessageFactory;

		public EdoProblemRoutineNotificationFactory(
			IEdoNotificationMessageFactory notificationMessageFactory)
		{
			_notificationMessageFactory = notificationMessageFactory
				?? throw new ArgumentNullException(nameof(notificationMessageFactory));
		}

		public EdoNotificationMessage Create(
			OrderEdoTask edoTask,
			EdoNotificationType notificationType,
			IEdoTaskValidator validator)
		{
			if(edoTask == null)
			{
				throw new ArgumentNullException(nameof(edoTask));
			}

			if(validator == null)
			{
				throw new ArgumentNullException(nameof(validator));
			}

			return _notificationMessageFactory.Create(
				notificationType,
				("EdoTaskId", edoTask.Id.ToString(CultureInfo.InvariantCulture)),
				("EdoTaskType", edoTask.TaskType.ToString()),
				("OrderId", edoTask.FormalEdoRequest.Order.Id.ToString(CultureInfo.InvariantCulture)),
				("ProblemSource", validator.Name),
				("ProblemMessage", validator.GetTemplatedMessage(edoTask)),
				("Recommendation", validator.Recommendation));
		}
	}
}
