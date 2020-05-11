using QS.DomainModel.Entity;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Operations
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "передвижения безнала",
		Nominative = "передвижение безнала")]
	public class CashlessMovementOperation : OperationBase
	{
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
