using System;
using QSOrmProject;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Operations
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "передвижения денег",
		Nominative = "передвижение денег")]
	public class MoneyMovementOperation: OperationBase
	{
		Order order;

		public virtual Order Order {
			get { return order; }
			set { SetField (ref order, value, () => Order); }
		}

		Counterparty counterparty;

		public virtual Counterparty Counterparty {
			get { return counterparty; }
			set { SetField (ref counterparty, value, () => Counterparty); }
		}

		Decimal? debt;

		public virtual Decimal? Debt {
			get { return debt; }
			set { SetField (ref debt, value, () => Debt); }
		}

		Decimal? money;

		public virtual Decimal? Money {
			get { return money; }
			set { SetField (ref money, value, () => Money); }
		}

		Decimal? deposit;

		public virtual Decimal? Deposit {
			get { return deposit; }
			set { SetField (ref deposit, value, () => Deposit); }
		}

		PaymentType paymentType;

		public virtual PaymentType PaymentType {
			get { return paymentType; }
			set { SetField (ref paymentType, value, () => PaymentType); }
		}
	}
}

