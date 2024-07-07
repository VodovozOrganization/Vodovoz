using QS.Navigation;
using System;
using Vodovoz.Extensions;
using Vodovoz.PrintableDocuments;
using Vodovoz.ViewModels.Infrastructure.Print;
using Vodovoz.ViewModels.Print;

namespace Vodovoz.Additions.Printing
{
	public class CustomPrintRdlDocumentsPrinter : ICustomPrintRdlDocumentsPrinter
	{
		private bool _cancelPrinting = false;
		ICustomPrintRdlDocument _documentToPrint;
		private readonly INavigationManager _navigationManager;

		public CustomPrintRdlDocumentsPrinter(INavigationManager navigationManager)
		{
			_navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
		}

		public event EventHandler DocumentsPrinted;
		public event EventHandler PrintingCanceled;

		public void Print(ICustomPrintRdlDocument document)
		{
			_documentToPrint = document ?? throw new ArgumentNullException(nameof(document));
			_cancelPrinting = false;

			if(string.IsNullOrWhiteSpace(_documentToPrint.PrinterName) || _documentToPrint.CopiesToPrint < 1)
			{
				OpenPrinterSelectionDialog();
			}

			if(!_cancelPrinting)
			{
				DocumentsPrinted?.Invoke(this, new EventArgs());
			}
			else
			{
				PrintingCanceled?.Invoke(this, new EventArgs());
			}

			_documentToPrint = null;
		}

		//private static void Print(string printer, UserPrintSettings userPrintSettings, Pages pages, bool isWindowsOs)
		//{
		//	void HandlePrintBeginPrint(object o, BeginPrintArgs args)
		//	{
		//		var printing = (PrintOperation)o;
		//		printing.NPages = pages.Count;
		//	}

		//	void HandlePrintDrawPage(object o, DrawPageArgs args)
		//	{
		//		using(var g = args.Context.CairoContext)
		//		using(var render = new RenderCairo(g))
		//		{
		//			render.RunPage(pages[args.PageNr]);
		//		}
		//	}

		//	PrintOperation printOperation = null;
		//	PrintOperationResult result;

		//	try
		//	{
		//		printOperation = new PrintOperation();
		//		printOperation.Unit = Unit.Points;
		//		printOperation.UseFullPage = true;
		//		printOperation.AllowAsync = true;
		//		printOperation.PrintSettings = new PrintSettings
		//		{
		//			Printer = printer,
		//			Orientation = (PageOrientation)Enum.Parse(typeof(PageOrientation), userPrintSettings.PageOrientation.ToString()),
		//			NCopies = (int)userPrintSettings.NumberOfCopies
		//		};

		//		printOperation.BeginPrint += HandlePrintBeginPrint;
		//		printOperation.DrawPage += HandlePrintDrawPage;

		//		result = printOperation.Run(PrintOperationAction.Print, null);
		//	}
		//	catch(Exception e) when(e.Message == "Error from StartDoc")
		//	{
		//		result = PrintOperationResult.Cancel;
		//		_logger.Debug("Операция печати отменена");
		//	}
		//	finally
		//	{
		//		if(printOperation != null)
		//		{
		//			printOperation.BeginPrint -= HandlePrintBeginPrint;
		//			printOperation.DrawPage -= HandlePrintDrawPage;
		//			printOperation.Dispose();
		//		}
		//	}

		//	if(isWindowsOs && new[] { PrintOperationResult.Apply, PrintOperationResult.InProgress }.Contains(result))
		//	{
		//		ShowPrinterQueue(printer);
		//	}
		//}

		private void OpenPrinterSelectionDialog()
		{
			if(_documentToPrint is null)
			{
				return;
			}

			var printerSelectionViewModel = _navigationManager.OpenViewModel<PrinterSelectionViewModel>(null).ViewModel;

			printerSelectionViewModel.ConfigureDialog(
				_documentToPrint.PrinterName,
				1,
				$"Печать документа: {_documentToPrint.DocumentType.GetEnumDisplayName()}");

			printerSelectionViewModel.PrinterSelected += OnPrinterSelected;
			printerSelectionViewModel.SelectionCanceled += OnPrinterSelectionCanceled; ;
		}

		private void OnPrinterSelected(object sender, PrinterSelectedEventArgs e)
		{
			if(_documentToPrint is null)
			{
				return;
			}

			_documentToPrint.PrinterName = e.PrinterName;
			_documentToPrint.CopiesToPrint = e.NumberOfCopies;
		}

		private void OnPrinterSelectionCanceled(object sender, EventArgs e)
		{
			_cancelPrinting = true;
		}
	}
}
