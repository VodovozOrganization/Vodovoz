using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Print;
using QS.ViewModels;
using System;
using System.Linq;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Extensions;
using Vodovoz.PrintableDocuments;
using Vodovoz.Services;
using Vodovoz.ViewModels.Infrastructure;
using Vodovoz.ViewModels.Infrastructure.Print;

namespace Vodovoz.ViewModels.Print
{
	public class PrintDocumentsSelectablePrinterViewModel : DialogTabViewModelBase
	{
		private readonly IEntityDocumentsPrinterFactory _entityDocumentsPrinterFactory;
		private readonly IUserSettingsService _userSettingsService;
		private CarLoadDocument _carLoadDocument;
		private IEntityDocumentsPrinter _entityDocumentsPrinter;

		public PrintDocumentsSelectablePrinterViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IEntityDocumentsPrinterFactory entityDocumentsPrinterFactory,
			IUserSettingsService userSettingsService)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_entityDocumentsPrinterFactory = entityDocumentsPrinterFactory ?? throw new ArgumentNullException(nameof(entityDocumentsPrinterFactory));
			_userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));

			TabName = "Печать документов";

			PrintSelectedDocumentsCommand = new DelegateCommand(PrintSelectedDocuments);
			CancelCommand = new DelegateCommand(Cancel);
			EditPrintrSettingsCommand = new DelegateCommand(EditPrintrSettings);
			SavePrinterSettingsCommand = new DelegateCommand(SavePrinterSettings);
			ReportPrintedCommand = new DelegateCommand(OnReportPrinted);
		}

		public event Action PreviewDocument;
		public event EventHandler DocumentsPrinted;

		public DelegateCommand PrintSelectedDocumentsCommand;
		public DelegateCommand CancelCommand;
		public DelegateCommand EditPrintrSettingsCommand;
		public DelegateCommand SavePrinterSettingsCommand;
		public DelegateCommand ReportPrintedCommand;

		public IEntityDocumentsPrinter EntityDocumentsPrinter
		{
			get => _entityDocumentsPrinter;
			private set => SetField(ref _entityDocumentsPrinter, value);
		}

		public SelectablePrintDocument SelectedDocument { get; set; }

		public void ConfigureForCarLoadDocumentsPrint(
			IEventsQrPlacer eventsQrPlacer,
			CarLoadDocument carLoadDocument)
		{
			EntityDocumentsPrinter = _entityDocumentsPrinterFactory
				.CreateCarLoadDocumentsPrinter(UnitOfWorkFactory, eventsQrPlacer, _userSettingsService, carLoadDocument);

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

		private void PrintSelectedDocuments()
		{
			EntityDocumentsPrinter.Print();
		}

		private void Cancel()
		{
			Close(false, CloseSource.Cancel);
		}

		private void EditPrintrSettings()
		{
			if(!(SelectedDocument is ICustomPrinterPrintDocument doc))
			{
				return;
			}

			var printerSelectionViewModel = NavigationManager.OpenViewModel<PrinterSelectionViewModel>(null).ViewModel;

			printerSelectionViewModel.ConfigureDialog(
				doc.PrinterName,
				doc.CopiesToPrint,
				$"Тип документа: {doc.DocumentType.GetEnumDisplayName()}");

			printerSelectionViewModel.PrinterSelected += OnPrinterSelected;
		}

		private void OnPrinterSelected(object sender, PrinterSelectedEventArgs e)
		{
			if(!(SelectedDocument is ICustomPrinterPrintDocument doc))
			{
				return;
			}

			doc.PrinterName = e.PrinterName;
			doc.CopiesToPrint = e.NumberOfCopies;
		}

		private void SavePrinterSettings()
		{
			if(!(SelectedDocument is ICustomPrinterPrintDocument doc))
			{
				return;
			}

			if(string.IsNullOrWhiteSpace(doc.PrinterName) || doc.CopiesToPrint < 1)
			{
				return;
			}

			var userSettings = _userSettingsService.Settings;

			var existingPrinterSettinsForDocument = userSettings.DocumentPrinterSettings
				.Where(s => s.DocumentType == doc.DocumentType)
				.FirstOrDefault();

			if(existingPrinterSettinsForDocument is null)
			{
				var newDocumentPrinterSetting = new DocumentPrinterSetting
				{
					DocumentType = doc.DocumentType,
					PrinterName = doc.PrinterName,
					NumberOfCopies = doc.CopiesToPrint
				};

				_userSettingsService.Settings.DocumentPrinterSettings.Add(newDocumentPrinterSetting);
			}
			else
			{
				existingPrinterSettinsForDocument.PrinterName = doc.PrinterName;
				existingPrinterSettinsForDocument.NumberOfCopies = doc.CopiesToPrint;
			}

			using(var uow = UnitOfWorkFactory.CreateForRoot<UserSettings>(_userSettingsService.Settings.Id))
			{
				uow.Save(userSettings);
			}

			ShowInfoMessage("Настройка принтера сохранена!");
		}

		private void OnReportPrinted()
		{
			DocumentsPrinted?.Invoke(this, new PrintEventArgs(SelectedDocument?.Document));
		}

		private void OnDocumentsPrinted(object sender, EventArgs e)
		{
			DocumentsPrinted?.Invoke(sender, e);
		}
	}

	public class PrintEventArgs : EventArgs
	{
		public PrintEventArgs(IPrintableDocument document)
		{
			Document = document;
		}

		public IPrintableDocument Document { get; }
	}
}
