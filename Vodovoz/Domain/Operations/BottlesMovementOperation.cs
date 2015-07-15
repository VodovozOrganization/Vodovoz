using QSOrmProject;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Operations
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "передвижения бутылей",
		Nominative = "передвижение бутылей")]
	public class BottlesMovementOperation: OperationBase
	{
		//TODO ID Документа перемещения

		OrderItem orderItem;

		public virtual OrderItem OrderItem {
			get { return orderItem; }
			set { SetField (ref orderItem, value, () => OrderItem); }
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

