using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.JournalNodes;
using Vodovoz.JournalViewModels;
using VodovozInfrastructure.Extensions;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class PromotionalSetsJournalRegistrar : ColumnsConfigRegistrarBase<PromotionalSetsJournalViewModel, PromotionalSetJournalNode>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<PromotionalSetJournalNode> config) =>
			config.AddColumn("Код")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Id.ToString())
				.AddColumn("Название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.Name)
				.AddColumn("Основание скидки")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(n => n.PromoSetDiscountReasonName)
				.AddColumn("В архиве?")
					.HeaderAlignment(0.5f)
					.AddTextRenderer()
					.AddSetter((c, n) => c.Text = n.IsArchive.ConvertToYesOrEmpty())
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchive ? GdkColors.DarkGrayColor : GdkColors.BlackColor)
				.Finish();
	}
}
