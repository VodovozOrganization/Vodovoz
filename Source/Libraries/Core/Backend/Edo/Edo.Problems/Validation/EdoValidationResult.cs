using System.Collections.Generic;
using Vodovoz.Core.Domain.Edo;
using EdoNotifications.Contracts;

namespace Edo.Problems.Validation
{
	public class EdoValidationResult
	{
		public static EdoValidationResult Valid(IEdoTaskValidator validator)
		{
			return new EdoValidationResult(validator)
			{
				IsValid = true
			};
		}

		public static EdoValidationResult Valid(IEdoTaskValidator validator, IEnumerable<EdoTaskItem> problemItems)
		{
			return new EdoValidationResult(validator)
			{
				IsValid = true,
				ProblemItems = problemItems
			};
		}

		public static EdoValidationResult Invalid(IEdoTaskValidator validator)
		{
			return new EdoValidationResult(validator)
			{
				IsValid = false
			};
		}

		public static EdoValidationResult Invalid(IEdoTaskValidator validator, IEnumerable<EdoTaskItem> problemItems)
		{
			return new EdoValidationResult(validator)
			{
				IsValid = false,
				ProblemItems = problemItems
			};
		}

		/// <summary>
		/// Создаёт результат неуспешной проверки с уведомлением о проблеме.
		/// </summary>
		/// <param name="validator">Валидатор, обнаруживший проблему.</param>
		/// <param name="notification">Уведомление для сохранения вместе с проблемой.</param>
		/// <returns>Результат проверки.</returns>
		public static EdoValidationResult InvalidWithNotification(IEdoTaskValidator validator, EdoNotificationMessage notification)
		{
			return new EdoValidationResult(validator)
			{
				IsValid = false,
				Notification = notification ?? throw new System.ArgumentNullException(nameof(notification))
			};
		}

		private EdoValidationResult(IEdoTaskValidator validator)
		{
			Validator = validator ?? throw new System.ArgumentNullException(nameof(validator));
		}

		public bool IsValid { get; private set; }
		/// <summary>
		/// Уведомление о проблеме, если проверка требует его отправки.
		/// </summary>
		public EdoNotificationMessage Notification { get; private set; }
		public IEdoTaskValidator Validator { get; private set; }
		public IEnumerable<EdoTaskItem> ProblemItems { get; private set; } = new List<EdoTaskItem>();
	}
}
