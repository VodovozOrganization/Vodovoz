using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QSDocTemplates;
using QSProjectsLib;
using QSReport;
using QSTDI;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;

namespace Vodovoz.Dialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class OrderDocumentsPrinterDlg : TdiTabBase
	{
		Order currentOrder;
		MultipleDocumentPrinter multipleDocumentPrinter = new MultipleDocumentPrinter();
		List<SelectablePrintDocument> printDocuments = new List<SelectablePrintDocument>();

		public OrderDocumentsPrinterDlg(Order order)
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

		protected void DefaultPreviewDocument()
		{
			var documents = printDocuments.Where(x => x.Document is OrderDocument)
										  .Where(x => (x.Document as OrderDocument).Order.Id == currentOrder.Id);

			var driverTicket = documents.Where(x => x.Document is DriverTicketDocument).FirstOrDefault();
			var invoiceDocument = documents.Where(x => x.Document is InvoiceDocument).FirstOrDefault();
			if(driverTicket != null && currentOrder.PaymentType == Domain.Client.PaymentType.cashless) {
				PreviewDocument(driverTicket);
			} else if(invoiceDocument != null) {
				PreviewDocument(invoiceDocument);
			}
		}

		void PreviewDocument(SelectablePrintDocument selectedDocument)
		{
			var rdldoc = selectedDocument.Document as IPrintableRDLDocument;
			if(rdldoc == null)
				return;

			var reportInfo = rdldoc.GetReportInfo();
			reportviewer.LoadReport(reportInfo.GetReportUri(), reportInfo.GetParametersString(), reportInfo.ConnectionString, true);
		}

		protected void OnButtonPrintAllClicked(object sender, EventArgs e)
		{
			multipleDocumentPrinter.PrintSelectedDocuments();
		}

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			var selectedDocument = ytreeviewDocuments.GetSelectedObject() as SelectablePrintDocument;
			if(selectedDocument == null) {
				return;
			}
			multipleDocumentPrinter.PrintDocument(selectedDocument);
		}

		protected void OnYtreeviewDocumentsRowActivated(object o, RowActivatedArgs args)
		{
			PreviewDocument(ytreeviewDocuments.GetSelectedObject() as SelectablePrintDocument);
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e)
		{
			OnCloseTab(false);
		}
	}
}
