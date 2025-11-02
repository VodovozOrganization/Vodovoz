using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Domain.Fuel;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Fuel.FuelCards;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class FuelCardJournalRegistrar : ColumnsConfigRegistrarBase<FuelCardJournalViewModel, FuelCard>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<FuelCard> config) =>
			config.AddColumn("Id").AddNumericRenderer(node => node.Id)
				.AddColumn("Номер карты").AddTextRenderer(node => node.CardNumber).WrapWidth(300).WrapMode(WrapMode.WordChar)
				.AddColumn("В архиве").AddToggleRenderer(node => node.IsArchived).Editing(false).XAlign(0f)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchived ? GdkColors.InsensitiveText : GdkColors.PrimaryText)
				.Finish();
	}
}
