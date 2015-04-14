using System;
using QSOrmProject;

namespace Vodovoz
{
	[OrmSubject ("Передвижения товаров")]
	public class GoodsMovementOperation: OperationBase
	{
		//TODO ID Документа перемещения

		//TODO ID Приходной накладной

		Warehouse writeoffWarehouse;

		public virtual Warehouse WriteoffWarehouse {
			get { return writeoffWarehouse; }
			set { SetField (ref writeoffWarehouse, value, () => WriteoffWarehouse); }
		}

		Warehouse incomingWarehouse;

		public virtual Warehouse IncomingWarehouse {
			get { return incomingWarehouse; }
			set { SetField (ref incomingWarehouse, value, () => IncomingWarehouse); }
		}

		Counterparty incomingCounterparty;

		public virtual Counterparty IncomingCounterparty {
			get { return incomingCounterparty; }
			set { SetField (ref incomingCounterparty, value, () => IncomingCounterparty); }
		}

		DeliveryPoint incomingDeliveryPoint;

		public virtual DeliveryPoint IncomingDeliveryPoint {
			get { return incomingDeliveryPoint; }
			set { SetField (ref incomingDeliveryPoint, value, () => IncomingDeliveryPoint); }
		}

		Counterparty writeoffCounterparty;

		public virtual Counterparty WriteoffCounterparty {
			get { return writeoffCounterparty; }
			set { SetField (ref writeoffCounterparty, value, () => WriteoffCounterparty); }
		}

		DeliveryPoint writeoffDeliveryPoint;

		public virtual DeliveryPoint WriteoffDeliveryPoint {
			get { return writeoffDeliveryPoint; }
			set { SetField (ref writeoffDeliveryPoint, value, () => WriteoffDeliveryPoint); }
		}

		int amount;

		public virtual int Amount {
			get { return amount; }
			set { SetField (ref amount, value, () => Amount); }
		}
	}
}

