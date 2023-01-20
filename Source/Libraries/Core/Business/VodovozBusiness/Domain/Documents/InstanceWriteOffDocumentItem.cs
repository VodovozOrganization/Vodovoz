using System;
using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания (экземплярный учет)",
		Nominative = "строка списания (экземплярный учет)")]
	[HistoryTrace]
	public class InstanceWriteOffDocumentItem : WriteOffDocumentItem
	{
		private InventoryNomenclatureInstance _inventoryNomenclatureInstance;

		public override AccountingType AccountingType => AccountingType.Instance;

		[Display (Name = "Экземпляр номенклатуры")]
		public virtual InventoryNomenclatureInstance InventoryNomenclatureInstance
		{
			get => _inventoryNomenclatureInstance;
			set
			{
				if(!SetField(ref _inventoryNomenclatureInstance, value))
				{
					return;
				}

				/*if(WarehouseWriteoffOperation != null)
				{
					if(WarehouseWriteoffOperation.InventoryNomenclatureInstance != value)
					{
						WarehouseWriteoffOperation.InventoryNomenclatureInstance = value;
					}
					if(WarehouseWriteoffOperation.Nomenclature != _nomenclature)
					{
						WarehouseWriteoffOperation.Nomenclature = _nomenclature;
					}
				}*/

				/*if(CounterpartyWriteoffOperation != null && CounterpartyWriteoffOperation.Nomenclature != _nomenclature)
				{
					CounterpartyWriteoffOperation.Nomenclature = _nomenclature;
				}*/
			}
		}
		
		protected virtual InstanceGoodsAccountingOperation InstanceGoodsAccountingOperation =>
			WarehouseWriteOffOperation as InstanceGoodsAccountingOperation;

		public override string InventoryNumber =>
			InventoryNomenclatureInstance?.Nomenclature != null && InventoryNomenclatureInstance.Nomenclature.HasInventoryAccounting
				? InventoryNomenclatureInstance.InventoryNumber
				: base.InventoryNumber;

		public override bool CanEditAmount => false;

		protected override void FillOperation()
		{
			if(WarehouseWriteOffOperation is null)
			{
				throw new InvalidOperationException("Не создана операция списания!");
			}
			
			InstanceGoodsAccountingOperation.InventoryNomenclatureInstance = InventoryNomenclatureInstance;
			base.FillOperation();
		}
	}
}

