using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

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
		private ExternalOrderStatus _externalOrderStatus;
		private string _notificationText;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => _id = value;
		}

		[Display(Name = "Статус онлайн заказа")]
		public virtual ExternalOrderStatus ExternalOrderStatus
		{
			get => _externalOrderStatus;
			set => SetField(ref _externalOrderStatus, value);
		}

		[Display(Name = "Текст уведомления")]
		public virtual string NotificationText
		{
			get => _notificationText;
			set => SetField(ref _notificationText, value);
		}

		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(string.IsNullOrEmpty(NotificationText) || NotificationText.Length > 255)
			{
				yield return new ValidationResult("Длина уведомления должна быть заполнена (не более 255 символов)",
					new[] { nameof(NotificationText) });
			}
		}

		public override string ToString() =>
			$"Настройка уведомления для онлайн заказов {Id}. {NotificationText} ({ExternalOrderStatus})";
	}
}
