using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.FastPayments
{
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		NominativePlural = "уведомления о быстрых платежах",
		Nominative = "уведомление о быстром платеже")]
	public class FastPaymentNotification : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private DateTime _time;
		private FastPayment _payment;
		private FastPaymentNotificationType _type;
		private DateTime? _lastTryTime;
		private bool _successfullyNotified;
		private bool _stopNotifications;

		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		[Display(Name = "Время уведомления")]
		public virtual DateTime Time
		{
			get => _time;
			set => SetField(ref _time, value);
		}

		[Display(Name = "Платеж")]
		public virtual FastPayment Payment
		{
			get => _payment;
			set => SetField(ref _payment, value);
		}

		[Display(Name = "Тип")]
		public virtual FastPaymentNotificationType Type
		{
			get => _type;
			set => SetField(ref _type, value);
		}

		[Display(Name = "Время последней попытки")]
		public virtual DateTime? LastTryTime
		{
			get => _lastTryTime;
			set => SetField(ref _lastTryTime, value);
		}

		[Display(Name = "Уведомлено успешно")]
		public virtual bool SuccessfullyNotified
		{
			get => _successfullyNotified;
			set => SetField(ref _successfullyNotified, value);
		}

		[Display(Name = "Уведомления остановлены")]
		public virtual bool StopNotifications
		{
			get => _stopNotifications;
			set => SetField(ref _stopNotifications, value);
		}
	}
}
