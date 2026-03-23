using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders.OrderEnums;

namespace Vodovoz.Core.Domain.Orders
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "настройки уведомлений для онлайн заказов",
		Nominative = "настройка уведомления для онлайн заказов")]
	[EntityPermission]
	[HistoryTrace]
	public class OnlineOrderNotificationSetting : PropertyChangedBase, IDomainObject, IValidatableObject
	{
		private int _id;
		private string _notificationText;
		private CustomerNotificationClassification _notificationClassification;
		private CustomerNotificationEventType _customerNotificationEventType;
		private bool _notificationDisabled;
		private bool _allowDuplicateNotifications;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Тип события для уведомления клиента")]
		public virtual CustomerNotificationEventType CustomerNotificationEventType
		{
			get => _customerNotificationEventType;
			set => SetField(ref _customerNotificationEventType, value);
		}

		[Display(Name = "Текст уведомления")]
		public virtual string NotificationText
		{
			get => _notificationText;
			set => SetField(ref _notificationText, value);
		}

		[Display(Name = "Классификация уведомления")]
		public virtual CustomerNotificationClassification NotificationClassification
		{
			get => _notificationClassification;
			set => SetField(ref _notificationClassification, value);
		}

		[Display(Name = "Не отправлять")]
		public virtual bool NotificationDisabled
		{
			get => _notificationDisabled;
			set => SetField(ref _notificationDisabled, value);
		}

		[Display(Name = "Разрешить повторные отправки")]
		public virtual bool AllowDuplicateNotifications
		{
			get => _allowDuplicateNotifications;
			set => SetField(ref _allowDuplicateNotifications, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(NotificationText) || NotificationText.Length > 255)
			{
				yield return new ValidationResult("Текст уведомления должен быть заполнен (не более 255 символов)",
					new[] { nameof(NotificationText) });
			}
		}

		public override string ToString() =>
			$"Настройка уведомления для онлайн заказов {Id}. {NotificationText} ({CustomerNotificationEventType})";
	}
}
