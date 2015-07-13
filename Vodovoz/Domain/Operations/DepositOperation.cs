using System;
using QSOrmProject;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Operations
{
	[OrmSubject ("Изменения залогов")]
	public class DepositOperation: OperationBase
	{
		OrderItem orderItem;

		public virtual OrderItem OrderItem {
			get { return orderItem; }
			set { SetField (ref orderItem, value, () => OrderItem); }
		}

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

