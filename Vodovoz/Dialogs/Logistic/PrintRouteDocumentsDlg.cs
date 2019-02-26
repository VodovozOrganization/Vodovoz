using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.Utilities;
using Gtk;
using NHibernate;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QSProjectsLib;
using QSReport;
using Vodovoz.Additions.Logistic;
using Vodovoz.Additions.Printing;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Dialogs.Logistic
{
	public partial class PrintRouteDocumentsDlg : QS.Dialog.Gtk.TdiTabBase
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		Gdk.Pixbuf vodovozCarIcon = Gdk.Pixbuf.LoadFromResource("Vodovoz.icons.buttons.vodovoz-logo.png");

		List<SelectablePrintDocument> Routes = new List<SelectablePrintDocument>();
		GenericObservableList<OrderDocTypeNode> OrderDocTypesToPrint = new GenericObservableList<OrderDocTypeNode>();
		GenericObservableList<GeographicGroup> geographicGroups;
		GenericObservableList<string> warnings;

		IUnitOfWork uow = UnitOfWorkFactory.CreateWithoutRoot();

		public PrintRouteDocumentsDlg()
		{
			this.Build();

			TabName = "Массовая печать документов МЛ";

			chkDocumentsOfOrders.Toggled += ChkDocumentsOfOrders_Toggled;
			ytreeRoutes.ColumnsConfig = FluentColumnsConfig<SelectablePrintDocument>.Create()
				.AddColumn("Печатать")
					.AddToggleRenderer(x => x.Selected)
					.AddSetter((c, n) => c.Visible = (n.Document as RouteListPrintableDocs).routeList.Status >= RouteListStatus.Confirmed)
				.AddColumn("Номер")
					.AddTextRenderer(x => (x.Document as RouteListPrintableDocs).routeList.Id.ToString())
				.AddColumn("Статус")
					.AddTextRenderer(x => (x.Document as RouteListPrintableDocs).routeList.Status.GetEnumTitle())
				.AddColumn("Водитель")
					.AddTextRenderer(x => (x.Document as RouteListPrintableDocs).routeList.Driver.ShortName)
				.AddColumn("Автомобиль")
					.AddPixbufRenderer(x => (x.Document as RouteListPrintableDocs).routeList.Car != null && (x.Document as RouteListPrintableDocs).routeList.Car.IsCompanyHavings ? vodovozCarIcon : null)
					.AddTextRenderer(x => (x.Document as RouteListPrintableDocs).routeList.Car != null ? (x.Document as RouteListPrintableDocs).routeList.Car.RegistrationNumber : "нет")
				.AddColumn("Часть города")
					.AddTextRenderer(x => string.Join(", ", (x.Document as RouteListPrintableDocs).routeList.GeographicGroups.Select(g => g.Name)))
				.AddColumn("МЛ распечатан")
					.AddTextRenderer(x => (x.Document as RouteListPrintableDocs).routeList.Printed ? "МЛ распечатан ранее" : "Не печатался")
				.AddColumn("")
				.RowCells()
					.AddSetter<CellRendererText>((c, n) => c.Foreground = (n.Document as RouteListPrintableDocs).routeList.Printed ? "grey" : "black")
				.Finish();

			geograficGroup.UoW = uow;
			geograficGroup.Label = "Часть города:";
			geographicGroups = new GenericObservableList<GeographicGroup>();
			geograficGroup.Items = geographicGroups;
			geograficGroup.ListContentChanged += OnYdatePrintDateChanged;
			foreach(var gg in uow.Session.QueryOver<GeographicGroup>().List())
				geographicGroups.Add(gg);

			ydatePrint.Date = DateTime.Today;

			OrderDocumentType[] selectedByDefault = {
				OrderDocumentType.Invoice,
				OrderDocumentType.InvoiceBarter,
				OrderDocumentType.InvoiceContractDoc,
				OrderDocumentType.Bill,
				OrderDocumentType.UPD,
				OrderDocumentType.SpecialBill,
				OrderDocumentType.SpecialUPD,
				OrderDocumentType.DriverTicket,
				OrderDocumentType.M2Proxy,
				OrderDocumentType.EquipmentTransfer,
				OrderDocumentType.DoneWorkReport,
				OrderDocumentType.EquipmentReturn,
				OrderDocumentType.PumpWarranty,
				OrderDocumentType.CoolerWarranty,
				OrderDocumentType.Torg12,
				OrderDocumentType.ShetFactura,
				OrderDocumentType.RefundBottleDeposit,
				OrderDocumentType.RefundEquipmentDeposit,
				OrderDocumentType.BottleTransfer,
				OrderDocumentType.ProductCertificate
			};

			foreach(OrderDocumentType t in Enum.GetValues(typeof(OrderDocumentType)))
				OrderDocTypesToPrint.Add(new OrderDocTypeNode(t, selectedByDefault.Contains(t)));

			yTreeOrderDocumentTypes.ColumnsConfig = FluentColumnsConfig<OrderDocTypeNode>.Create()
				.AddColumn("Печатать")
					.AddToggleRenderer(x => x.Selected)
					.Editing()
					.ChangeSetProperty(PropertyUtil.GetPropertyInfo<OrderDocTypeNode>(x => x.Selected))
				.AddColumn("Название").AddTextRenderer(x => x.Type.GetEnumTitle())
				.AddColumn("")
				.Finish();
			yTreeOrderDocumentTypes.SetItemsSource(OrderDocTypesToPrint);
			yTreeOrderDocumentTypes.HeadersVisible = false;
			ChkDocumentsOfOrders_Toggled(this, null);

			warnings = new GenericObservableList<string>();
			yTreeViewWarnings.HeadersVisible = false;
			yTreeViewWarnings.ColumnsConfig = FluentColumnsConfig<string>.Create()
				.AddColumn("")
					.AddTextRenderer(x => x)
				.RowCells()
					.AddSetter<CellRendererText>((c,n)=>c.Foreground = "red")
				.Finish();
			yTreeViewWarnings.SetItemsSource(warnings);
		}

		class OrderDocTypeNode : PropertyChangedBase
		{
			bool selected;

			public OrderDocTypeNode(OrderDocumentType type, bool selected)
			{
				Type = type;
				Selected = selected;
			}

			public virtual bool Selected {
				get => selected;
				set => SetField(ref selected, value, () => Selected);
			}
			public OrderDocumentType Type { get; set; }
		}

		void ChkDocumentsOfOrders_Toggled(object sender, EventArgs e)
		{
			gtkScrlWnd.Visible = chkDocumentsOfOrders.Active;
		}

		protected void OnYdatePrintDateChanged(object sender, EventArgs e) => UpdateRouteList();

		private void UpdateRouteList()
		{
			gtkScrollWndWarnings.Visible = false;
			var ggIds = geographicGroups.Select(x => x.Id).ToList();
			var routeQuery = Repository.Logistics.RouteListRepository.GetRoutesAtDay(ydatePrint.Date, ggIds).GetExecutableQueryOver(uow.Session);
			Routes = routeQuery.Fetch(SelectMode.Fetch, x => x.Driver)
							   .Fetch(SelectMode.Fetch, x => x.Car)
							   .List()
							   .Select(x => new SelectablePrintDocument(new RouteListPrintableDocs(uow, x, RouteListPrintableDocuments.RouteList)))
							   .OrderBy(x => (x.Document as RouteListPrintableDocs).routeList.Driver.LastName)
							   .ToList()
							   .Distinct()
							   .ToList()
							   ;
			ytreeRoutes.SetItemsSource(new GenericObservableList<SelectablePrintDocument>(Routes));
			var notPrintedRoutes = Routes.Where(x => (x.Document as RouteListPrintableDocs).routeList.Status < RouteListStatus.Confirmed).ToList();

			if(notPrintedRoutes.Any())
				MessageDialogHelper.RunWarningDialog(
					String.Format(
						"Маршрутные листы {0} не могут быть напечатаны, так как еще не подтверждены.",
						String.Join(", ", notPrintedRoutes.Select(x => $"{(x.Document as RouteListPrintableDocs).routeList.Id}({(x.Document as RouteListPrintableDocs).routeList.Driver.ShortName})"))
					)
				);
		}

		protected void OnCheckSelectAllToggled(object sender, EventArgs e)
		{
			Routes.Where(x => (x.Document as RouteListPrintableDocs).routeList.Status >= RouteListStatus.Confirmed).ToList().ForEach(x => x.Selected = checkSelectAll.Active);
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			var routeCount = Routes.Count(x => x.Selected);
			progressPrint.Adjustment.Upper = routeCount;
			progressPrint.Adjustment.Value = 0;

			foreach(var item in Routes.Where(x => x.Selected)) {
				if(item.Document is RouteListPrintableDocs rlPrintableDoc) {
					progressPrint.Text = String.Format("Печатаем МЛ {0} - {1}", rlPrintableDoc.routeList.Id, rlPrintableDoc.routeList.Driver.ShortName);
					QSMain.WaitRedraw();
					var rlDocTypesToPrint = new List<RouteListPrintableDocuments>();
					OrderDocumentType[] oDocTypesToPrint = null;

					if(checkRoute.Active)
						rlDocTypesToPrint.Add(RouteListPrintableDocuments.RouteList);
					if(checkRouteMap.Active)
						rlDocTypesToPrint.Add(RouteListPrintableDocuments.RouteMap);
					if(chkLoadDocument.Active)
						rlDocTypesToPrint.Add(RouteListPrintableDocuments.LoadDocument);
					if(chkDocumentsOfOrders.Active)
						oDocTypesToPrint = OrderDocTypesToPrint.Where(n => n.Selected)
															   .Select(n => n.Type)
															   .ToArray();

					bool cancelPrinting = false;
					EntitiyDocumentsPrinter printer = new EntitiyDocumentsPrinter(
						uow,
						rlPrintableDoc.routeList,
						rlDocTypesToPrint.ToArray(),
						oDocTypesToPrint
					);
					printer.PrintingCanceled += (s, ea) => {
						cancelPrinting = true;
					};
					printer.Print();
					if(!string.IsNullOrEmpty(printer.ODTTemplateNotFoundMessages)) {
						gtkScrollWndWarnings.Visible = true;
						warnings.Add(string.Format("МЛ №{0} - {1}:", rlPrintableDoc.routeList.Id, rlPrintableDoc.routeList.Driver.ShortName));
						warnings.Add(printer.ODTTemplateNotFoundMessages);
					}
					if(cancelPrinting) {
						progressPrint.Text = "Печать отменена";
						break;
					}
				}
				progressPrint.Text = "Готово";

				progressPrint.Adjustment.Value++;
			}
			EntitiyDocumentsPrinter.PrinterSettings = null;
		}
	}
}