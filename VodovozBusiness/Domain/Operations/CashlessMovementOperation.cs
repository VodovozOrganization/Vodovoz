using QS.DomainModel.Entity;
using Vodovoz.Domain.Payments;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Operations
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "передвижения безнала",
		Nominative = "передвижение безнала")]
	public class CashlessMovementOperation : OperationBase
	{
		PaymentItem paymentItem;
		[Display(Name = "Строка платежа")]
		public virtual PaymentItem PaymentItem {
			get => paymentItem;
			set => SetField(ref paymentItem, value);
		}

		Payment payment;
		[Display(Name = "Платеж")]
		public virtual Payment Payment {
			get => payment;
			set => SetField(ref payment, value);
		}

		decimal income;
		[Display(Name = "Приход")]
		public virtual decimal Income {
			get => income;
			set => SetField(ref income, value);
		}

		decimal expense;
		[Display(Name = "Расход")]
		public virtual decimal Expense {
			get => expense;
			set => SetField(ref expense, value);
		}

		public CashlessMovementOperation()
		{
		}
	}
}
