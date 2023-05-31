﻿using QS.DomainModel.Entity;
using Vodovoz.Domain.Documents.IncomingInvoices;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Documents.MovementDocuments.InstanceAccounting
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки перемещения(экземплярный учет)",
		Nominative = "строка перемещения(экземплярный учет)")]
	public abstract class InstanceMovementDocumentItem : MovementDocumentItem
	{
		private InventoryNomenclatureInstance _inventoryNomenclatureInstance;

		public virtual InventoryNomenclatureInstance InventoryNomenclatureInstance
		{
			get => _inventoryNomenclatureInstance;
			set
			{
				if(SetField(ref _inventoryNomenclatureInstance, value))
				{
					Nomenclature = value?.Nomenclature;
				}
			}
		}

		public override AccountingType AccountingType => AccountingType.Instance;
		public override string InventoryNumber =>
			InventoryNomenclatureInstance != null ? InventoryNomenclatureInstance.InventoryNumber : string.Empty;
		public override bool CanEditAmount => false;
	}
}
