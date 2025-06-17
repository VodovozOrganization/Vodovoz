using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Documents
{
	public class SelfDeliveryDocumentItem: SelfDeliveryDocumentItemEntity
	{
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

				if (GoodsAccountingOperation != null && GoodsAccountingOperation.Nomenclature != nomenclature)
					GoodsAccountingOperation.Nomenclature = nomenclature;
			}
		}

		Equipment equipment;

		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment {
			get { return equipment; }
			set {
				SetField (ref equipment, value, () => Equipment);

				if(CounterpartyMovementOperation != null && CounterpartyMovementOperation.Equipment != equipment)
				{
					CounterpartyMovementOperation.Equipment = equipment;
				}
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

		WarehouseBulkGoodsAccountingOperation _goodsAccountingOperation;

		public virtual WarehouseBulkGoodsAccountingOperation GoodsAccountingOperation { 
			get => _goodsAccountingOperation;
			set => SetField (ref _goodsAccountingOperation, value);
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

		#endregion

		#region Функции

		public virtual string Title {
			get {
				string res = String.Empty;
				if(GoodsAccountingOperation != null)
					res = String.Format(
						"[{2}] {0} - {1}",
						GoodsAccountingOperation.Nomenclature.Name,
						GoodsAccountingOperation.Nomenclature.Unit.MakeAmountShortStr(GoodsAccountingOperation.Amount),
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

		public virtual void CreateOperation(Warehouse warehouse, DateTime time)
		{
			GoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation
				{
					Warehouse = warehouse,
					Amount = -Amount,
					OperationTime = time,
					Nomenclature = Nomenclature,
				};
		}

		public virtual void UpdateOperation(Warehouse warehouse)
		{
			GoodsAccountingOperation.Warehouse = warehouse;
			GoodsAccountingOperation.Amount = -Amount;
		}

		#endregion
	}
}

