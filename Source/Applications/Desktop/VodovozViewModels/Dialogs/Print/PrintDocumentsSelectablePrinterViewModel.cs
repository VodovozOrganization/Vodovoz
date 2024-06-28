using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Print;
using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.ViewModels.Infrastructure;
using Vodovoz.ViewModels.Infrastructure.Print;

namespace Vodovoz.ViewModels.Dialogs.Print
{
	public class PrintDocumentsSelectablePrinterViewModel : DialogTabViewModelBase
	{
		private readonly IEntityDocumentsPrinterFactory _entityDocumentsPrinterFactory;
		private CarLoadDocument _carLoadDocument;
		private IEntityDocumentsPrinter _entityDocumentsPrinter;

		public event Action PreviewDocument;
		public event EventHandler DocumentsPrinted;

		public PrintDocumentsSelectablePrinterViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IEntityDocumentsPrinterFactory entityDocumentsPrinterFactory)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_entityDocumentsPrinterFactory = entityDocumentsPrinterFactory ?? throw new ArgumentNullException(nameof(entityDocumentsPrinterFactory));

			TabName = "Печать документов";

			PrintSelectedCommand = new DelegateCommand(PrintSelected);
			CancelCommand = new DelegateCommand(Cancel);
		}

		public DelegateCommand PrintSelectedCommand;
		public DelegateCommand CancelCommand;

		public IEntityDocumentsPrinter EntityDocumentsPrinter
		{
			get => _entityDocumentsPrinter;
			private set => SetField(ref _entityDocumentsPrinter, value);
		}

		public SelectablePrintDocument SelectedDocument { get; set; }

		public void ConfigureForCarLoadDocumentsPrint(
			IUnitOfWork unitOfWork,
			IEventsQrPlacer eventsQrPlacer,
			CarLoadDocument carLoadDocument)
		{
			EntityDocumentsPrinter = _entityDocumentsPrinterFactory
				.CreateCarLoadDocumentsPrinter(unitOfWork, eventsQrPlacer, carLoadDocument);

			TabName = "Печать талонов погрузки";

			_carLoadDocument = carLoadDocument;

			EntityDocumentsPrinter.DocumentsPrinted += OnDocumentsPrinted;
			DefaultPreviewDocument();
		}

		public void DefaultPreviewDocument()
		{
			if(EntityDocumentsPrinter is null)
			{
				return;
			}

			var printDocuments = EntityDocumentsPrinter.DocumentsToPrint;

			if(_carLoadDocument != null)
			{
				SelectedDocument = printDocuments.FirstOrDefault(x => x.Selected) ?? printDocuments.FirstOrDefault();
				PreviewDocument?.Invoke();
			}
		}

		private void PrintSelected() => EntityDocumentsPrinter.Print();
		private void Cancel() => Close(false, CloseSource.Cancel);

		public void ReportViewerOnReportPrinted(object o, EventArgs args) => DocumentsPrinted?.Invoke(o, args);

		private void OnDocumentsPrinted(object sender, EventArgs e)
		{
			DocumentsPrinted?.Invoke(sender, e);
		}
	}
}
