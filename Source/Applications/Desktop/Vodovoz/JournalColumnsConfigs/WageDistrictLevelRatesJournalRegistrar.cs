using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.WageCalculation;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class WageDistrictLevelRatesJournalRegistrar : ColumnsConfigRegistrarBase<WageDistrictLevelRatesJournalViewModel, WageDistrictLevelRatesJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<WageDistrictLevelRatesJournalNode> config) =>
			config.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Id.ToString())
				.AddColumn("Название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Name)
				.AddColumn("По умолчанию для новых сотрудников (Найм)")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.IsDefaultLevelString)
				.AddColumn("По умолчанию для новых сотрудников (Наши авто)")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.IsDefaultLevelOurCarsString)
				.AddColumn("По умолчанию для новых сотрудников (Для авто в раскате)")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.IsDefaultLevelRaskatCarsString)
				.AddColumn("В архиве?")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.IsArchiveString)
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRendererText>((c, n) =>
					{
						c.ForegroundGdk = n.IsArchive ? GdkColors.InsensitiveText : GdkColors.PrimaryText;
					})
				.Finish();
	}
}
