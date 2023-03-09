using Gamma.ColumnConfig;
using Vodovoz.JournalViewModels;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class UserRolesJournalRegistrar : ColumnsConfigRegistrarBase<UserRolesJournalViewModel, UserRolesJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<UserRolesJournalNode> config) =>
			config.AddColumn("Код")
					.AddNumericRenderer(node => node.Id)
				.AddColumn("Название")
					.AddTextRenderer(node => node.Name)
				.AddColumn("Описание роли")
					.AddTextRenderer(node => node.Description)
				.AddColumn("")
				.Finish();
	}
}
