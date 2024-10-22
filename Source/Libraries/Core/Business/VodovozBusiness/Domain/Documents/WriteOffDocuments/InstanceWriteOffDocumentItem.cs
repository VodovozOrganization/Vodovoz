using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using Vodovoz.Domain.Documents.IncomingInvoices;
using Vodovoz.Domain.Goods;

namespace Vodovoz.Domain.Documents.WriteOffDocuments
{
	[Appellative (Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки списания (экземплярный учет)",
		Nominative = "строка списания (экземплярный учет)")]
	public abstract class InstanceWriteOffDocumentItem : WriteOffDocumentItem
	{
		private InventoryNomenclatureInstance _inventoryNomenclatureInstance;

		[Display(Name = "Экземпляр номенклатуры")]
		public virtual InventoryNomenclatureInstance InventoryNomenclatureInstance
		{
			get => _inventoryNomenclatureInstance;
			set => SetField(ref _inventoryNomenclatureInstance, value);
		}

		public override AccountingType AccountingType => AccountingType.Instance;

		public override string InventoryNumber =>
			InventoryNomenclatureInstance?.Nomenclature != null && InventoryNomenclatureInstance.Nomenclature.HasInventoryAccounting
				? InventoryNomenclatureInstance.GetInventoryNumber
				: base.InventoryNumber;

		public override bool CanEditAmount => false;
	}
}

