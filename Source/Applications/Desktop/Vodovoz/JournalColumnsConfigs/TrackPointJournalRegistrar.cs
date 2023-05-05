using Gamma.ColumnConfig;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;
using Vodovoz.ViewModels.Journals.JournalViewModels.Logistic;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class TrackPointJournalRegistrar : ColumnsConfigRegistrarBase<TrackPointJournalViewModel, TrackPointJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<TrackPointJournalNode> config) =>
			config.AddColumn("Номер МЛ").AddNumericRenderer(node => node.RouteListId)
				.AddColumn("Время").AddTextRenderer(node => node.Time.ToString("G"))
				.AddColumn("Широта").AddNumericRenderer(node => node.Latitude).Digits(8)
				.AddColumn("Долгота").AddNumericRenderer(node => node.Longitude).Digits(8)
				.AddColumn("Время получения").AddTextRenderer(node => node.ReceiveTime.ToString("G"))
				.Finish();
	}
}
