using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using QSProjectsLib;
using Vodovoz.Domain.Logistic;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Journals.JournalNodes;
using Vodovoz.ViewModels.Logistic;
using WrapMode = Pango.WrapMode;

namespace Vodovoz.JournalColumnsConfigs
{
	internal sealed class RouteListJournalRegistrar : ColumnsConfigRegistrarBase<RouteListJournalViewModel, RouteListJournalNode>
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
				.AddColumn("Долг по МЛ")
					.AddTextRenderer(node => node.RouteListDebt.ToShortCurrencyString())
				.AddColumn("Комментарий ЛО")
					.AddTextRenderer(node => node.LogisticiansComment)
					.WrapWidth(300)
					.WrapMode(WrapMode.WordChar)
					.AddSetter((c, x) => c.Xpad = 50)
				.AddColumn("Комментарий по закрытию")
					.AddTextRenderer(node => node.ClosinComments)
					.WrapWidth(300)
					.WrapMode(WrapMode.WordChar)
				.AddColumn("Комментарий по водителю")
					.AddTextRenderer(node => node.DriverComment)
					.WrapWidth(300)
					.WrapMode(WrapMode.WordChar)
				.RowCells()
					.AddSetter<CellRendererText>((c, n) =>
						c.ForegroundGdk = n.NotFullyLoaded ? GdkColors.Orange : GdkColors.PrimaryText)
					.AddSetter<CellRenderer>(
						(c, n) =>
						{
							var color = GdkColors.PrimaryBase;

							if(n.StatusEnum != RouteListStatus.New
								&& n.GrossMarginPercents.HasValue
								&& n.GrossMarginPercents.Value != 0
								&& n.GrossMarginPercents.Value < n.RouteListProfitabilityIndicator)
							{
								color = GdkColors.InsensitiveBase;
							}

							c.CellBackgroundGdk = color;
						})
				.Finish();
	}
}
