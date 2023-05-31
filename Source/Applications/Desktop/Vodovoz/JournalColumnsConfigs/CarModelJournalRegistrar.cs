using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Journals.JournalNodes.Logistic;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class CarModelJournalRegistrar : ColumnsConfigRegistrarBase<CarModelJournalViewModel, CarModelJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<CarModelJournalNode> config) =>
			config.AddColumn("Код").HeaderAlignment(0.5f).AddTextRenderer(n => n.Id.ToString()).XAlign(0.5f)
				.AddColumn("Производитель").HeaderAlignment(0.5f).AddTextRenderer(n => n.ManufactererName).XAlign(0.5f)
				.AddColumn("Название").HeaderAlignment(0.5f).AddTextRenderer(n => n.Name).XAlign(0.5f)
				.AddColumn("Тип").HeaderAlignment(0.5f).AddTextRenderer(n => n.TypeOfUse.GetEnumTitle()).XAlign(0.5f)
				.AddColumn("")
				.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? GdkColors.DarkGrayColor : GdkColors.BlackColor)
				.Finish();
	}
}
