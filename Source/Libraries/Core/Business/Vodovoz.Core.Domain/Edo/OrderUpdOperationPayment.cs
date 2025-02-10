using QS.DomainModel.Entity;
using System;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Core.Domain.Edo
{
	public class OrderUpdOperationPayment : PropertyChangedBase, IDomainObject
	{
		private int _id;
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
