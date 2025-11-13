using MoreLinq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Report;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Users.Settings;
using Vodovoz.Domain.Documents;
using Vodovoz.EntityRepositories;
using Vodovoz.Extensions;
using Vodovoz.PrintableDocuments;
using Vodovoz.ViewModels.Infrastructure;
using Vodovoz.ViewModels.Infrastructure.Print;
using Vodovoz.ViewModels.Print.Store;

namespace Vodovoz.ViewModels.Print
{
	public partial class PrintDocumentsSelectablePrinterViewModel : DialogTabViewModelBase
	{
		private readonly IEventsQrPlacer _eventsQrPlacer;
		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly UserSettings _userSettings;

		private CarLoadDocument _carLoadDocument;
		private IList<ICustomPrintRdlDocument> _documentsToPrint = new List<ICustomPrintRdlDocument>();
		private GenericObservableList<PrintDocumentSelectableNode> _documentsNodes = new GenericObservableList<PrintDocumentSelectableNode>();
		private bool _printOperationCancelled;

		public PrintDocumentsSelectablePrinterViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ICustomPrintRdlDocumentsPrinter documentsPrinter,
			IUserRepository userRepository,
			IEventsQrPlacer eventsQrPlacer,
			IReportInfoFactory reportInfoFactory
			)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			if(userRepository is null)
			{
				throw new ArgumentNullException(nameof(userRepository));
			}

			Printer = documentsPrinter ?? throw new ArgumentNullException(nameof(documentsPrinter));
			_eventsQrPlacer = eventsQrPlacer ?? throw new ArgumentNullException(nameof(eventsQrPlacer));
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));

			TabName = "Печать документов";

			PrintSelectedDocumentsCommand = new DelegateCommand(PrintSelectedDocuments);
			CancelCommand = new DelegateCommand(Cancel);
			EditPrintrSettingsCommand = new DelegateCommand(EditPrintrSettings);
			SavePrinterSettingsCommand = new DelegateCommand(SavePrinterSettings);
			ReportPrintedCommand = new DelegateCommand(OnReportPrinted);

			_userSettings = userRepository.GetCurrentUserSettings(UoW);
		}

		public event Action PreviewDocument;
		public event EventHandler DocumentsPrinted;

		public DelegateCommand PrintSelectedDocumentsCommand;
		public DelegateCommand CancelCommand;
		public DelegateCommand EditPrintrSettingsCommand;
		public DelegateCommand SavePrinterSettingsCommand;
		public DelegateCommand ReportPrintedCommand;

		public ICustomPrintRdlDocumentsPrinter Printer { get; }
		public PrintDocumentSelectableNode ActiveNode { get; set; }

		public ICustomPrintRdlDocument ActiveDocument =>
			ActiveNode is null
			? null
			: DocumentsToPrint.Where(d => d.DocumentType == ActiveNode.DocumentType).FirstOrDefault();

		public IList<ICustomPrintRdlDocument> DocumentsToPrint
		{
			get => _documentsToPrint;
			set => SetField(ref _documentsToPrint, value);
		}

		public GenericObservableList<PrintDocumentSelectableNode> DocumentsNodes
		{
			get => _documentsNodes;
			set => SetField(ref _documentsNodes, value);
		}

		public void ConfigureForCarLoadDocumentsPrint(CarLoadDocument carLoadDocument)
		{
			TabName = "Печать талонов погрузки";

			_carLoadDocument = carLoadDocument;

			var waterCarLoadDocument = WaterCarLoadDocumentRdl.Create(_userSettings, carLoadDocument, CarLoadDocumentPlaseEventsQr, _reportInfoFactory);
			var controlCarLoadDocument = ControlCarLoadDocumentRdl.Create(_userSettings, carLoadDocument, _reportInfoFactory);
			var equipmentCarLoadDocument = EquipmentCarLoadDocumentRdl.Create(_userSettings, carLoadDocument, _reportInfoFactory);

			DocumentsToPrint.Add(waterCarLoadDocument);
			DocumentsToPrint.Add(controlCarLoadDocument);
			DocumentsToPrint.Add(equipmentCarLoadDocument);

			DocumentsNodes = new GenericObservableList<PrintDocumentSelectableNode>(
				DocumentsToPrint.Select(d => new PrintDocumentSelectableNode
				{
					IsSelected = true,
					DocumentType = d.DocumentType,
					PrinterName = d.PrinterName,
					NumberOfCopies = d.CopiesToPrint
				})
				.ToList());

			Printer.DocumentsPrinted += OnDocumentsPrinted;
			Printer.PrintingCanceled += OnPrintingCanceled;
			DefaultPreviewDocument();
		}

		private string CarLoadDocumentPlaseEventsQr(int documentId, string reportSource)
		{
			return _eventsQrPlacer.AddQrEventForWaterCarLoadDocument(
					UoW, documentId, reportSource);
		}

		public void DefaultPreviewDocument()
		{
			if(Printer is null)
			{
				return;
			}

			var printDocuments = DocumentsToPrint;

			if(_carLoadDocument != null)
			{
				ActiveNode = DocumentsNodes.FirstOrDefault(x => x.IsSelected) ?? DocumentsNodes.FirstOrDefault();
				PreviewDocument?.Invoke();
			}
		}

		private void PrintSelectedDocuments()
		{
			_printOperationCancelled = false;

			var selectedDocumentTypes = DocumentsNodes.Where(d => d.IsSelected).Select(d => d.DocumentType).ToList();
			var documentsToPrint = DocumentsToPrint.Where(d => selectedDocumentTypes.Contains(d.DocumentType)).ToList();

			foreach(var document in documentsToPrint)
			{
				if(_printOperationCancelled)
				{
					return;
				}

				Printer.Print(document);
			}
		}

		private void Cancel()
		{
			Close(false, CloseSource.Cancel);
		}

		private void EditPrintrSettings()
		{
			if(!(ActiveDocument is ICustomPrintRdlDocument doc))
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
			if(!(ActiveDocument is ICustomPrintRdlDocument doc))
			{
				return;
			}

			doc.PrinterName = e.PrinterName;
			doc.CopiesToPrint = e.NumberOfCopies;

			ActiveNode.PrinterName = e.PrinterName;
			ActiveNode.NumberOfCopies = e.NumberOfCopies;

			OnPropertyChanged(nameof(DocumentsNodes));
		}

		private void SavePrinterSettings()
		{
			if(!(ActiveDocument is ICustomPrintRdlDocument doc))
			{
				return;
			}

			if(string.IsNullOrWhiteSpace(doc.PrinterName) || doc.CopiesToPrint < 1)
			{
				ShowErrorMessage($"Сохранение недоступно!\nНастройки принтера для документа \"{doc.DocumentType.GetEnumDisplayName()}\" не заданы!");
				return;
			}

			var existingPrinterSettinsForDocument = _userSettings.DocumentPrinterSettings
					.Where(s => s.DocumentType == doc.DocumentType)
					.FirstOrDefault();

			if(existingPrinterSettinsForDocument is null)
			{
				var newDocumentPrinterSetting = new DocumentPrinterSetting
				{
					UserSettings = _userSettings,
					DocumentType = doc.DocumentType,
					PrinterName = doc.PrinterName,
					NumberOfCopies = doc.CopiesToPrint
				};

				_userSettings.DocumentPrinterSettings.Add(newDocumentPrinterSetting);
			}
			else
			{
				existingPrinterSettinsForDocument.PrinterName = doc.PrinterName;
				existingPrinterSettinsForDocument.NumberOfCopies = doc.CopiesToPrint;
			}

			UoW.Save(_userSettings);
			UoW.Commit();

			ShowInfoMessage($"Настройки принтера для документа \"{doc.DocumentType.GetEnumDisplayName()}\" сохранены!");
		}

		private void OnReportPrinted()
		{
			DocumentsPrinted?.Invoke(this, new PrintEventArgs(ActiveDocument));
		}

		private void OnDocumentsPrinted(object sender, EventArgs e)
		{
			DocumentsPrinted?.Invoke(sender, e);
		}

		private void OnPrintingCanceled(object sender, EventArgs e)
		{
			_printOperationCancelled = true;
		}
	}
}
