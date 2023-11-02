using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Logistic;

namespace Vodovoz.JournalColumnsConfigs
{
	public class RouteListMileageCheckJournalRegistrar : ColumnsConfigRegistrarBase<RouteListMileageCheckJournalViewModel, RouteListJournalNode>
	{
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
				.WrapMode(Pango.WrapMode.WordChar)
			.AddColumn("Комментарий по закрытию")
				.AddTextRenderer(node => node.ClosinComments)
				.WrapWidth(300)
				.WrapMode(Pango.WrapMode.WordChar)
			.AddColumn("Комментарий по водителю")
				.AddTextRenderer(node => node.DriverComment)
				.WrapWidth(300)
				.WrapMode(Pango.WrapMode.WordChar)
			.RowCells()
				.AddSetter<CellRendererText>((c, n) =>
				{
					c.ForegroundGdk = n.NotFullyLoaded ? GdkColors.Orange : GdkColors.PrimaryText;
				})
				.Finish();
	}
}
