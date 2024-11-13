using Gamma.ColumnConfig;
using Vodovoz.Domain.Logistic;
using Vodovoz.Presentation.ViewModels.Logistic;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class AcceptBeforeJournalRegistrar : ColumnsConfigRegistrarBase<AcceptBeforeJournalViewModel, AcceptBefore>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<AcceptBefore> config) =>
			config
				.AddColumn("Номер").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("")
				.Finish();
	}
}
