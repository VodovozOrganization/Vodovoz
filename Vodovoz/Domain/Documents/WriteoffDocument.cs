using System;
using QSOrmProject;

namespace Vodovoz
{
	public class WriteoffDocument: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		DateTime dateAndTime;

		public virtual DateTime DateAndTime {
			get { return dateAndTime; }
			set { dateAndTime = value; }
		}

		Employee responsibleEmployee;

		public virtual Employee ResponsibleEmployee {
			get { return responsibleEmployee; }
			set { responsibleEmployee = value; }
		}

		Counterparty client;

		public virtual Counterparty Client {
			get { return client; }
			set { client = value; }
		}

		DeliveryPoint deliveryPoint;

		public virtual DeliveryPoint DeliveryPoint {
			get { return deliveryPoint; }
			set { deliveryPoint = value; }
		}

		Warehouse writeoffWarehouse;

		public virtual Warehouse WriteoffWarehouse {
			get { return writeoffWarehouse; }
			set { writeoffWarehouse = value; }
		}
	}
}

