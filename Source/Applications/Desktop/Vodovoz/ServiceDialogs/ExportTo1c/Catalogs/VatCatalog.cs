using Gamma.Utilities;
using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Attributes;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.ServiceDialogs.ExportTo1c;

namespace Vodovoz.ExportTo1c.Catalogs
{
	public class VatCatalog : GenericCatalog<VatCatalog>, IDomainObject
	{
		public VatCatalog(ExportData exportData)
			: base(exportData)
		{
		}

		public VAT Vat { get; set; }
		protected override string Name => "СтавкиНДС";

		public int Id { get; }

		public override ReferenceNode CreateReferenceTo(VatCatalog vatCatalog)
		{
			var id = GetReferenceId(vatCatalog);

			var referenceNode = new ReferenceNode(id);

			referenceNode.Properties.Add(new PropertyNode("Наименование",
				Common1cTypes.String,
				vatCatalog.Vat.GetAttribute<Value1cComplexAutomation>().Value));

			return referenceNode;
		}
		
		protected override PropertyNode[] GetProperties(VatCatalog vatCatalog)
		{
			return new PropertyNode[] { };
		}
	}
}
