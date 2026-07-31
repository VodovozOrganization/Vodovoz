using Edo.Problems.Validation;
using EdoNotifications.Application.Factories;
using EdoNotifications.Contracts;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
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
				("EdoTaskType", GetTaskTypeDisplayName(edoTask.TaskType)),
				("OrderId", edoTask.FormalEdoRequest.Order.Id.ToString(CultureInfo.InvariantCulture)),
				("ProblemSource", GetProblemSourceDisplayName(validator)),
				("ProblemMessage", validator.GetTemplatedMessage(edoTask)),
				("Recommendation", validator.Recommendation));
		}

		private static string GetProblemSourceDisplayName(IEdoTaskValidator validator)
		{
			var displayAttribute = validator.GetType()
				.GetProperty(nameof(validator.Name))?
				.GetCustomAttribute<DisplayAttribute>();

			return displayAttribute?.Name ?? validator.Description;
		}

		private static string GetTaskTypeDisplayName(EdoTaskType taskType)
		{
			var displayAttribute = typeof(EdoTaskType)
				.GetField(taskType.ToString())
				?.GetCustomAttribute<DisplayAttribute>();

			return displayAttribute?.Name ?? taskType.ToString();
		}
	}
}
