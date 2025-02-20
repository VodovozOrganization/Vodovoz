using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;
using VodovozBusiness.Domain.Goods;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class GtinJournalRegistrar : ColumnsConfigRegistrarBase<GtinJournalViewModel, Gtin>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<Gtin> config) =>
			config.AddColumn("Номер").AddNumericRenderer(node => node.Id)
				.AddColumn("Gtin").AddTextRenderer(node => node.GtinNumber)
				.Finish();
	}
}
