using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.DomainModel.UoW;
using QS.Report;
using QSDocTemplates;
using QSProjectsLib;
using QSReport;
using QS.Tdi;
using Vodovoz.Additions.Logistic;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.Dialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DocumentsPrinterDlg : QS.Dialog.Gtk.TdiTabBase
	{
		Order currentOrder;
		RouteList currentRouteList;
		MultipleDocumentPrinter multipleDocumentPrinter = new MultipleDocumentPrinter();
		List<SelectablePrintDocument> printDocuments = new List<SelectablePrintDocument>();
		SelectablePrintDocument selectedDocument;
		public event EventHandler DocumentsPrinted;

		public DocumentsPrinterDlg(Order order)
		{
			this.Build();

			TabName = "Печать документов заказа";

			currentOrder = order;
			bool? successfulUpdate = null;
			foreach(var item in currentOrder.OrderDocuments) {
				if(item is IPrintableOdtDocument) {
					switch(item.Type) {
						case OrderDocumentType.AdditionalAgreement:
							if((item as IPrintableOdtDocument).GetTemplate() == null)
								successfulUpdate = (item as OrderAgreement).AdditionalAgreement.UpdateContractTemplate(currentOrder.UoW);
							(item as OrderAgreement).PrepareTemplate(currentOrder.UoW);
							break;
						case OrderDocumentType.Contract:
							if((item as IPrintableOdtDocument).GetTemplate() == null)
								successfulUpdate = (item as OrderContract).Contract.UpdateContractTemplate(currentOrder.UoW);
							(item as OrderContract).PrepareTemplate(currentOrder.UoW);
							break;
						case OrderDocumentType.M2Proxy:
							if((item as IPrintableOdtDocument).GetTemplate() == null)
								successfulUpdate = (item as OrderM2Proxy).M2Proxy.UpdateM2ProxyDocumentTemplate(currentOrder.UoW);
							(item as OrderM2Proxy).PrepareTemplate(currentOrder.UoW);
							break;
						default:
							throw new NotImplementedException("Документ не поддерживается");
					}
					if(successfulUpdate == false) {
						MessageDialogWorks.RunWarningDialog(
							String.Format("Документ '{0}' в комплект печати добавлен не был, т.к. " +
										  "для него не установлен шаблон документа и не удалось найти подходящий.", item.Name)
						);
						continue;
					} else if(successfulUpdate == true) {
						/*MessageDialogWorks.RunInfoDialog(
							String.Format("Для документа '{0}' успешно подобран шаблон, который был добавлен в комплект печати. " +
										  "Для сохранения шаблона документа необходимо нажать кнопку 'Сохранить' в заказе.", item.Name)
						);*/
					}
				}
				printDocuments.Add(new SelectablePrintDocument(item, DefaultCopies(item.Type)) { Selected = true });
			}

			Configure();
		}

		public DocumentsPrinterDlg(IUnitOfWork uow, RouteList routeList, RouteListPrintableDocuments selectedType)
		{
			this.Build();
			TabName = "Печать документов МЛ";
			currentRouteList = routeList;

			foreach(RouteListPrintableDocuments rlDocType in Enum.GetValues(typeof(RouteListPrintableDocuments))) {
				if(rlDocType == RouteListPrintableDocuments.LoadDocument || rlDocType == RouteListPrintableDocuments.All)
					continue;
				var rlDoc = new RouteListPrintableDocs(uow, currentRouteList, rlDocType);
				bool isSelected = selectedType == RouteListPrintableDocuments.All || selectedType == rlDocType;
				SelectablePrintDocument doc = new SelectablePrintDocument(rlDoc, rlDoc.CopiesToPrint) { Selected = isSelected };
				printDocuments.Add(doc);
			}

			Configure();
		}

		int DefaultCopies(OrderDocumentType orderDocType)
		{
			switch(orderDocType) {
				case OrderDocumentType.Bill:
				case OrderDocumentType.DriverTicket:
				case OrderDocumentType.M2Proxy:
					return 1;
				case OrderDocumentType.UPD:
				case OrderDocumentType.Torg12:
				case OrderDocumentType.ShetFactura:
					return currentOrder.DocumentType == Domain.Client.DefaultDocumentType.torg12 ? 1 : 2;
				default:
					return 2;
			}
		}

		void Configure()
		{
			multipleDocumentPrinter.PrintableDocuments = new GenericObservableList<SelectablePrintDocument>(printDocuments);
			multipleDocumentPrinter.DocumentsPrinted += MultipleDocumentPrinter_DocumentsPrinted;
			ytreeviewDocuments.ColumnsConfig = ColumnsConfigFactory.Create<SelectablePrintDocument>()
				.AddColumn("✓").AddToggleRenderer(x => x.Selected)
				.AddColumn("Документ").AddTextRenderer(x => x.Document.Name)
				.AddColumn("Копий").AddNumericRenderer(x => x.Copies).Editing()
					   .Adjustment(new Adjustment(0, 0, 10000, 1, 100, 0))
				.RowCells()
				.Finish();

			ytreeviewDocuments.ItemsDataSource = multipleDocumentPrinter.PrintableDocuments;

			DefaultPreviewDocument();
		}

		void MultipleDocumentPrinter_DocumentsPrinted(object o, EventArgs args)
		{
			DocumentsPrinted?.Invoke(o, args);
		}

		protected void DefaultPreviewDocument()
		{
			if(currentOrder != null) { //если этот диалог вызван из заказа
				var documents = printDocuments.Where(x => x.Document is OrderDocument)
											  .Where(x => (x.Document as OrderDocument).Order.Id == currentOrder.Id);

				var driverTicket = documents.FirstOrDefault(x => x.Document is DriverTicketDocument);
				var invoiceDocument = documents.FirstOrDefault(x => x.Document is InvoiceDocument);
				if(driverTicket != null && currentOrder.PaymentType == Domain.Client.PaymentType.cashless) {
					selectedDocument = driverTicket;
					PreviewDocument();
				}
				else if(invoiceDocument != null) {
					selectedDocument = invoiceDocument;
					PreviewDocument();
				}
			} else if(currentRouteList != null){ //если этот диалог вызван из МЛ
				selectedDocument = printDocuments.FirstOrDefault(x => x.Selected) ?? printDocuments.FirstOrDefault();
				PreviewDocument();
			}
		}

		void PreviewDocument()
		{
			var rdldoc = selectedDocument.Document as IPrintableRDLDocument;
			if(rdldoc == null)
				return;

			reportviewer.ReportPrinted -= Reportviewer_ReportPrinted;
			reportviewer.ReportPrinted += Reportviewer_ReportPrinted;
			var reportInfo = rdldoc.GetReportInfo();
			if(reportInfo.Source != null)
				reportviewer.LoadReport(reportInfo.Source, reportInfo.GetParametersString(), reportInfo.ConnectionString, true);
			else
				reportviewer.LoadReport(reportInfo.GetReportUri(), reportInfo.GetParametersString(), reportInfo.ConnectionString, true);
		}

		void Reportviewer_ReportPrinted(object sender, EventArgs e)
		{
			DocumentsPrinted?.Invoke(this, new EndPrintArgs { Args = new[] { selectedDocument.Document }});
		}

		protected void OnButtonPrintAllClicked(object sender, EventArgs e)
		{
			multipleDocumentPrinter.PrintSelectedDocuments();
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if(selectedDocument == null) {
				return;
			}
			multipleDocumentPrinter.PrintDocument(selectedDocument);
		}

		protected void OnYtreeviewDocumentsRowActivated(object o, RowActivatedArgs args)
		{
			selectedDocument = ytreeviewDocuments.GetSelectedObject() as SelectablePrintDocument;
			PreviewDocument();
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}
	}
}