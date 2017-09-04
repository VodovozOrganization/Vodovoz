using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gtk;
using QSOrmProject;
using QSProjectsLib;
using QSReport;
using QSTDI;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Logistic;

namespace Vodovoz.Dialogs.Logistic
{
	public partial class PrintRouteDocumentsDlg : TdiTabBase
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		Gdk.Pixbuf vodovozCarIcon = Gdk.Pixbuf.LoadFromResource("Vodovoz.icons.buttons.vodovoz-logo.png");

		List<PrintRoute> Routes = new List<PrintRoute>();

		IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();

		Gtk.PrintOperation Printer;
		Gtk.PageSetup PageSetup;
		PrintSettings PrintSettings;

		bool showDialog = true;

		public PrintRouteDocumentsDlg()
		{
			this.Build();

			TabName = "Печать МЛ";

			ytreeRoutes.ColumnsConfig = FluentColumnsConfig<PrintRoute>.Create()
				.AddColumn("Печатать").AddToggleRenderer(x => x.Selected)
				.AddColumn("Номер").AddTextRenderer(x => x.RouteList.Id.ToString())
				.AddColumn("Водитель").AddTextRenderer(x => x.RouteList.Driver.ShortName)
				.AddColumn("Автомобиль")
				.AddPixbufRenderer(x => x.RouteList.Car != null && x.RouteList.Car.IsCompanyHavings ? vodovozCarIcon : null)
					.AddTextRenderer(x => x.RouteList.Car != null ? x.RouteList.Car.RegistrationNumber : "нет")
				.AddColumn("")
				.Finish();

			ydatePrint.Date = DateTime.Today;
		}


		class PrintRoute : PropertyChangedBase
		{
			private bool selected;

			public virtual bool Selected {
				get { return selected; }
				set { SetField(ref selected, value, () => Selected); }
			}

			public RouteList RouteList;

			public PrintRoute(RouteList routeList)
			{
				RouteList = routeList;
			}
		}

		protected void OnYdatePrintDateChanged(object sender, EventArgs e)
		{
			UpdateRouteList();
		}

		private void UpdateRouteList()
		{
			var routeQuery = Repository.Logistics.RouteListRepository.GetRoutesAtDay(ydatePrint.Date).GetExecutableQueryOver(uow.Session);
			Routes = routeQuery.Fetch(x => x.Driver).Eager
								 .Fetch(x => x.Car).Eager
								 .List()
			                     .Select(x => new PrintRoute(x))
			                     .ToList();
			ytreeRoutes.SetItemsSource<PrintRoute>(new GenericObservableList<PrintRoute>(Routes));
		}

		protected void OnCheckSelectAllToggled(object sender, EventArgs e)
		{
			Routes.ForEach(x => x.Selected = checkSelectAll.Active);
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			var docCount = (checkRoute.Active ? 1 : 0) + (checkDailyList.Active ? 1 : 0);
			var routeCount = Routes.Count(x => x.Selected);
			progressPrint.Adjustment.Upper = docCount * routeCount;
			progressPrint.Adjustment.Value = 0;
			showDialog = true;

			foreach(var route in Routes.Where(x => x.Selected))
			{
				progressPrint.Text = String.Format("Печатаем МЛ {0} - {1}", route.RouteList.Id, route.RouteList.Driver.ShortName);
				QSMain.WaitRedraw();

				if(checkDailyList.Active) {
					PrintDoc(route.RouteList, RouteListPrintableDocuments.DailyList, PageOrientation.Portrait, 1);
					progressPrint.Adjustment.Value++;
					QSMain.WaitRedraw();
				}

				if(checkRoute.Active) {
					PrintDoc(route.RouteList, RouteListPrintableDocuments.RouteList, PageOrientation.Landscape, spinRoute.ValueAsInt);
					progressPrint.Adjustment.Value++;
					QSMain.WaitRedraw();
				}

			}
			progressPrint.Text = "Готово";
		}

		private void PrintDoc(RouteList route, RouteListPrintableDocuments type, PageOrientation orientation, int copies)
		{
			var reportInfo = PrintRouteListHelper.GetRDL(route, type, uow);

			var action = showDialog ? PrintOperationAction.PrintDialog : PrintOperationAction.Print;
			showDialog = false;

			Printer = new PrintOperation();
			Printer.Unit = Unit.Points;
			Printer.UseFullPage = true;

			if(PrintSettings == null)
			{
				Printer.DefaultPageSetup = new PageSetup();
				Printer.PrintSettings = new PrintSettings();
			}
			else
			{
				Printer.DefaultPageSetup = PageSetup;
				Printer.PrintSettings = PrintSettings;
			}

			Printer.DefaultPageSetup.Orientation = orientation;
			Printer.PrintSettings.Orientation = orientation;

			var rprint = new ReportPrinter(reportInfo);
			rprint.PrepareReport();

			Printer.NPages = rprint.PageCount;
			Printer.PrintSettings.NCopies = copies;
			if(copies > 1)
				Printer.PrintSettings.Collate = true;

			Printer.DrawPage += rprint.DrawPage;
			Printer.Run(action, null);

			PrintSettings = Printer.PrintSettings;
			PageSetup = Printer.DefaultPageSetup;
		}
	}
}
