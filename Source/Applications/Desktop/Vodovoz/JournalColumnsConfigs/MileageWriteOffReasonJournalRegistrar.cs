using Gamma.ColumnConfig;
using Gtk;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Logistic.MileagesWriteOff;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class MileageWriteOffReasonJournalRegistrar : ColumnsConfigRegistrarBase<MileageWriteOffReasonJournalViewModel, MileageWriteOffReason>
	{
		public override IColumnsConfig Configure(FluentColumnsConfig<MileageWriteOffReason> config) =>
			config.AddColumn("Id").AddNumericRenderer(node => node.Id)
				.AddColumn("Название").AddTextRenderer(node => node.Name).WrapWidth(400).WrapMode(WrapMode.WordChar)
				.AddColumn("В архиве").AddToggleRenderer(node => node.IsArchived).Editing(false).XAlign(0f)
				.RowCells().AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.IsArchived ? GdkColors.InsensitiveText : GdkColors.PrimaryText)
				.Finish();
	}
}
