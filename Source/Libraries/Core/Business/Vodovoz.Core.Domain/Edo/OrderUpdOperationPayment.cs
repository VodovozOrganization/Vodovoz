using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Платеж в операции УПД по заказу
	/// </summary>
	[Appellative(Gender = GrammaticalGender.Feminine,
		Nominative = "платеж в операции УПД по заказу",
		NominativePlural = "платежи в операциях УПД по заказам"
	)]
	public class OrderUpdOperationPayment : PropertyChangedBase, IDomainObject
	{
		private int _id;
		private OrderUpdOperation _orderUpdOperation;
		private string _paymentNum;
		private DateTime _paymentDate;

		/// <summary>
		/// Код
		/// </summary>
		[Display(Name = "Код")]
		public virtual int Id
		{
			get => _id;
			set => SetField(ref _id, value);
		}

		/// <summary>
		/// Операция УПД по заказу
		/// </summary>
		[Display(Name = "Операция УПД по заказу")]
		public virtual OrderUpdOperation OrderUpdOperation
		{
			get => _orderUpdOperation;
			set => SetField(ref _orderUpdOperation, value);
		}

		/// <summary>
		/// Номер платежа
		/// </summary>
		[Display(Name = "Номер платежа")]
		public virtual string PaymentNum
		{
			get => _paymentNum;
			set => SetField(ref _paymentNum, value);
		}

		/// <summary>
		/// Дата платежа
		/// </summary>
		[Display(Name = "Дата платежа")]
		public virtual DateTime PaymentDate
		{
			get => _paymentDate;
			set => SetField(ref _paymentDate, value);
		}
	}
}
