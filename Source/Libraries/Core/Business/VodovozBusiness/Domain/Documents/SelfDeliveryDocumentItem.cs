using System;
using System.ComponentModel.DataAnnotations;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Documents
{
	public class SelfDeliveryDocumentItem: SelfDeliveryDocumentItemEntity
	{
		private SelfDeliveryDocument _document;
		private Nomenclature _nomenclature;
		private Equipment _equipment;
		private OrderItem _orderItem;
		private OrderEquipment _orderEquipment;
		private WarehouseBulkGoodsAccountingOperation _goodsAccountingOperation;
		private CounterpartyMovementOperation _counterpartyMovementOperation;

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
		/// Номенклатура
		/// </summary>
		[Display (Name = "Номенклатура")]
		public virtual new Nomenclature Nomenclature
		{
			get => _nomenclature;
			set
			{
				SetField (ref _nomenclature, value);

				if(GoodsAccountingOperation != null && GoodsAccountingOperation.Nomenclature != _nomenclature)
				{
					GoodsAccountingOperation.Nomenclature = _nomenclature;
				}
			}
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

		/// <summary>
		/// Операция передвижения товаров по складу
		/// </summary>
		[Display(Name = "Операция передвижения товаров по складу")]
		public virtual WarehouseBulkGoodsAccountingOperation GoodsAccountingOperation
		{ 
			get => _goodsAccountingOperation;
			set => SetField (ref _goodsAccountingOperation, value);
		}

		/// <summary>
		/// Операция передвижения товара контрагента
		/// </summary>
		[Display(Name = "Операция передвижения товара контрагента")]
		public virtual CounterpartyMovementOperation CounterpartyMovementOperation
		{
			get => _counterpartyMovementOperation;
			set => SetField (ref _counterpartyMovementOperation, value);
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

