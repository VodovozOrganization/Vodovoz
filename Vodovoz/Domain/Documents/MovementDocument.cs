using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings;
using System.Linq;
using QSOrmProject;

namespace Vodovoz.Domain
{
	[OrmSubject (JournalName = "Перемещения ТМЦ", ObjectName = "Документ перемещения")]
	public class MovementDocument: Document, IValidatableObject
	{
		MovementDocumentCategory category;

		[Display (Name = "Тип документа перемещения")]
		public virtual MovementDocumentCategory Category {
			get { return category; }
			set { SetField (ref category, value, () => Category);
				switch (category) {
				case MovementDocumentCategory.counterparty:
					FromWarehouse = null;
					ToWarehouse = null;
					break;
				case MovementDocumentCategory.warehouse:
					FromClient = null;
					ToClient = null;
					break;
				}
			}
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
			set { SetField (ref fromClient, value, () => FromClient);
				if(FromClient == null || 
					(FromDeliveryPoint != null && FromClient.DeliveryPoints.All (p => p.Id != FromDeliveryPoint.Id))) {
					FromDeliveryPoint = null;
				}
			}
		}

		Counterparty toClient;

		[Display (Name = "Клиент получения")]
		public virtual Counterparty ToClient {
			get { return toClient; }
			set { SetField (ref toClient, value, () => ToClient); 
				if(ToClient == null || 
					(ToDeliveryPoint != null && ToClient.DeliveryPoints.All (p => p.Id != ToDeliveryPoint.Id))) {
					ToDeliveryPoint = null;
				}
			}
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
			set { SetField (ref fromWarehouse, value, () => FromWarehouse);	}
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

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if (Category == MovementDocumentCategory.warehouse && FromWarehouse == ToWarehouse)
				yield return new ValidationResult ("Склады отправления и получения должны различатся.",
					new[] { this.GetPropertyName (o => o.FromWarehouse), this.GetPropertyName (o => o.ToWarehouse) });
			if (Category == MovementDocumentCategory.counterparty)
			{
				if(FromClient == null)
					yield return new ValidationResult ("Клиент отправитель должен быть указан.",
						new[] { this.GetPropertyName (o => o.FromClient)});
				if(ToClient == null)
					yield return new ValidationResult ("Клиент получатель должен быть указан.",
						new[] { this.GetPropertyName (o => o.ToClient)});
				if(FromDeliveryPoint == null)
					yield return new ValidationResult ("Точка доставки отправителя должена быть указана.",
						new[] { this.GetPropertyName (o => o.FromDeliveryPoint)});
				if(ToDeliveryPoint == null)
					yield return new ValidationResult ("Точка доставки получателя должена быть указана.",
						new[] { this.GetPropertyName (o => o.ToDeliveryPoint)});
				if(FromDeliveryPoint == ToDeliveryPoint)
					yield return new ValidationResult ("Точки отправления и получения должны различатся.",
						new[] { this.GetPropertyName (o => o.FromDeliveryPoint), this.GetPropertyName (o => o.ToDeliveryPoint) });
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

