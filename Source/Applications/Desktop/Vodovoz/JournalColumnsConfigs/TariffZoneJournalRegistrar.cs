using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;
using VodovozInfrastructure.Extensions;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class TariffZoneJournalRegistrar : ColumnsConfigRegistrarBase<TariffZoneJournalViewModel, TariffZoneJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<TariffZoneJournalNode> config) =>
			config.AddColumn("Код").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("Доступна\nдоставка за час").AddTextRenderer(node => node.IsFastDeliveryAvailable.ConvertToYesOrEmpty()).XAlign(0.5f)
				.AddColumn("Время работы\nдоставки за час").AddTextRenderer(node => node.FastDeliveryAvailableTime)
				.Finish();
	}
}
