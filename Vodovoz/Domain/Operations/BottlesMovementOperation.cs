using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubject ("Передвижения бутылей")]
	public class BottlesMovementOperation: Operation
	{
		//TODO ID Документа перемещения

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

		int movedTo;

		public virtual int MovedTo {
			get { return movedTo; }
			set { SetField (ref movedTo, value, () => MovedTo); }
		}

		int movedFrom;

		public virtual int MovedFrom {
			get { return movedFrom; }
			set { SetField (ref movedFrom, value, () => MovedFrom); }
		}
	}
}

