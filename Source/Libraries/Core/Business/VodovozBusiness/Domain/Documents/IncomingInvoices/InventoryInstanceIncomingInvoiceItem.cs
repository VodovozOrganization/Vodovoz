﻿using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents.IncomingInvoices
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки входящей накладной(экземплярный учет)",
		Nominative = "строка входящей накладной(экземплярный учет)")]
	[HistoryTrace]
	public class InventoryInstanceIncomingInvoiceItem : IncomingInvoiceItem
	{
		private InventoryNomenclatureInstance _inventoryNomenclatureInstance;

		public InventoryInstanceIncomingInvoiceItem()
		{
			GoodsAccountingOperation = new WarehouseInstanceGoodsAccountingOperation();
		}
		
		[Display(Name = "Экземпляр")]
		public virtual InventoryNomenclatureInstance InventoryNomenclatureInstance
		{
			get => _inventoryNomenclatureInstance;
			set
			{
				if(!SetField(ref _inventoryNomenclatureInstance, value))
				{
					return;
				}

				Nomenclature = InventoryNomenclatureInstance?.Nomenclature;
				UpdateOperation(nameof(InstanceGoodsAccountingOperation.InventoryNomenclatureInstance), value);
				UpdateOperation(nameof(GoodsAccountingOperation.Nomenclature), Nomenclature);
			}
		}

		public override string Name => InventoryNomenclatureInstance != null
			? InventoryNomenclatureInstance.Nomenclature?.Name
			: string.Empty;
		
		public override string InventoryNumberString =>
			InventoryNomenclatureInstance?.Nomenclature != null && InventoryNomenclatureInstance.Nomenclature.HasInventoryAccounting
				? InventoryNomenclatureInstance.InventoryNumber
				: string.Empty;
		public override bool CanEditAmount => false;

		public override int EntityId => InventoryNomenclatureInstance?.Id ?? default(int);

		public virtual WarehouseInstanceGoodsAccountingOperation WarehouseInstanceGoodsAccountingOperation
		{
			get => GoodsAccountingOperation as WarehouseInstanceGoodsAccountingOperation;
			set => GoodsAccountingOperation = value;
		}

		public override AccountingType AccountingType => AccountingType.Instance;
	}
}
