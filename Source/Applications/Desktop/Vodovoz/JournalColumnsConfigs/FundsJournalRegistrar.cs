using Gamma.ColumnConfig;
using Vodovoz.Extensions;
using Vodovoz.Presentation.ViewModels.Organisations.Journals;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class FundsJournalRegistrar : ColumnsConfigRegistrarBase<FundsJournalViewModel, FundsJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<FundsJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(x => x.Id)
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.AddColumn("Заполнение р/с по умолчанию")
					.AddTextRenderer(x => x.DefaultAccountFillType.GetEnumDisplayName(false))
				.AddColumn("Архив").AddToggleRenderer(x => x.IsArchive).Editing(false)
				.AddColumn("")
				.Finish();
	}
}
