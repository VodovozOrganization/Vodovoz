using System;
using QSOrmProject;
using System.Data.Bindings;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Orders
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "строки оборудования в заказе",
		Nominative = "строка оборудования в заказе")]
	public class OrderEquipment: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		Order order;

		[Display (Name = "Заказ")]
		public virtual Order Order {
			get { return order; }
			set { SetField (ref order, value, () => Order); }
		}

		Direction direction;

		[Display (Name = "Направление")]
		public virtual Direction Direction {
			get { return direction; }
			set { SetField (ref direction, value, () => Direction); }
		}

		OrderItem orderItem;

		[Display (Name = "Связанная строка")]
		public virtual OrderItem OrderItem {
			get { return orderItem; }
			set { SetField (ref orderItem, value, () => OrderItem); }
		}

		Equipment equipment;

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set { SetField (ref equipment, value, () => Equipment); }
		}

		Nomenclature newEquipmentNomenclature;

		[Display (Name = "Номенклатура незарегистрированного оборудования")]
		public virtual Nomenclature NewEquipmentNomenclature {
			get { return newEquipmentNomenclature; }
			set { if (Equipment != null && value != null)
					throw new InvalidOperationException (String.Format ("Если указано конкретное оборудование в {0}, {1} не надо заполнять, так как это поле только для незарегистрированного оборудования.",
						this.GetPropertyName (e => e.Equipment),
						this.GetPropertyName (e => e.NewEquipmentNomenclature)
					));
				SetField (ref newEquipmentNomenclature, value, () => NewEquipmentNomenclature); }
		}

		Reason reason;

		[Display (Name = "Причина")]
		public virtual Reason Reason {
			get { return reason; }
			set { SetField (ref reason, value, () => Reason); }
		}

		public virtual string NameString {
			get { 
				if (Equipment != null)
					return Equipment.Title;
				else if (NewEquipmentNomenclature != null)
					return String.Format ("{0} (не зарегистрированный)", NewEquipmentNomenclature.Name);
				else
					return "Неизвестное оборудование";
			}
		}

		public virtual string DirectionString { get { return Direction.GetEnumTitle (); } }

		public virtual string ReasonString { get { return Reason.GetEnumTitle (); } }

		//TODO Номер заявки на обслуживание

		#region IValidatableObject implementation

		public System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			return null;
		}

		#endregion
	}

	public enum Direction
	{
		[ItemTitleAttribute ("Доставить")]Deliver,
		[ItemTitleAttribute ("Забрать")]PickUp
	}

	public class DirectionStringType : NHibernate.Type.EnumStringType
	{
		public DirectionStringType () : base (typeof(Direction))
		{
		}
	}

	public enum Reason
	{
		[ItemTitleAttribute ("Сервис")]Service,
		[ItemTitleAttribute ("Аренда")]Rent,
		[ItemTitleAttribute ("Расторжение")]Cancellation,
		[ItemTitleAttribute ("Продажа")]Sale
	}

	public class ReasonStringType : NHibernate.Type.EnumStringType
	{
		public ReasonStringType () : base (typeof(Reason))
		{
		}
	}
}

