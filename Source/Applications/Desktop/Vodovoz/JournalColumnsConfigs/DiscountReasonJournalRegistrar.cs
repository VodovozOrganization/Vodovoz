using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Journals.JournalViewModels.Orders;
using VodovozInfrastructure.Extensions;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class DiscountReasonJournalRegistrar : ColumnsConfigRegistrarBase<DiscountReasonJournalViewModel, DiscountReasonJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<DiscountReasonJournalNode> config) =>
			config.AddColumn("Код").AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("В архиве?").AddTextRenderer(node => node.IsArchive.ConvertToYesOrEmpty())
				.AddColumn("")
				.Finish();
	}
}
