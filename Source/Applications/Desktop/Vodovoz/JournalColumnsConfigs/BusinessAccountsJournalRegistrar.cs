using Gamma.ColumnConfig;
using Vodovoz.Extensions;
using Vodovoz.Presentation.ViewModels.Organisations.Journals;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class BusinessAccountsJournalRegistrar : ColumnsConfigRegistrarBase<BusinessAccountsJournalViewModel, BusinessAccountJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<BusinessAccountJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(x => x.Id)
				.AddColumn("Название").AddTextRenderer(x => x.Name)
				.AddColumn("Номер").AddTextRenderer(x => x.Number)
				.AddColumn("Форма ДС").AddTextRenderer(x => x.Funds)
				.AddColumn("Банк").AddTextRenderer(x => x.Bank)
				.AddColumn("Направление").AddTextRenderer(x => x.BusinessActivity)
				.AddColumn("Заполнение р/с")
					.AddTextRenderer(x => x.AccountFillType.GetEnumDisplayName(false))
				.AddColumn("Архив").AddToggleRenderer(x => x.IsArchive).Editing(false)
				.AddColumn("")
				.Finish();
	}
}
