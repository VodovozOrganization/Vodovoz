using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Complaints;
using Vodovoz.ViewModels.Journals.JournalViewModels.Complaints;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class ResponsibleJournalRegistrar : ColumnsConfigRegistrarBase<ResponsibleJournalViewModel, ResponsibleJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<ResponsibleJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("В архиве").AddToggleRenderer(node => node.IsArchived).Editing(false)
				.AddColumn("")
				.Finish();
	}
}
