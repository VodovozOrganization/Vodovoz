using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QSOrmProject;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Service;

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

		OwnTypes ownType;

		[Display(Name = "Принадлежность")]
		public virtual OwnTypes OwnType {
			get { return ownType; }
			set { SetField(ref ownType, value, () => OwnType); }
		}


		Nomenclature nomenclature;

		[Display (Name = "Номенклатура оборудования")]
		public virtual Nomenclature Nomenclature {
			get { return Equipment?.Nomenclature ?? nomenclature; }
			set { SetField (ref nomenclature, value, () => Nomenclature); }
		}

		Reason reason = Reason.Unknown;

		[Display (Name = "Причина")]
		public virtual Reason Reason {
			get { return reason; }
			set { SetField (ref reason, value, () => Reason); }
		}

		bool confirmed;
		public virtual bool Confirmed{
			get{
				return confirmed;
			}
			set{
				SetField(ref confirmed, value, () => Confirmed);
			}
		}

		CounterpartyMovementOperation counterpartyMovementOperation;

		public virtual CounterpartyMovementOperation CounterpartyMovementOperation {
			get { return counterpartyMovementOperation; }
			set { SetField (ref counterpartyMovementOperation, value, () => CounterpartyMovementOperation); }
		}

		string confirmedComment;
		[Display (Name = "Комментарий по забору")]
		[StringLength(200)]
		public virtual string ConfirmedComment{
			get{
				return confirmedComment;
			}
			set{
				SetField(ref confirmedComment, value, () => ConfirmedComment);
			}
		}

		public virtual string NameString {
			get { 
				if (Equipment != null)
					return Equipment.Title;
				if (Nomenclature != null)
					return Nomenclature.ShortOrFullName;
				
				return "Неизвестное оборудование";
			}
		}

		public virtual string FullNameString {
			get {
				if(Equipment != null)
					return Equipment.Title;
				else if(Nomenclature != null)
					return Nomenclature.OfficialName;
				else
					return "Неизвестное оборудование";
			}
		}

		private ServiceClaim serviceClaim;

		[Display(Name = "Номер заявки на обслуживание")]
		public virtual ServiceClaim ServiceClaim
		{
			get { return serviceClaim; }
			set { SetField(ref serviceClaim, value, () => ServiceClaim); }
		}

		public virtual string DirectionString { get { return Direction.GetEnumTitle (); } }

		public virtual string ReasonString { get { return Reason.GetEnumTitle (); } }

		//TODO Номер заявки на обслуживание

		int count;

		/// <summary>
		/// Количество оборудования, которое изначально должен был привезти/забрать водитель
		/// </summary>
		[Display(Name = "Количество")]
		public virtual int Count {
			get { return count; }
			set { SetField(ref count, value, () => Count); }
		}

		int actualCount;
		/// <summary>
		/// Количество оборудования, которое фактически привез/забрал водитель
		/// </summary>
		public virtual int ActualCount {
			get { return actualCount; }
			set { SetField(ref actualCount, value, () => ActualCount); }
		}

		#region Вычисляемые

		public virtual int ReturnedCount {
			get {
				return Count - ActualCount;
			}
		}

		public virtual bool IsDelivered {
			get {
				return Count - ActualCount == 0;
			}
		}

		#endregion

		#region Функции

		public virtual CounterpartyMovementOperation UpdateCounterpartyOperation()
		{
			if(ActualCount == 0)
			{
				CounterpartyMovementOperation = null;
				return null;
			}

			if (CounterpartyMovementOperation == null)
			{
				CounterpartyMovementOperation = new CounterpartyMovementOperation();
			}

			CounterpartyMovementOperation.OperationTime = Order.DeliveryDate.Value.Date.AddHours(23).AddMinutes(59);
			CounterpartyMovementOperation.Amount = ActualCount;
			CounterpartyMovementOperation.Nomenclature = nomenclature;
			CounterpartyMovementOperation.ForRent = (Reason != Reason.Sale);
			if (Direction == Direction.Deliver)
			{
				CounterpartyMovementOperation.IncomingCounterparty = Order.Client;
				CounterpartyMovementOperation.IncomingDeliveryPoint = Order.DeliveryPoint;
				CounterpartyMovementOperation.WriteoffCounterparty = null;
				CounterpartyMovementOperation.WriteoffDeliveryPoint = null;
			}
			else
			{
				CounterpartyMovementOperation.WriteoffCounterparty = Order.Client;
				CounterpartyMovementOperation.WriteoffDeliveryPoint = Order.DeliveryPoint;
				CounterpartyMovementOperation.IncomingCounterparty = null;
				CounterpartyMovementOperation.IncomingDeliveryPoint = null;
			}	

			return CounterpartyMovementOperation;
		}


		#endregion

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			return null;
		}

		#endregion
	}

	public enum Direction
	{
		[Display (Name = "Доставить")]Deliver,
		[Display (Name = "Забрать")]PickUp
	}

	public class DirectionStringType : NHibernate.Type.EnumStringType
	{
		public DirectionStringType () : base (typeof(Direction))
		{
		}
	}

	public enum Reason
	{
		[Display(Name = "Неизвестна")] Unknown,
		[Display (Name= "Сервис")]Service,
		[Display (Name= "Аренда")]Rent,
		[Display (Name= "Расторжение")]Cancellation,
		[Display (Name= "Продажа")]Sale
	}

	public class ReasonStringType : NHibernate.Type.EnumStringType
	{
		public ReasonStringType () : base (typeof(Reason))
		{
		}
	}

	public enum OwnTypes
	{
		[Display(Name = "")] None,
		[Display(Name = "Собственность клиента")] Client,
		[Display(Name = "Дежурный")] Duty,
		[Display(Name = "Аренда")] Rent
	}

	public class OwnTypesStringType : NHibernate.Type.EnumStringType
	{
		public OwnTypesStringType() : base(typeof(OwnTypes))
		{
		}
	}
}

