using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Cash;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class FinancialResponsibilityCenterJournalViewModelRegistrar : ColumnsConfigRegistrarBase<FinancialResponsibilityCenterJournalViewModel, FinancialResponsibilityCenterNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<FinancialResponsibilityCenterNode> config) =>
			config
				.AddColumn("Номер").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name)
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRendererText>((c, n) =>
						c.ForegroundGdk = n.IsArchive ? GdkColors.InsensitiveText : GdkColors.PrimaryText)
				.Finish();
	}
}
