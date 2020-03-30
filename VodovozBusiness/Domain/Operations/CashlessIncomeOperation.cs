using Vodovoz.Domain.Client;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Payments;

namespace Vodovoz.Domain.Operations
{
	[Appellative(Gender = GrammaticalGender.Neuter,
		NominativePlural = "передвижения безнала",
		Nominative = "передвижение безнала")]
	public class CashlessIncomeOperation : OperationBase
	{
		Counterparty counterparty;

		public virtual Counterparty Counterparty {
			get => counterparty;
			set => SetField(ref counterparty, value);
		}

		Payment payment;

		public virtual Payment Payment {
			get => payment;
			set => SetField(ref payment, value);
		}

		decimal sum;

		public virtual decimal Sum {
			get => sum;
			set => SetField(ref sum, value);
		}

		public CashlessIncomeOperation()
		{
		}
	}
}
