using System;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Operations
{
	[Appellative (Gender = GrammaticalGender.Neuter,
		NominativePlural = "передвижения бутылей",
		Nominative = "передвижение бутылей")]
	public class BottlesMovementOperation: OperationBase
	{
		Order order;
		private int _deliveredInDisposableTare;

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
		/// <summary>
		/// Движение тары к контрагенту
		/// </summary>
		/// <value>The delivered.</value>
		public virtual int Delivered {
			get { return delivered; }
			set { SetField (ref delivered, value, () => Delivered); }
		}

		/// <summary>
		/// Движение воды в одноразовой таре к контрагенту
		/// </summary>
		public virtual int DeliveredInDisposableTare
		{
			get => _deliveredInDisposableTare;
			set => SetField(ref _deliveredInDisposableTare, value);
		}

		int returned;
		/// <summary>
		/// Движение тары от контрагента
		/// </summary>
		/// <value>The returned.</value>
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

