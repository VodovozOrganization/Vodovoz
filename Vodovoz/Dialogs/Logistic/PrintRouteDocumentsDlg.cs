using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
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
		PrintSettings PrintSettings;

		bool showDialog = true;

		public PrintRouteDocumentsDlg()
		{
			this.Build();

			TabName = "Печать МЛ";

			ytreeRoutes.ColumnsConfig = FluentColumnsConfig<PrintRoute>.Create()
				.AddColumn("Печатать").AddToggleRenderer(x => x.Selected)
				.AddSetter((c, n) => c.Visible = n.RouteList.Status >= RouteListStatus.InLoading)
				.AddColumn("Номер").AddTextRenderer(x => x.RouteList.Id.ToString())
				.AddColumn("Статус").AddTextRenderer(x => x.RouteList.Status.GetEnumTitle())
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
			var notPrintedRoutes = Routes.Where(x => x.RouteList.Status < RouteListStatus.InLoading).ToList();
			if(notPrintedRoutes.Count > 0)
				MessageDialogWorks.RunWarningDialog(String.Format("Маршрутные листы {0} не могут быть напечатаны, так как еще не подтверждены.",
				                                                  String.Join(", ", notPrintedRoutes.Select(x => $"{x.RouteList.Id}({x.RouteList.Driver.ShortName})"))));
		}

		protected void OnCheckSelectAllToggled(object sender, EventArgs e)
		{
			Routes.Where(x => x.RouteList.Status >= RouteListStatus.InLoading).ToList().ForEach(x => x.Selected = checkSelectAll.Active);
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			var docCount = (checkRoute.Active ? 1 : 0) + (checkDailyList.Active ? 1 : 0);
			var routeCount = Routes.Count(x => x.Selected);
			progressPrint.Adjustment.Upper = docCount * routeCount;
			progressPrint.Adjustment.Value = 0;
			showDialog = true;
			bool needCommit = false;

			foreach(var route in Routes.Where(x => x.Selected))
			{
				progressPrint.Text = String.Format("Печатаем МЛ {0} - {1}", route.RouteList.Id, route.RouteList.Driver.ShortName);
				QSMain.WaitRedraw();
				bool printed = false;

				if(checkDailyList.Active) {
					PrintDoc(route.RouteList, RouteListPrintableDocuments.DailyList, PageOrientation.Portrait, 1);
					progressPrint.Adjustment.Value++;
					QSMain.WaitRedraw();
				}

				if(checkRoute.Active) {
					PrintDoc(route.RouteList, RouteListPrintableDocuments.RouteList, PageOrientation.Landscape, spinRoute.ValueAsInt);
					progressPrint.Adjustment.Value++;
					QSMain.WaitRedraw();
					printed = true;
				}

				if(printed)
				{
					route.RouteList.Printed = true;
					uow.Save(route.RouteList);
					needCommit = true;
				}
			}

			if(needCommit)
				uow.Commit();
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
			//Printer.DefaultPageSetup = new PageSetup();

			if(PrintSettings == null)
			{
				Printer.PrintSettings = new PrintSettings();
			}
			else
			{
				Printer.PrintSettings = PrintSettings;
			}

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
		}
	}
}
