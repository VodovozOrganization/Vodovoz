using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Core.Domain.Operations;

namespace Vodovoz.Domain.Documents.IncomingInvoices
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки входящей накладной(объемный учет)",
		Nominative = "строка входящей накладной(объемный учет)")]
	[HistoryTrace]
	public class NomenclatureIncomingInvoiceItem : IncomingInvoiceItem
	{
		public NomenclatureIncomingInvoiceItem()
		{
			GoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation();
		}
		
		public override string Name => Nomenclature != null ? Nomenclature.Name : string.Empty;
		public override string InventoryNumberString => "-";
		public override bool CanEditAmount => Nomenclature != null && !Nomenclature.IsSerial;

		public virtual WarehouseBulkGoodsAccountingOperation WarehouseBulkGoodsAccountingOperation
		{
			get => GoodsAccountingOperation as WarehouseBulkGoodsAccountingOperation;
			set => GoodsAccountingOperation = value;
		}

		public override string ToString() => $"[{Document.Title}] {Nomenclature.Name} - {Nomenclature.Unit.MakeAmountShortStr(Amount)}";
		public override int EntityId => Nomenclature?.Id ?? default(int);
		public override AccountingType AccountingType => AccountingType.Bulk;
		
		public override void UpdateWarehouseOperation()
		{
			WarehouseBulkGoodsAccountingOperation.Warehouse = Document.Warehouse;
		}
	}
}
