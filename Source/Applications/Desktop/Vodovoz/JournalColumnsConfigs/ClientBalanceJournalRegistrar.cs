using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Counterparties;
using static Vodovoz.ViewModels.Counterparties.ClientEquipmentBalanceJournalViewModel;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class ClientBalanceJournalRegistrar : ColumnsConfigRegistrarBase<ClientEquipmentBalanceJournalViewModel, ClientEquipmentBalanceNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<ClientEquipmentBalanceNode> config) =>
			config.AddColumn("Номенклатура").AddTextRenderer(node => node.NomenclatureName)
				.AddColumn("Серийный номер").AddTextRenderer(node => node.SerialNumber)
				.AddColumn("Наше").AddToggleRenderer(node => node.IsOur)
				.AddColumn("Клиент").AddTextRenderer(node => node.Client)
				.AddColumn("Адрес").AddTextRenderer(node => node.Address)
				.Finish();
	}
}
