using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz
{
	[OrmSubject (JournalName = "Перемещения ТМЦ", ObjectName = "Документ перемещения")]
	public class MovementDocument: Document
	{
		//TODO Movement type (CP - CP or WH - WH)
		//TODO List of elements
		Employee responsiblePerson;

		[Required (ErrorMessage = "Должен быть указан ответственнй за перемещение.")]
		[Display (Name = "Ответственный")]
		public virtual Employee ResponsiblePerson {
			get { return responsiblePerson; }
			set { responsiblePerson = value; }
		}

		Counterparty fromClient;

		[Display (Name = "Клиент отправки")]
		public virtual Counterparty FromClient {
			get { return fromClient; }
			set { fromClient = value; }
		}

		Counterparty toClient;

		[Display (Name = "Клиент получения")]
		public virtual Counterparty ToClient {
			get { return toClient; }
			set { toClient = value; }
		}

		DeliveryPoint fromDeliveryPoint;

		[Display (Name = "Точка отправки")]
		public virtual DeliveryPoint FromDeliveryPoint {
			get { return fromDeliveryPoint; }
			set { fromDeliveryPoint = value; }
		}

		DeliveryPoint toDeliveryPoint;

		[Display (Name = "Точка получения")]
		public virtual DeliveryPoint ToDeliveryPoint {
			get { return toDeliveryPoint; }
			set { toDeliveryPoint = value; }
		}

		Warehouse fromWarehouse;

		[Display (Name = "Склад отправки")]
		public virtual Warehouse FromWarehouse {
			get { return fromWarehouse; }
			set { fromWarehouse = value; }
		}

		Warehouse toWarehouse;

		[Display (Name = "Склад получения")]
		public virtual Warehouse ToWarehouse {
			get { return toWarehouse; }
			set { toWarehouse = value; }
		}

		#region IDocument implementation

		new public virtual string DocType {
			get { return "Документ перемещения"; }
		}

		new public virtual string Description {
			get { return ""; }
		}

		#endregion
	}
}

