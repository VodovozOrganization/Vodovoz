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
			.AddColumn("Есть регистрация онлайн кассы")
				.AddToggleRenderer(node => node.HasCashBoxId).Editing(false)
			.AddColumn("Есть регистрация в Авангарде")
				.AddToggleRenderer(node => node.HasAvangardShopId).Editing(false)
			.AddColumn("Есть регистрация в Такскоме(ЭДО)")
				.AddToggleRenderer(node => node.HasTaxcomEdoAccountId).Editing(false)
			.AddColumn("Рассылка писем о задолженности")
				.AddToggleRenderer(node => node.SendDebtLetters).Editing(false)
			.AddColumn("Письма о задолженности с подписью и печатью")
				.AddToggleRenderer(node => node.SendDebtLettersWithASignatureAndSeal).Editing(false)
			.AddColumn("")
			.Finish();
	}
}
