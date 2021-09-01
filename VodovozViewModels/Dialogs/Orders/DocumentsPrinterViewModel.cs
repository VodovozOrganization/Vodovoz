using System;
using System.Linq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Print;
using QS.ViewModels;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Orders.Documents;
using Vodovoz.ViewModels.Infrastructure.Print;

namespace Vodovoz.ViewModels.Dialogs.Orders
{
    public class DocumentsPrinterViewModel : TabViewModelBase
    {
        private readonly Order _currentOrder;
		private readonly RouteList _currentRouteList;
		private SelectablePrintDocument _selectedDocument;
		private DelegateCommand _printSelectedCommand;
		
		public event Action PreviewDocument;
		public event EventHandler DocumentsPrinted;
		
		public DocumentsPrinterViewModel(
			IEntityDocumentsPrinterFactory entityDocumentsPrinterFactory,
			IInteractiveService interactiveService,
			INavigationManager navigationManager, 
			Order order) : base (interactiveService, navigationManager)
		{
			EntityDocumentsPrinter =
				(entityDocumentsPrinterFactory ?? throw new ArgumentNullException(nameof(entityDocumentsPrinterFactory)))
				.CreateOrderDocumentsPrinter(order);

			TabName = "Печать документов заказа";

			if(!string.IsNullOrEmpty(EntityDocumentsPrinter.ODTTemplateNotFoundMessages))
			{
				interactiveService.ShowMessage(ImportanceLevel.Warning, EntityDocumentsPrinter.ODTTemplateNotFoundMessages);
			}

			_currentOrder = order;
			Configure();
		}
		
		public DocumentsPrinterViewModel(
			IUnitOfWork uow,
			IEntityDocumentsPrinterFactory entityDocumentsPrinterFactory,
			INavigationManager navigationManager,
			RouteList routeList,
			RouteListPrintableDocuments selectedType,
			IInteractiveService interactiveService) : base (interactiveService, navigationManager)
		{
			EntityDocumentsPrinter =
				(entityDocumentsPrinterFactory ?? throw new ArgumentNullException(nameof(entityDocumentsPrinterFactory)))
				.CreateRouteListWithOrderDocumentsPrinter(uow, routeList, new[] { selectedType });

			TabName = "Печать документов МЛ";
			
			_currentRouteList = routeList;
			Configure();
		}

		public DelegateCommand PrintSelectedCommand => _printSelectedCommand ?? (_printSelectedCommand = new DelegateCommand(
			() => EntityDocumentsPrinter.Print(SelectedDocument),
			() => CanPrint
			)
		);
		
		public IEntityDocumentsPrinter EntityDocumentsPrinter { get; }

		public SelectablePrintDocument SelectedDocument
		{
			get => _selectedDocument;
			set
			{
				if(SetField(ref _selectedDocument, value))
				{
					OnPropertyChanged(nameof(CanPrint));
				}
			}
		}

		public bool CanPrint => SelectedDocument != null;

		private void Configure()
		{
			DefaultPreviewDocument();
			EntityDocumentsPrinter.DocumentsPrinted += (o, args) => DocumentsPrinted?.Invoke(o, args);
		}
		
		private void DefaultPreviewDocument()
		{
			var printDocuments = EntityDocumentsPrinter.DocumentsToPrint;
			if(_currentOrder != null) 
			{ //если этот диалог вызван из заказа
				var documents =
					printDocuments.Where(x => x.Document is OrderDocument doc && doc.Order.Id == _currentOrder.Id).ToList();

				var driverTicket = documents.FirstOrDefault(x => x.Document is DriverTicketDocument);
				var invoiceDocument = documents.FirstOrDefault(x => x.Document is InvoiceDocument);
				
				if(driverTicket != null && _currentOrder.PaymentType == Domain.Client.PaymentType.cashless) 
				{
					SelectedDocument = driverTicket;
					PreviewDocument?.Invoke();
				} 
				else if(invoiceDocument != null) 
				{
					SelectedDocument = invoiceDocument;
					PreviewDocument?.Invoke();
				}
			}
			else if(_currentRouteList != null) 
			{ //если этот диалог вызван из МЛ
				SelectedDocument = printDocuments.FirstOrDefault(x => x.Selected) ?? printDocuments.FirstOrDefault();
				PreviewDocument?.Invoke();
			}
		}

		public void PrintAll() => EntityDocumentsPrinter.Print();

		public void ReportViewerOnReportPrinted(object o, EventArgs args) => DocumentsPrinted?.Invoke(o, args);
    }
}