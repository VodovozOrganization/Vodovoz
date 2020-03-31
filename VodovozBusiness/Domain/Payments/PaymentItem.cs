using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Payments
{
	public class PaymentItem : PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		Order order;

		[Display(Name = "Заказ")]
		public virtual Order Order {
			get => order;
			set => SetField(ref order, value);
		}

		Payment payment;
		[Display(Name = "Платеж")]
		public virtual Payment Payment {
			get => payment;
			set => SetField(ref payment, value);
		}

		CashlessIncomeOperation cashlessIncomeOperation;

		public virtual CashlessIncomeOperation CashlessIncomeOperation {
			get => cashlessIncomeOperation;
			set => SetField(ref cashlessIncomeOperation, value);
		}

		decimal sum;

		public virtual decimal Sum {
			get => sum;
			set => SetField(ref sum, value);
		}

		public PaymentItem()
		{
		}
	}
}
