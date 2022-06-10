using System;
using QS.DomainModel.Entity;

namespace Vodovoz.Domain.Payments
{
	public class PaymentFromAvangard : PropertyChangedBase, IDomainObject
	{
		private DateTime _paidDate;
		private int _orderNum;
		private decimal _totalSum;
		
		public virtual int Id { get; set; }

		public virtual DateTime PaidDate
		{
			get => _paidDate;
			set => SetField(ref _paidDate, value);
		}
		
		public virtual int OrderNum
		{
			get => _orderNum;
			set => SetField(ref _orderNum, value);
		}
		
		public virtual decimal TotalSum
		{
			get => _totalSum;
			set => SetField(ref _totalSum, value);
		}
	}
}
