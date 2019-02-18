using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Store;

namespace Vodovoz.Domain.Documents
{
	[Appellative (Gender = GrammaticalGender.Masculine,
		NominativePlural = "документы перемещения ТМЦ",
		Nominative = "документ перемещения ТМЦ")]
	[EntityPermission]
	public class MovementDocument : Document, IValidatableObject
	{
		MovementDocumentCategory category;

		[Display (Name = "Тип документа перемещения")]
		public virtual MovementDocumentCategory Category {
			get { return category; }
			set {
				SetField (ref category, value, () => Category);
				switch (category) {
				case MovementDocumentCategory.counterparty:
					FromWarehouse = null;
					ToWarehouse = null;
					break;
				case MovementDocumentCategory.Transportation:
				case MovementDocumentCategory.warehouse:
					FromClient = null;
					ToClient = null;
					break;
				}
			}
		}

		public override DateTime TimeStamp {
			get { return base.TimeStamp; }
			set {
				base.TimeStamp = value;
				foreach (var item in Items) {
					if (item.WarehouseMovementOperation != null && item.WarehouseMovementOperation.OperationTime != TimeStamp)
						item.WarehouseMovementOperation.OperationTime = TimeStamp;
					if (item.CounterpartyMovementOperation != null && item.CounterpartyMovementOperation.OperationTime != TimeStamp)
						item.CounterpartyMovementOperation.OperationTime = TimeStamp;
				}
			}
		}

		DateTime? deliveredTime;

		[PropertyChangedAlso("TransportationDescription")]
		[Display(Name = "Время доставки")]
		public virtual DateTime? DeliveredTime {
			get { return deliveredTime; }
			set {
				SetField (ref deliveredTime, value, () => DeliveredTime);
				if (deliveredTime.HasValue)
				{
					foreach (var item in Items)
					{
						if (item.DeliveryMovementOperation != null && item.DeliveryMovementOperation.OperationTime != DeliveredTime.Value)
							item.DeliveryMovementOperation.OperationTime = DeliveredTime.Value;
					}
				}
			}
		}

		TransportationStatus transportationStatus;

		[Display (Name = "Статус транспортировки")]
		[PropertyChangedAlso("TransportationDescription")]
		public virtual TransportationStatus TransportationStatus {
			get { return transportationStatus; }
			protected set { SetField (ref transportationStatus, value, () => TransportationStatus); }
		}

		MovementWagon movementWagon;

		[Display (Name = "Фура")]
		public virtual MovementWagon MovementWagon {
			get { return movementWagon; }
			set { SetField (ref movementWagon, value, () => MovementWagon); }
		}

		string comment;

		[Display (Name = "Комментарий")]
		public virtual string Comment {
			get { return comment; }
			set { SetField (ref comment, value, () => Comment); }
		}

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
			set {
				SetField (ref fromClient, value, () => FromClient);
				if (FromClient == null ||
				    (FromDeliveryPoint != null && FromClient.DeliveryPoints.All (p => p.Id != FromDeliveryPoint.Id))) {
					FromDeliveryPoint = null;
				}
				foreach (var item in Items) {
					if (item.CounterpartyMovementOperation != null && item.CounterpartyMovementOperation.WriteoffCounterparty != fromClient)
						item.CounterpartyMovementOperation.WriteoffCounterparty = fromClient;
				}
			}
		}

		Counterparty toClient;

		[Display (Name = "Клиент получения")]
		public virtual Counterparty ToClient {
			get { return toClient; }
			set {
				SetField (ref toClient, value, () => ToClient); 
				if (ToClient == null ||
				    (ToDeliveryPoint != null && ToClient.DeliveryPoints.All (p => p.Id != ToDeliveryPoint.Id))) {
					ToDeliveryPoint = null;
				}
				foreach (var item in Items) {
					if (item.CounterpartyMovementOperation != null && item.CounterpartyMovementOperation.IncomingCounterparty != toClient)
						item.CounterpartyMovementOperation.IncomingCounterparty = toClient;
				}
			}
		}

		DeliveryPoint fromDeliveryPoint;

		[Display (Name = "Точка отправки")]
		public virtual DeliveryPoint FromDeliveryPoint {
			get { return fromDeliveryPoint; }
			set {
				SetField (ref fromDeliveryPoint, value, () => FromDeliveryPoint); 
				foreach (var item in Items) {
					if (item.CounterpartyMovementOperation != null && item.CounterpartyMovementOperation.WriteoffDeliveryPoint != fromDeliveryPoint)
						item.CounterpartyMovementOperation.WriteoffDeliveryPoint = fromDeliveryPoint;
				}
			}
		}

		DeliveryPoint toDeliveryPoint;

		[Display (Name = "Точка получения")]
		public virtual DeliveryPoint ToDeliveryPoint {
			get { return toDeliveryPoint; }
			set {
				SetField (ref toDeliveryPoint, value, () => ToDeliveryPoint); 
				foreach (var item in Items) {
					if (item.CounterpartyMovementOperation != null && item.CounterpartyMovementOperation.IncomingDeliveryPoint != toDeliveryPoint)
						item.CounterpartyMovementOperation.IncomingDeliveryPoint = toDeliveryPoint;
				}
			}
		}

		Warehouse fromWarehouse;

		[Display (Name = "Склад отправки")]
		public virtual Warehouse FromWarehouse {
			get { return fromWarehouse; }
			set { 
				SetField (ref fromWarehouse, value, () => FromWarehouse);	
				foreach (var item in Items) {
					if (item.WarehouseMovementOperation != null && item.WarehouseMovementOperation.WriteoffWarehouse != fromWarehouse)
						item.WarehouseMovementOperation.WriteoffWarehouse = fromWarehouse;
				}
			}
		}

		Warehouse toWarehouse;

		[Display (Name = "Склад получения")]
		public virtual Warehouse ToWarehouse {
			get { return toWarehouse; }
			set { 
				SetField (ref toWarehouse, value, () => ToWarehouse); 
				foreach (var item in Items) {
					if(Category == MovementDocumentCategory.warehouse)
					{
						if (item.WarehouseMovementOperation != null && item.WarehouseMovementOperation.IncomingWarehouse != toWarehouse)
							item.WarehouseMovementOperation.IncomingWarehouse = toWarehouse;
					}
					if(Category == MovementDocumentCategory.Transportation)
					{
						if (item.DeliveryMovementOperation != null && item.DeliveryMovementOperation.IncomingWarehouse != toWarehouse)
							item.DeliveryMovementOperation.IncomingWarehouse = toWarehouse;
					}
				}
			}
		}

		IList<MovementDocumentItem> items = new List<MovementDocumentItem> ();

		[Display (Name = "Строки")]
		public virtual IList<MovementDocumentItem> Items {
			get { return items; }
			set {
				SetField (ref items, value, () => Items);
				observableItems = null;
			}
		}

		GenericObservableList<MovementDocumentItem> observableItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<MovementDocumentItem> ObservableItems {
			get {
				if (observableItems == null)
					observableItems = new GenericObservableList<MovementDocumentItem> (Items);
				return observableItems;
			}
		}

		#region Вычисляемые

		public virtual string Title => String.Format("Перемещение ТМЦ №{0} от {1:d}", Id, TimeStamp);

		public virtual string TransportationDescription { 
			get { 
				if(TransportationStatus == TransportationStatus.Delivered)
					return String.Format ("{0} ({1:g})", TransportationStatus.GetEnumTitle(), DeliveredTime);
				else
					return String.Format ("{0}", TransportationStatus.GetEnumTitle()); }
		}

		#endregion

		#region IValidatableObject implementation

		public virtual System.Collections.Generic.IEnumerable<ValidationResult> Validate (ValidationContext validationContext)
		{
			if(Items.Count == 0)
				yield return new ValidationResult (String.Format("Табличная часть документа пустая."),
					new[] { this.GetPropertyName (o => o.Items) });

			if (Category == MovementDocumentCategory.warehouse || Category == MovementDocumentCategory.Transportation)
			{
				if(FromWarehouse == ToWarehouse)
					yield return new ValidationResult ("Склады отправления и получения должны различатся.",
						new[] { this.GetPropertyName (o => o.FromWarehouse), this.GetPropertyName (o => o.ToWarehouse) });
				if(FromWarehouse == null)
					yield return new ValidationResult ("Склады отправления должен быть указан.",
						new[] { this.GetPropertyName (o => o.FromWarehouse)});
				if(ToWarehouse == null)
					yield return new ValidationResult ("Склады получения должен быть указан.",
						new[] { this.GetPropertyName (o => o.ToWarehouse)});
			}
				
			if (Category == MovementDocumentCategory.counterparty) {
				if (FromClient == null)
					yield return new ValidationResult ("Клиент отправитель должен быть указан.",
						new[] { this.GetPropertyName (o => o.FromClient) });
				if (ToClient == null)
					yield return new ValidationResult ("Клиент получатель должен быть указан.",
						new[] { this.GetPropertyName (o => o.ToClient) });
				if (FromDeliveryPoint == null)
					yield return new ValidationResult ("Точка доставки отправителя должена быть указана.",
						new[] { this.GetPropertyName (o => o.FromDeliveryPoint) });
				if (ToDeliveryPoint == null)
					yield return new ValidationResult ("Точка доставки получателя должена быть указана.",
						new[] { this.GetPropertyName (o => o.ToDeliveryPoint) });
				if (FromDeliveryPoint == ToDeliveryPoint)
					yield return new ValidationResult ("Точки отправления и получения должны различатся.",
						new[] { this.GetPropertyName (o => o.FromDeliveryPoint), this.GetPropertyName (o => o.ToDeliveryPoint) });
			}

			if(Category == MovementDocumentCategory.Transportation)
			{
				if(MovementWagon == null)
					yield return new ValidationResult ("Фура не указана.",
						new[] { this.GetPropertyName (o => o.MovementWagon)});
			}

			foreach(var item in Items)
			{
				if(item.Amount <= 0)
					yield return new ValidationResult (String.Format("Для номенклатуры <{0}> не указано количество.", item.Nomenclature.Name),
						new[] { this.GetPropertyName (o => o.Items) });
			}
		}

		#endregion

		#region Функции

		public virtual void AddItem (Nomenclature nomenclature, decimal amount, decimal inStock)
		{
			var item = new MovementDocumentItem
			{
					Nomenclature = nomenclature,
					Amount = amount,
					AmountOnSource = inStock,
					Document = this
			};
			if (Category == MovementDocumentCategory.counterparty)
				item.CreateOperation(FromClient, FromDeliveryPoint, ToClient, ToDeliveryPoint, TimeStamp);
			else if (Category == MovementDocumentCategory.warehouse)
				item.CreateOperation(FromWarehouse, ToWarehouse, TimeStamp, TransportationStatus);
			else
			{
				if (TransportationStatus == TransportationStatus.WithoutTransportation)
					TransportationStatus = TransportationStatus.Submerged;
				item.CreateOperation(FromWarehouse, ToWarehouse, TimeStamp, TransportationStatus);
			}
			
			ObservableItems.Add (item);
		}

		public virtual void TransportationCompleted()
		{
			if (Category != MovementDocumentCategory.Transportation)
				throw new InvalidOperationException("Нельзя завершить доставку для документа не имеющего тип транспортировка.");
			DeliveredTime = DateTime.Now;
			TransportationStatus = TransportationStatus.Delivered;

			foreach(var item in Items)
			{
				item.CreateOperation(ToWarehouse, DeliveredTime.Value);
			}
		}

		#endregion

		public MovementDocument ()
		{
			Comment = String.Empty;
		}
	}

	public enum MovementDocumentCategory
	{
		[Display (Name = "Именное списание")]
		counterparty,
		[Display (Name = "Внутреннее перемещение")]
		warehouse,
		[Display (Name = "Транспортировка")]
		Transportation
	}

	public class MovementDocumentCategoryStringType : NHibernate.Type.EnumStringType
	{
		public MovementDocumentCategoryStringType () : base (typeof(MovementDocumentCategory))
		{
		}
	}

	public enum TransportationStatus
	{
		[Display (Name = "Без транспортировки")]
		WithoutTransportation,
		[Display (Name = "Погружено")]
		Submerged,
		[Display (Name = "Доставлено")]
		Delivered
	}

	public class TransportationStatusStringType : NHibernate.Type.EnumStringType
	{
		public TransportationStatusStringType () : base (typeof(TransportationStatus))
		{
		}
	}

}

