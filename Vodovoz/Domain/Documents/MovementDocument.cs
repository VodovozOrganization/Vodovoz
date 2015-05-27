using System;
using QSOrmProject;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;

namespace Vodovoz.Domain
{
	[OrmSubject (JournalName = "Перемещения ТМЦ", ObjectName = "Документ перемещения")]
	public class MovementDocument: Document
	{
		MovementDocumentCategory category;

		[Display (Name = "Тип документа перемещения")]
		public virtual MovementDocumentCategory Category {
			get { return category; }
			set { SetField (ref category, value, () => Category); }
		}
		//TODO List of elements
		Employee responsiblePerson;

		[Required (ErrorMessage = "Должен быть указан ответственнй за перемещение.")]
		[Display (Name = "Ответственный")]
		public virtual Employee ResponsiblePerson {
			get { return responsiblePerson; }
			set { SetField (ref responsiblePerson, value, () => ResponsiblePerson); }
		}

		Counterparty fromClient;

		[Display (Name = "Клиент отправки")]
		public virtual Counterparty FromClient {
			get { return fromClient; }
			set { SetField (ref fromClient, value, () => FromClient); }
		}

		Counterparty toClient;

		[Display (Name = "Клиент получения")]
		public virtual Counterparty ToClient {
			get { return toClient; }
			set { SetField (ref toClient, value, () => ToClient); }
		}

		DeliveryPoint fromDeliveryPoint;

		[Display (Name = "Точка отправки")]
		public virtual DeliveryPoint FromDeliveryPoint {
			get { return fromDeliveryPoint; }
			set { SetField (ref fromDeliveryPoint, value, () => FromDeliveryPoint); }
		}

		DeliveryPoint toDeliveryPoint;

		[Display (Name = "Точка получения")]
		public virtual DeliveryPoint ToDeliveryPoint {
			get { return toDeliveryPoint; }
			set { SetField (ref toDeliveryPoint, value, () => ToDeliveryPoint); }
		}

		Warehouse fromWarehouse;

		[Display (Name = "Склад отправки")]
		public virtual Warehouse FromWarehouse {
			get { return fromWarehouse; }
			set { SetField (ref fromWarehouse, value, () => FromWarehouse); }
		}

		Warehouse toWarehouse;

		[Display (Name = "Склад получения")]
		public virtual Warehouse ToWarehouse {
			get { return toWarehouse; }
			set { SetField (ref toWarehouse, value, () => ToWarehouse); }
		}

		#region IDocument implementation

		new public virtual string DocType {
			get { return "Документ перемещения"; }
		}

		new public virtual string Description {
			get { 
				if (Category == MovementDocumentCategory.counterparty)
					return String.Format ("\"{0}\" -> \"{1}\"", 
						FromClient == null ? "" : FromClient.Name,
						ToClient == null ? "" : ToClient.Name);
				return String.Format ("{0} -> {1}",
					FromWarehouse == null ? "" : FromWarehouse.Name,
					ToWarehouse == null ? "" : ToWarehouse.Name); 
			}
		}

		#endregion
	}

	public enum MovementDocumentCategory
	{
		[ItemTitleAttribute ("Именное списание")]
		counterparty,
		[ItemTitleAttribute ("Внутреннее перемещение")]
		warehouse
	}

	public class MovementDocumentCategoryStringType : NHibernate.Type.EnumStringType
	{
		public MovementDocumentCategoryStringType () : base (typeof(MovementDocumentCategory))
		{
		}
	}
}

