using System.ComponentModel.DataAnnotations;
using QSOrmProject;
using System.Data.Bindings;
using System;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (JournalName = "Списания ТМЦ", ObjectName = "Акт списания")]
	public class WriteoffDocument: Document
	{
		//TODO List of elements

		Employee responsibleEmployee;

		[Required (ErrorMessage = "Должен быть указан ответственнй за перемещение.")]
		[Display (Name = "Ответственный")]
		public virtual Employee ResponsibleEmployee {
			get { return responsibleEmployee; }
			set { responsibleEmployee = value; }
		}

		Counterparty client;

		[Display (Name = "Клиент списания")]
		public virtual Counterparty Client {
			get { return client; }
			set { client = value; }
		}

		DeliveryPoint deliveryPoint;

		[Display (Name = "Точка доставки списания")]
		public virtual DeliveryPoint DeliveryPoint {
			get { return deliveryPoint; }
			set { deliveryPoint = value; }
		}

		Warehouse writeoffWarehouse;

		[Display (Name = "Склад списания")]
		public virtual Warehouse WriteoffWarehouse {
			get { return writeoffWarehouse; }
			set { writeoffWarehouse = value; }
		}

		#region IDocument implementation

		new public virtual string DocType {
			get { return "Акт списания"; }
		}

		new public virtual string Description {
			get { 
				if (WriteoffWarehouse != null)
					return String.Format ("Со склада \"{0}\"", WriteoffWarehouse.Name);
				else if (Client != null)
					return String.Format ("От клиента \"{0}\"", Client.Name);
				return "";
			}
		}

		#endregion
	}

	public enum WriteoffType
	{
		[ItemTitleAttribute ("От клиента")]
		counterparty,
		[ItemTitleAttribute ("Со склада")]
		warehouse
	}

	public class WriteoffStringType : NHibernate.Type.EnumStringType
	{
		public WriteoffStringType () : base (typeof(WriteoffType))
		{
		}
	}
}

