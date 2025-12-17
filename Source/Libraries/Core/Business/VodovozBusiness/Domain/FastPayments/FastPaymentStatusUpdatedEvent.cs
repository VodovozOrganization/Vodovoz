using System;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.FastPayments;

namespace Vodovoz.Domain.FastPayments
{
	/// <summary>
	/// Событие обновления статуса быстрого платежа
	/// </summary>
	public class FastPaymentStatusUpdatedEvent : IDomainObject
	{
		public virtual int Id { get; set; }
		/// <summary>
		/// Дата создания
		/// </summary>
		public DateTime CreateAt { get; protected set; }
		/// <summary>
		/// Быстрый платеж
		/// </summary>
		public virtual FastPayment FastPayment { get; set; }
		/// <summary>
		/// Статус платежа
		/// </summary>
		public virtual FastPaymentStatus FastPaymentStatus { get; set; }
		/// <summary>
		/// Код ответа принимающей стороны
		/// </summary>
		public virtual int? HttpCode { get; set; }
		/// <summary>
		/// Код ответа принимающей стороны
		/// </summary>
		public virtual bool DriverNotified { get; set; }

		public static FastPaymentStatusUpdatedEvent Create(FastPayment fastPayment, FastPaymentStatus fastPaymentStatus) =>
			new FastPaymentStatusUpdatedEvent
			{
				FastPayment = fastPayment,
				FastPaymentStatus = fastPaymentStatus,
				CreateAt = DateTime.Now
			};
	}
}
