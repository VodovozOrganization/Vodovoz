using System.Linq;
using QS.Project.Journal;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Goods;
using Vodovoz.Extensions;

namespace Vodovoz.ViewModels.Journals.JournalNodes.Goods
{
	public class NomenclatureJournalNode : JournalEntityNodeBase<Nomenclature>
	{
		public override string Title => Name;
		public string Name { get; set; }
		public NomenclatureCategory Category { get; set; }
		public decimal InStock { get; set; }
		public decimal? Reserved { get; set; }
		public decimal Available => InStock - Reserved.GetValueOrDefault();
		public string UnitName { get; set; }
		public short UnitDigits { get; set; }
		
		public bool IsEquipmentWithSerial { get; set; }
		public string OnlineStoreExternalId { get; set; }

		public bool CalculateQtyOnStock { get; set; } = true;
		public string InStockText => UsedStock ? Format(InStock) : string.Empty;
		public string ReservedText => UsedStock && Reserved.HasValue ? Format(Reserved.Value) : string.Empty;
		public string AvailableText => UsedStock ? Format(Available) : string.Empty;

		string Format(decimal value) => string.Format("{0:F" + UnitDigits + "} {1}", value, UnitName);
		bool UsedStock => CalculateQtyOnStock && Nomenclature.GetCategoriesForGoods().Contains(Category);

		public GlassHolderType? GlassHolderType {  get; set; }
		public string GlassHolderTypeString => 
			GlassHolderType.HasValue
			? GlassHolderType.Value.GetEnumDisplayName()
			: string.Empty;
	}
}
