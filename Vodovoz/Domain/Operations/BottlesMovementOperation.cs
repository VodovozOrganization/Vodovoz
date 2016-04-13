using System;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Operations
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Neuter,
		NominativePlural = "передвижения бутылей",
		Nominative = "передвижение бутылей")]
	public class BottlesMovementOperation: OperationBase
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

		DeliveryPoint deliveryPoint;

		public virtual DeliveryPoint DeliveryPoint {
			get { return deliveryPoint; }
			set { SetField (ref deliveryPoint, value, () => DeliveryPoint); }
		}

		int delivered;

		public virtual int Delivered {
			get { return delivered; }
			set { SetField (ref delivered, value, () => Delivered); }
		}

		int returned;

		public virtual int Returned {
			get { return returned; }
			set { SetField (ref returned, value, () => Returned); }
		}

		public virtual string Title{
			get{
				return String.Format("Движения тары к контрагенту {0} от контрагента {1} бутылей", Delivered, Returned);
			}
		}
	}
}

