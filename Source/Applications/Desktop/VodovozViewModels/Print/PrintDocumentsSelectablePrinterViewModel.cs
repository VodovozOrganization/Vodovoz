using MoreLinq;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Print;
using QS.Report;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Logistics.Drivers;
using Vodovoz.Domain.Documents;
using Vodovoz.Domain.Employees;
using Vodovoz.Extensions;
using Vodovoz.PrintableDocuments;
using Vodovoz.PrintableDocuments.Store;
using Vodovoz.Services;
using Vodovoz.ViewModels.Infrastructure;
using Vodovoz.ViewModels.Infrastructure.Print;

namespace Vodovoz.ViewModels.Print
{
	public class PrintDocumentsSelectablePrinterViewModel : DialogTabViewModelBase
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IUserSettingsService _userSettingsService;

		private CarLoadDocument _carLoadDocument;
		private IList<ICustomPrintRdlDocument> _documentsToPrint = new List<ICustomPrintRdlDocument>();
		private GenericObservableList<PrintDocumentSelectableNode> _documentsNodes = new GenericObservableList<PrintDocumentSelectableNode>();
		private bool _printOperationCancelled;

		public PrintDocumentsSelectablePrinterViewModel(
			IUnitOfWorkFactory unitOfWorkFactory,
			IInteractiveService interactiveService,
			INavigationManager navigation,
			ICustomPrintRdlDocumentsPrinter documentsPrinter,
			IUserSettingsService userSettingsService)
			: base(unitOfWorkFactory, interactiveService, navigation)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			Printer = documentsPrinter ?? throw new ArgumentNullException(nameof(documentsPrinter));
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

		public ICustomPrintRdlDocumentsPrinter Printer { get; }

		public PrintDocumentSelectableNode ActiveNode { get; set; }
		public ICustomPrintRdlDocument ActiveDocument =>
			ActiveNode is null ? null : DocumentsToPrint.Where(d => d.DocumentType == ActiveNode.DocumentType).FirstOrDefault();

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

		public void ConfigureForCarLoadDocumentsPrint(
			IEventsQrPlacer eventsQrPlacer,
			CarLoadDocument carLoadDocument)
		{
			TabName = "Печать талонов погрузки";

			_carLoadDocument = carLoadDocument;

			ReportInfo reportInfo = eventsQrPlacer.AddQrEventForPrintingDocument(
					UoW, carLoadDocument.Id, carLoadDocument.Title, EventQrDocumentType.CarLoadDocument);

			var waterCarLoadDocument = WaterCarLoadDocumentRdl.Create(_userSettingsService, carLoadDocument, reportInfo);
			var equipmentCarLoadDocument = EquipmentCarLoadDocumentRdl.Create(_userSettingsService, carLoadDocument);

			DocumentsToPrint.Add(waterCarLoadDocument);
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
		}

		private void SavePrinterSettings()
		{
			if(!(ActiveDocument is ICustomPrintRdlDocument doc))
			{
				return;
			}

			if(string.IsNullOrWhiteSpace(doc.PrinterName) || doc.CopiesToPrint < 1)
			{
				return;
			}

			using(var uowGeneric = _unitOfWorkFactory.CreateForRoot<UserSettings>(_userSettingsService.Settings.Id, "Кнопка сохранения настройки принтера"))
			{
				var userSettins = uowGeneric.Root;

				var existingPrinterSettinsForDocument = userSettins.DocumentPrinterSettings
					.Where(s => s.DocumentType == doc.DocumentType)
					.FirstOrDefault();

				if(existingPrinterSettinsForDocument is null)
				{
					var newDocumentPrinterSetting = new DocumentPrinterSetting
					{
						UserSettings = userSettins,
						DocumentType = doc.DocumentType,
						PrinterName = doc.PrinterName,
						NumberOfCopies = doc.CopiesToPrint
					};

					userSettins.DocumentPrinterSettings.Add(newDocumentPrinterSetting);
				}
				else
				{
					existingPrinterSettinsForDocument.PrinterName = doc.PrinterName;
					existingPrinterSettinsForDocument.NumberOfCopies = doc.CopiesToPrint;
				}

				uowGeneric.Save();
				uowGeneric.Commit();
			}

			ShowInfoMessage("Настройка принтера сохранена!");
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

		public class PrintDocumentSelectableNode
		{
			public bool IsSelected { get; set; }
			public CustomPrintDocumentType DocumentType { get; set; }
			public string PrinterName { get; set; }
			public int NumberOfCopies { get; set; }
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
