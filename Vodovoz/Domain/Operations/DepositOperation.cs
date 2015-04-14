using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubject ("Передвижения залогов")]
	public class DepositOperation: Operation
	{
		//TODO ID Строки заказа

		DepositType depositType;

		public virtual DepositType DepositType {
			get { return depositType; }
			set { SetField (ref depositType, value, () => DepositType); }
		}

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

		Decimal receivedDeposit;

		public virtual Decimal ReceivedDeposit {
			get { return receivedDeposit; }
			set { SetField (ref receivedDeposit, value, () => ReceivedDeposit); }
		}

		Decimal refundDeposit;

		public virtual Decimal RefundDeposit {
			get { return refundDeposit; }
			set { SetField (ref refundDeposit, value, () => RefundDeposit); }
		}
	}
}

