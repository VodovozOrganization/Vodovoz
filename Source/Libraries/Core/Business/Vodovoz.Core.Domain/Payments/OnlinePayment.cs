using QS.DomainModel.Entity;
using QS.HistoryLog;
using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Orders;

namespace Vodovoz.Core.Domain.Payments
{
	/// <summary>
	/// Онлайн-платёж
	/// </summary>
	[Appellative(
		Gender = GrammaticalGender.Masculine,
		Nominative = "онлайн-платёж",
		NominativePlural = "онлайн-платежи"
	)]
	[HistoryTrace]
	public class OnlinePayment : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private int _externalId;
		private string _transactionId;
		private OnlinePaymentSource? _paymentSource;
		private DateTime _date;

		/// <summary>
		/// Идентификатор
		/// </summary>
		[Display(Name = "Идентификатор")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Идентификатор платежа 
		/// </summary>
		[Display(Name = "Идентификатор платежа из ИПЗ")]
		public virtual int ExternalId
		{
			get => _externalId;
			set => SetField(ref _externalId, value);
		}

		/// <summary>
		/// ID транзакции в платёжной системе
		/// </summary>
		[Display(Name = "ID транзакции")]
		public virtual string TransactionId
		{
			get => _transactionId;
			set => SetField(ref _transactionId, value);
		}

		/// <summary>
		/// Источник оплаты
		/// </summary>
		[Display(Name = "Источник оплаты")]
		public virtual OnlinePaymentSource? PaymentSource
		{
			get => _paymentSource;
			set => SetField(ref _paymentSource, value);
		}

		/// <summary>
		/// Дата создания
		/// </summary>
		[Display(Name = "Дата создания")]
		public virtual DateTime Date
		{
			get => _date;
			set => SetField(ref _date, value);
		}
	}
}
