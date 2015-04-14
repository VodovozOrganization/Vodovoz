using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubject ("Передвижения денег")]
	public class MoneyMovementOperation: Operation
	{
		

		//TODO ID Строки заказа

		Counterparty counterparty;

		public virtual Counterparty Counterparty {
			get { return counterparty; }
			set { SetField (ref counterparty, value, () => Counterparty); }
		}

		DeliveryPoint deliveryPoint;

		public virtual DeliveryPoint DeliveryPoint {
			get { return deliveryPoint; }
			set { SetField (ref deliveryPoint, value, () => DeliveryPoint); }
		}

		Decimal debt;

		public virtual Decimal Debt {
			get { return debt; }
			set { SetField (ref debt, value, () => Debt); }
		}

		Decimal money;

		public virtual Decimal Money {
			get { return money; }
			set { SetField (ref money, value, () => Money); }
		}

		Decimal deposit;

		public virtual Decimal Deposit {
			get { return deposit; }
			set { SetField (ref deposit, value, () => Deposit); }
		}

		DepositType depositType;

		public virtual DepositType DepositType {
			get { return depositType; }
			set { SetField (ref depositType, value, () => DepositType); }
		}

		PaymentType paymentType;

		public virtual PaymentType PaymentType {
			get { return paymentType; }
			set { SetField (ref paymentType, value, () => PaymentType); }
		}
	}
}

