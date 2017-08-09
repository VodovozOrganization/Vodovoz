using System;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using QSOrmProject;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Documents
{
	[OrmSubject (Gender = QSProjectsLib.GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения",
		Nominative = "строка перемещения")]
	public class MovementDocumentItem: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		public virtual MovementDocument Document { get; set; }

		Nomenclature nomenclature;

		[Required (ErrorMessage = "Номенклатура должна быть заполнена.")]
		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set {
				SetField (ref nomenclature, value, () => Nomenclature);
				if (WarehouseMovementOperation != null && WarehouseMovementOperation.Nomenclature != nomenclature)
					WarehouseMovementOperation.Nomenclature = nomenclature;

				if (CounterpartyMovementOperation != null && CounterpartyMovementOperation.Nomenclature != nomenclature)
					CounterpartyMovementOperation.Nomenclature = nomenclature;
			}
		}

		Equipment equipment;

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set {
				SetField (ref equipment, value, () => Equipment);
				if (WarehouseMovementOperation != null && WarehouseMovementOperation.Equipment != equipment)
					WarehouseMovementOperation.Equipment = equipment;

				if (CounterpartyMovementOperation != null && CounterpartyMovementOperation.Equipment != equipment)
					CounterpartyMovementOperation.Equipment = equipment;
			}
		}

		decimal amount;

		[Min (1)]
		[Display (Name = "Количество")]
		public virtual decimal Amount {
			get { return amount; }
			set {
				SetField (ref amount, value, () => Amount);
				if (WarehouseMovementOperation != null && WarehouseMovementOperation.Amount != amount)
					WarehouseMovementOperation.Amount = amount;

				if (DeliveryMovementOperation != null && DeliveryMovementOperation.Amount != amount)
					DeliveryMovementOperation.Amount = amount;

				if (CounterpartyMovementOperation != null && CounterpartyMovementOperation.Amount != amount)
					CounterpartyMovementOperation.Amount = amount;
			}
		}

		decimal amountOnSource = 10000000;
		//FIXME пока не реализуем способ загружать количество на складе на конкретный день

		[Display (Name = "Имеется на складе")]
		public virtual decimal AmountOnSource {
			get { return amountOnSource; }
			set {
				SetField (ref amountOnSource, value, () => AmountOnSource);
			}
		}

		public virtual string Name {
			get { return Nomenclature != null ? Nomenclature.Name : ""; }
		}

		public virtual string EquipmentString { 
			get { return Equipment != null && Equipment.Nomenclature.Serial ? Equipment.Serial : "-"; } 
		}

		public virtual bool CanEditAmount { 
			get { return Nomenclature != null && !Nomenclature.Serial; }
		}

		WarehouseMovementOperation warehouseMovementOperation;

		public virtual WarehouseMovementOperation WarehouseMovementOperation {
			get { return warehouseMovementOperation; }
			set { SetField (ref warehouseMovementOperation, value, () => WarehouseMovementOperation); }
		}

		WarehouseMovementOperation deliveryMovementOperation;

		public virtual WarehouseMovementOperation DeliveryMovementOperation {
			get { return deliveryMovementOperation; }
			set { SetField (ref deliveryMovementOperation, value, () => DeliveryMovementOperation); }
		}

		CounterpartyMovementOperation counterpartyMovementOperation;

		public virtual CounterpartyMovementOperation CounterpartyMovementOperation {
			get { return counterpartyMovementOperation; }
			set { SetField (ref counterpartyMovementOperation, value, () => CounterpartyMovementOperation); }
		}

		public virtual string Title {
			get{
				return String.Format("{0} - {1}", 
					Nomenclature.Name, 
					Nomenclature.Unit.MakeAmountShortStr(Amount));
			}
		}

		#region Функции

		public virtual void CreateOperation(Warehouse warehouseSrc, Warehouse warehouseDst, DateTime time, TransportationStatus status)
		{
			CounterpartyMovementOperation = null;
			WarehouseMovementOperation = new WarehouseMovementOperation
				{
					WriteoffWarehouse = warehouseSrc,
					IncomingWarehouse = status == TransportationStatus.WithoutTransportation ?  warehouseDst : null,
					Amount = Amount,
					OperationTime = time,
					Nomenclature = Nomenclature,
					Equipment = Equipment
				};
			if (status == TransportationStatus.Delivered)
				CreateOperation(warehouseDst, Document.DeliveredTime.Value);
		}

		/// <summary>
		/// Создание операции доставки при транспортировке
		/// </summary>
		public virtual void CreateOperation(Warehouse warehouseDst, DateTime deliveredTime)
		{
			DeliveryMovementOperation = new WarehouseMovementOperation
				{
					IncomingWarehouse = warehouseDst,
					Amount = Amount,
					OperationTime = deliveredTime,
					Nomenclature = Nomenclature,
					Equipment = Equipment
				};
			WarehouseMovementOperation.IncomingWarehouse = null;
		}

		public virtual void CreateOperation(Counterparty counterpartySrc, DeliveryPoint pointSrc, Counterparty counterpartyDst, DeliveryPoint pointDst, DateTime time)
		{
			WarehouseMovementOperation = null;
			CounterpartyMovementOperation = new CounterpartyMovementOperation
				{
					IncomingCounterparty = counterpartyDst,
					IncomingDeliveryPoint = pointDst,
					WriteoffCounterparty = counterpartySrc,
					WriteoffDeliveryPoint = pointSrc,
					Amount = Amount,
					OperationTime = time,
					Nomenclature = Nomenclature,
					Equipment = Equipment
				};
		}

		#endregion
	}
}

