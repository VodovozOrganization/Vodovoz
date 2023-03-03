using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gdk;
using Gtk;
using Vodovoz.Domain.Logistic;
using Vodovoz.JournalViewModels;
using Vodovoz.ViewModels.Journals.JournalNodes;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class RouteListWorkingJournalRegistrar : ColumnsConfigRegistrarBase<RouteListWorkingJournalViewModel, RouteListJournalNode>
	{
		private static readonly Color _colorOrange = new Color(0xfc, 0x66, 0x00);
		private static readonly Color _colorBlack = new Color(0, 0, 0);
		private static readonly Color _colorWhite = new Color(0xff, 0xff, 0xff);
		private static readonly Color _colorLightGray = new Color(0xcc, 0xcc, 0xcc);

		public override IColumnsConfig Configure(FluentColumnsConfig<RouteListJournalNode> config) =>
			config.AddColumn("Номер")
					.AddTextRenderer(node => node.Id.ToString())
				.AddColumn("Дата")
					.AddTextRenderer(node => node.Date.ToString("d"))
				.AddColumn("Смена")
					.AddTextRenderer(node => node.ShiftName)
				.AddColumn("Статус")
					.AddTextRenderer(node => node.StatusEnum.GetEnumTitle())
				.AddColumn("Водитель и машина")
					.AddTextRenderer(node => node.DriverAndCar)
				.AddColumn("Сдается в кассу")
					.AddTextRenderer(node => node.ClosingSubdivision)
				.AddColumn("Комментарий ЛО")
					.AddTextRenderer(node => node.LogisticiansComment)
					.WrapWidth(300)
					.WrapMode(WrapMode.WordChar)
				.AddColumn("Комментарий по закрытию")
					.AddTextRenderer(node => node.ClosinComments)
					.WrapWidth(300)
					.WrapMode(WrapMode.WordChar)
				.AddColumn("Комментарий по водителю")
					.AddTextRenderer(node => node.DriverComment)
					.WrapWidth(300)
					.WrapMode(WrapMode.WordChar)
				.RowCells()
					.AddSetter<CellRendererText>((c, n) => c.ForegroundGdk = n.NotFullyLoaded ? _colorOrange : _colorBlack)
				.AddSetter<CellRenderer>(
					(c, n) =>
					{
						var color = _colorWhite;

						if(n.StatusEnum != RouteListStatus.New
							&& n.GrossMarginPercents.HasValue
							&& n.GrossMarginPercents.Value != 0
							&& n.GrossMarginPercents.Value < n.RouteListProfitabilityIndicator)
						{
							color = _colorLightGray;
						}

						c.CellBackgroundGdk = color;
					})
				.Finish();
	}
}
