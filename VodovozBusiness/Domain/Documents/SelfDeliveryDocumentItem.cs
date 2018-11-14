using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QSOrmProject;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Store;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки документа самовывоза",
		Nominative = "строка документа самовывоза")]	
	public class SelfDeliveryDocumentItem: PropertyChangedBase, IDomainObject
	{
		public virtual int Id { get; set; }

		SelfDeliveryDocument document;

		public virtual SelfDeliveryDocument Document {
			get { return document; }
			set { SetField (ref document, value, () => Document); }
		}

		Nomenclature nomenclature;

		[Display (Name = "Номенклатура")]
		public virtual Nomenclature Nomenclature {
			get { return nomenclature; }
			set {
				SetField (ref nomenclature, value, () => Nomenclature);

				if (WarehouseMovementOperation != null && WarehouseMovementOperation.Nomenclature != nomenclature)
					WarehouseMovementOperation.Nomenclature = nomenclature;
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

		[Display (Name = "Количество")]
		public virtual decimal Amount {
			get { return amount; }
			set {
				SetField (ref amount, value, () => Amount);
			}
		}

		OrderItem orderItem;

		[Display (Name = "Связанный товар")]
		public virtual OrderItem OrderItem {
			get { return orderItem; }
			set { SetField (ref orderItem, value, () => OrderItem); }
		}

		OrderEquipment orderEquipment;

		[Display(Name = "Связанное оборудование")]
		public virtual OrderEquipment OrderEquipment {
			get { return orderEquipment; }
			set { SetField(ref orderEquipment, value, () => OrderEquipment); }
		}

		WarehouseMovementOperation warehouseMovementOperation;

		public virtual WarehouseMovementOperation WarehouseMovementOperation { 
			get { return warehouseMovementOperation; }
			set { SetField (ref warehouseMovementOperation, value, () => WarehouseMovementOperation); }
		}

		CounterpartyMovementOperation counterpartyMovementOperation;

		public virtual CounterpartyMovementOperation CounterpartyMovementOperation {
			get { return counterpartyMovementOperation; }
			set { SetField (ref counterpartyMovementOperation, value, () => CounterpartyMovementOperation); }
		}

		#region Не сохраняемые

		decimal amountInStock;

		[Display (Name = "Количество на складе")]
		public virtual decimal AmountInStock {
			get { return amountInStock; }
			set {
				SetField (ref amountInStock, value, () => AmountInStock);
			}
		}

		decimal amountUnloaded;

		[Display (Name = "Уже отгружено")]
		public virtual decimal AmountUnloaded {
			get { return amountUnloaded; }
			set {
				SetField (ref amountUnloaded, value, () => AmountUnloaded);
			}
		}

		public virtual string Title {
			get{
				string res = String.Empty;
				if(WarehouseMovementOperation != null)
					res = String.Format(
						"[{2}] {0} - {1}",
						WarehouseMovementOperation.Nomenclature.Name,
						WarehouseMovementOperation.Nomenclature.Unit.MakeAmountShortStr(WarehouseMovementOperation.Amount),
						Document.Title
					);
				else if(Nomenclature != null)
					res = String.Format(
						"[{2}] {0} - {1}",
						Nomenclature.Name,
						Nomenclature.Unit.MakeAmountShortStr(Amount),
						Document.Title
					);
				return res;
			}
		}

		#endregion

		#region Функции

		public virtual void CreateOperation(Warehouse warehouse, DateTime time)
		{
			WarehouseMovementOperation = new WarehouseMovementOperation
				{
					WriteoffWarehouse = warehouse,
					Amount = Amount,
					OperationTime = time,
					Nomenclature = Nomenclature,
					Equipment = Equipment
				};
		}

		public virtual void UpdateOperation(Warehouse warehouse)
		{
			WarehouseMovementOperation.WriteoffWarehouse = warehouse;
			WarehouseMovementOperation.IncomingWarehouse = null;
			WarehouseMovementOperation.Amount = Amount;
			WarehouseMovementOperation.Equipment = Equipment;
		}

		#endregion
	}
}

