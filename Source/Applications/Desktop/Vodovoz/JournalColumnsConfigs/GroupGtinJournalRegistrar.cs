using Gamma.ColumnConfig;
using Vodovoz.Core.Domain.Goods.Gtins;
using Vodovoz.ViewModels.Journals.JournalViewModels.Goods;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class GroupGtinJournalRegistrar : ColumnsConfigRegistrarBase<GroupGtinJournalViewModel, GroupGtin>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<GroupGtin> config) =>
			config.AddColumn("Номер").AddNumericRenderer(node => node.Id)
				.AddColumn("Gtin").AddTextRenderer(node => node.GtinNumber)
				.AddColumn("Штук в упаковке").AddTextRenderer(node => node.CodesCount.ToString())
				.Finish();
	}
}
