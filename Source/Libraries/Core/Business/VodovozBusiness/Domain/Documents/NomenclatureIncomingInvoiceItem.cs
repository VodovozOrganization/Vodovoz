using System.ComponentModel.DataAnnotations;
using QS.DomainModel.Entity;
using QS.HistoryLog;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Operations;

namespace Vodovoz.Domain.Documents
{
	[Appellative(Gender = GrammaticalGender.Feminine,
		NominativePlural = "строки входящей накладной",
		Nominative = "строка входящей накладной")]
	[HistoryTrace]
	public class NomenclatureIncomingInvoiceItem : IncomingInvoiceItem
	{
		public NomenclatureIncomingInvoiceItem()
		{
			GoodsAccountingOperation = new WarehouseBulkGoodsAccountingOperation();
		}
		
		public override string Name => Nomenclature != null ? Nomenclature.Name : "";
		public override string NumberString => "-";
		public override bool CanEditAmount => Nomenclature != null && !Nomenclature.IsSerial;

		public virtual WarehouseBulkGoodsAccountingOperation WarehouseBulkGoodsAccountingOperation
		{
			get => GoodsAccountingOperation as WarehouseBulkGoodsAccountingOperation;
			set => GoodsAccountingOperation = value;
		}

		public override string ToString() => $"[{Document.Title}] {Nomenclature.Name} - {Nomenclature.Unit.MakeAmountShortStr(Amount)}";
		
		public override AccountingType AccountingType => AccountingType.Bulk;
	}
}
