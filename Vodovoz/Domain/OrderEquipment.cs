using System;
using QSOrmProject;
using System.Data.Bindings;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain
{
	[OrmSubject (JournalName =  "Строки оборудования в заказе", ObjectName = "оборудование в заказе")]
	public class OrderEquipment: PropertyChangedBase, IDomainObject, IValidatableObject
	{
		public virtual int Id { get; set; }

		Direction direction;

		[Display(Name = "Направление")]
		public virtual Direction Direction {
			get { return direction; }
			set { SetField (ref direction, value, () => Direction); }
		}

		OrderItem orderItem;

		[Display(Name = "Связанная строка")]
		public virtual OrderItem OrderItem {
			get { return orderItem; }
			set { SetField (ref orderItem, value, () => OrderItem); }
		}

		Equipment equipment;

		[Display(Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set { SetField (ref equipment, value, () => Equipment); }
		}

		Reason reason;

		[Display(Name = "Причина")]
		public virtual Reason Reason {
			get { return reason; }
			set { SetField (ref reason, value, () => Reason); }
		}

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
		[ItemTitleAttribute ("Расторжение")]Cancellation
	}

	public class ReasonStringType : NHibernate.Type.EnumStringType
	{
		public ReasonStringType () : base (typeof(Reason))
		{
		}
	}
}

