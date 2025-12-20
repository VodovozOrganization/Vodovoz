using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Documents
{
	public class SelfDeliveryDocumentItem: SelfDeliveryDocumentItemEntity
	{
		private SelfDeliveryDocument _document;
		private Equipment _equipment;
		private OrderItem _orderItem;
		private OrderEquipment _orderEquipment;

		/// <summary>
		/// Документ самовывоза
		/// </summary>
		[Display (Name = "Документ самовывоза")]
		public virtual new SelfDeliveryDocument Document
		{
			get => _document;
			set => SetField(ref _document, value);
		}

		/// <summary>
		/// Оборудование
		/// </summary>
		[Display (Name = "Оборудование")]
		public virtual Equipment Equipment
		{
			get => _equipment;
			set
			{
				SetField (ref _equipment, value, () => Equipment);

				if(CounterpartyMovementOperation != null && CounterpartyMovementOperation.Equipment != _equipment)
				{
					CounterpartyMovementOperation.Equipment = _equipment;
				}
			}
		}

		/// <summary>
		/// Связанный товар
		/// </summary>
		[Display (Name = "Связанный товар")]
		public virtual new OrderItem OrderItem
		{
			get => _orderItem;
			set => SetField (ref _orderItem, value);
		}

		/// <summary>
		/// Связанное оборудование
		/// </summary>
		[Display(Name = "Связанное оборудование")]
		public virtual OrderEquipment OrderEquipment
		{
			get => _orderEquipment;
			set => SetField(ref _orderEquipment, value, () => OrderEquipment);
		}

		#region Функции

		public virtual string Title {
			get {
				string res = string.Empty;
				if(GoodsAccountingOperation != null)
					res = string.Format(
						"[{2}] {0} - {1}",
						GoodsAccountingOperation.Nomenclature.Name,
						GoodsAccountingOperation.Nomenclature.Unit.MakeAmountShortStr(GoodsAccountingOperation.Amount),
						Document.Title
					);
				else if(Nomenclature != null)
					res = string.Format(
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

