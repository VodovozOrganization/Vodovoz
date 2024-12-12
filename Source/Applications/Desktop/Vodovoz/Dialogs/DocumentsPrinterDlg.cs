using System;
using System.Linq;
using Gamma.GtkWidgets;
using Gtk;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Print;
using QS.Report;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.ViewModels.Infrastructure.Print;

namespace Vodovoz.Dialogs
{
	[Obsolete("Удалить, если созданная viewModel работает корректно")]
	public partial class DocumentsPrinterDlg : QS.Dialog.Gtk.TdiTabBase
	{
		private readonly Order _currentOrder;
		private readonly RouteList _currentRouteList;
		private readonly IEntityDocumentsPrinter _entityDocumentsPrinter;
		private SelectablePrintDocument _selectedDocument;

		public event EventHandler DocumentsPrinted;

		public DocumentsPrinterDlg(Order order, IEntityDocumentsPrinterFactory entityDocumentsPrinterFactory)
		{
			Build();

			TabName = "Печать документов заказа";

			_entityDocumentsPrinter =
				(entityDocumentsPrinterFactory ?? throw new ArgumentNullException(nameof(entityDocumentsPrinterFactory)))
				.CreateOrderDocumentsPrinter(order);
			
			if(!string.IsNullOrEmpty(_entityDocumentsPrinter.ODTTemplateNotFoundMessages))
			{
				MessageDialogHelper.RunWarningDialog(_entityDocumentsPrinter.ODTTemplateNotFoundMessages);
			}

			_currentOrder = order;

			Configure();
		}

		public DocumentsPrinterDlg(
			IUnitOfWork uow,
			RouteList routeList,
			IEntityDocumentsPrinterFactory entityDocumentsPrinterFactory,
			RouteListPrintableDocuments selectedType)
		{
			Build();
			TabName = "Печать документов МЛ";
			_entityDocumentsPrinter =
				(entityDocumentsPrinterFactory ?? throw new ArgumentNullException(nameof(entityDocumentsPrinterFactory)))
				.CreateRouteListWithOrderDocumentsPrinter(uow, routeList, new[] { selectedType });
			
			_currentRouteList = routeList;

			Configure();
		}

		void Configure()
		{
			ytreeviewDocuments.ColumnsConfig = ColumnsConfigFactory.Create<SelectablePrintDocument>()
				.AddColumn("✓").AddToggleRenderer(x => x.Selected)
				.AddColumn("Документ").AddTextRenderer(x => x.Document.Name)
				.AddColumn("Копий").AddNumericRenderer(x => x.Copies).Editing()
					   .Adjustment(new Adjustment(0, 0, 10000, 1, 100, 0))
				.RowCells()
				.Finish();

			ytreeviewDocuments.ItemsDataSource = _entityDocumentsPrinter.MultiDocPrinterPrintableDocuments;

			DefaultPreviewDocument();
			_entityDocumentsPrinter.DocumentsPrinted += (o, args) => DocumentsPrinted?.Invoke(o, args);
		}

		protected void DefaultPreviewDocument()
		{
			var printDocuments = _entityDocumentsPrinter.MultiDocPrinterPrintableDocuments;
			
			if(_currentOrder != null)
			{ //если этот диалог вызван из заказа
				var documents =
					printDocuments.Where(x => x.Document is OrderDocument doc
																&& doc.Order.Id == _currentOrder.Id);

				var driverTicket = documents.FirstOrDefault(x => x.Document is DriverTicketDocument);
				var invoiceDocument = documents.FirstOrDefault(x => x.Document is InvoiceDocument);
				
				if(driverTicket != null && _currentOrder.PaymentType == Domain.Client.PaymentType.Cashless)
				{
					_selectedDocument = driverTicket;
					PreviewDocument();
				}
				else if(invoiceDocument != null)
				{
					_selectedDocument = invoiceDocument;
					PreviewDocument();
				}
			}
			else if(_currentRouteList != null)
			{ //если этот диалог вызван из МЛ
				_selectedDocument = printDocuments.FirstOrDefault(x => x.Selected) ?? printDocuments.FirstOrDefault();
				PreviewDocument();
			}
		}

		void PreviewDocument()
		{
			if(_selectedDocument.Document is IPrintableRDLDocument rdldoc)
			{
				reportviewer.ReportPrinted -= Reportviewer_ReportPrinted;
				reportviewer.ReportPrinted += Reportviewer_ReportPrinted;
				var reportInfo = rdldoc.GetReportInfo();

				if(reportInfo.Source != null)
				{
					reportviewer.LoadReport(reportInfo.Source, reportInfo.GetParametersString(), reportInfo.ConnectionString, true, reportInfo.RestrictedOutputPresentationTypes);
				}
				else
				{
					reportviewer.LoadReport(reportInfo.GetReportUri(), reportInfo.GetParametersString(), reportInfo.ConnectionString, true, reportInfo.RestrictedOutputPresentationTypes);
				}
			}
		}

		void Reportviewer_ReportPrinted(object sender, EventArgs e) => DocumentsPrinted?.Invoke(this, new EndPrintArgs { Args = new[] { _selectedDocument.Document } });

		protected void OnButtonPrintAllClicked(object sender, EventArgs e) => _entityDocumentsPrinter.Print();

		protected void OnButtonPrintClicked(object sender, EventArgs e)
		{
			if(_selectedDocument != null)
			{
				_entityDocumentsPrinter.Print(_selectedDocument);
			}
		}

		protected void OnYtreeviewDocumentsRowActivated(object o, RowActivatedArgs args)
		{
			_selectedDocument = ytreeviewDocuments.GetSelectedObject<SelectablePrintDocument>();
			PreviewDocument();
		}

		protected void OnButtonCancelClicked(object sender, EventArgs e) => OnCloseTab(false);
	}
}
