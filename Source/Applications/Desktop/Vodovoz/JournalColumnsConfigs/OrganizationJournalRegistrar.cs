using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Organizations;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class OrganizationJournalRegistrar : ColumnsConfigRegistrarBase<OrganizationJournalViewModel, OrganizationJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<OrganizationJournalNode> config) =>
		config.AddColumn("Код")
				.AddNumericRenderer(node => node.Id).WidthChars(4)
			.AddColumn("Название")
				.AddTextRenderer(node => node.Name)
			.AddColumn("")
			.Finish();
	}
}
