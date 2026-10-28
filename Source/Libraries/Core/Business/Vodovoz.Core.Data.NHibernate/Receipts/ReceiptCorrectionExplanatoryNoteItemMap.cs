using FluentNHibernate.Mapping;
using Vodovoz.Core.Domain.Receipts;

namespace Vodovoz.Core.Data.NHibernate.Receipts
{
	public class ReceiptCorrectionExplanatoryNoteItemMap : ClassMap<ReceiptCorrectionExplanatoryNoteItem>
	{
		public ReceiptCorrectionExplanatoryNoteItemMap()
		{
			Table("receipt_correction_explanatory_note_items");

			Id(x => x.Id).Column("id").GeneratedBy.Native();

			References(x => x.ExplanatoryNote).Column("explanatory_note_id");
			Map(x => x.LineNumber).Column("line_number");
			Map(x => x.NomenclatureId).Column("nomenclature_id");
			Map(x => x.Name).Column("name");
			Map(x => x.Quantity).Column("quantity");
			Map(x => x.Price).Column("price");
			Map(x => x.DiscountSum).Column("discount_sum");
			Map(x => x.Sum).Column("sum");
		}
	}
}
