using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Organizations;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class AccountJournalRegistrar : ColumnsConfigRegistrarBase<AccountJournalViewModel, AccountJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<AccountJournalNode> config) =>
		config.AddColumn("Код")
				.AddNumericRenderer(node => node.Id)
			.AddColumn("Основной")
				.AddTextRenderer(node => node.IsDefault ? "✔️": "")
			.AddColumn("Псевдоним")
				.AddTextRenderer(node => node.Alias)
			.AddColumn("В банке")
				.AddTextRenderer(node => node.BankName)
			.AddColumn("Номер")
				.AddTextRenderer(node => node.AccountNumber)
			.AddColumn("")
			.Finish();
	}
}
