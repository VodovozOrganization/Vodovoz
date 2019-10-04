using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.Sms
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "смс уведомления",
		Nominative = "смс уведомление")]
	[EntityPermission]
	[HistoryTrace]
	public abstract class SmsNotification : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public abstract SmsNotificationType SmsNotificationType { get; }

		private SmsNotificationStatus status;
		[Display(Name = "Статус")]
		public virtual SmsNotificationStatus Status {
			get => status;
			set => SetField(ref status, value, () => Status);
		}

		private string mobilePhone;
		[Display(Name = "Номер мобильного телефона")]
		public virtual string MobilePhone {
			get => mobilePhone;
			set => SetField(ref mobilePhone, value, () => MobilePhone);
		}

		private string messageText;
		[Display(Name = "Текст сообщения")]
		public virtual string MessageText {
			get => messageText;
			set => SetField(ref messageText, value, () => MessageText);
		}

		private string serverMessageId;
		[Display(Name = "Идентификатор сообщения на стороне сервера")]
		public virtual string ServerMessageId {
			get => serverMessageId;
			set => SetField(ref serverMessageId, value, () => ServerMessageId);
		}

		private string errorDescription;
		[Display(Name = "Описание ошибки")]
		public virtual string ErrorDescription {
			get => errorDescription;
			set => SetField(ref errorDescription, value, () => ErrorDescription);
		}

		private DateTime notifyTime;
		[Display(Name = "Время уведомления")]
		public virtual DateTime NotifyTime {
			get => notifyTime;
			set => SetField(ref notifyTime, value, () => NotifyTime);
		}

		private DateTime? expiredTime;
		/// <summary>
		/// Время после которого отправка уведомления будет не актуальна
		/// </summary>
		[Display(Name = "Время просрочки")]
		public virtual DateTime? ExpiredTime {
			get => expiredTime;
			set => SetField(ref expiredTime, value, () => ExpiredTime);
		}

		private string description;
		[Display(Name = "Описание")]
		public virtual string Description {
			get => description;
			set => SetField(ref description, value, () => Description);
		}
	}
}
