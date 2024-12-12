using QS.Commands;
using QS.DomainModel.Entity;
using QS.Navigation;
using QS.ViewModels.Dialog;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;

namespace Vodovoz.ViewModels.Print
{
	public class PrinterSelectionViewModel : WindowDialogViewModelBase
	{
		private string _selectedPrinter;
		private int _numberOfCopies;
		private string _dialogHeader;

		public PrinterSelectionViewModel(INavigationManager navigation) : base(navigation)
		{
			Printers = PrinterSettings.InstalledPrinters.Cast<string>().ToList();
			NumberOfCopies = 1;

			SelectPrinterCommand = new DelegateCommand(SelectPrinter, () => CanSelectPrinter);
			CancelCommand = new DelegateCommand(Cancel);

			Title = "Выбор принтера";
		}

		public event EventHandler<PrinterSelectedEventArgs> PrinterSelected;
		public event EventHandler SelectionCanceled;

		public DelegateCommand SelectPrinterCommand;
		public DelegateCommand CancelCommand;

		public IList<string> Printers { get; }

		[PropertyChangedAlso(nameof(CanSelectPrinter))]
		public string SelectedPrinter
		{
			get => _selectedPrinter;
			set => SetField(ref _selectedPrinter, value);
		}

		[PropertyChangedAlso(nameof(CanSelectPrinter))]
		public int NumberOfCopies
		{
			get => _numberOfCopies;
			set => SetField(ref _numberOfCopies, value);
		}

		public string DialogHeader
		{
			get => _dialogHeader;
			set => SetField(ref _dialogHeader, value);
		}

		public bool CanSelectPrinter =>
			!string.IsNullOrEmpty(_selectedPrinter)
			&& NumberOfCopies > 0;

		public void ConfigureDialog(string selectedPrinterName, int numberOfCopies, string dialogHeader = null)
		{
			if(numberOfCopies < 1)
			{
				throw new ArgumentException($"Значение числа копий должно быть больше нуля", nameof(numberOfCopies));
			}

			if(!string.IsNullOrWhiteSpace(selectedPrinterName))
			{
				SelectedPrinter = Printers.Where(p => p == selectedPrinterName).FirstOrDefault();
			}

			DialogHeader = dialogHeader;

			NumberOfCopies = numberOfCopies;
		}

		private void SelectPrinter()
		{
			if(!CanSelectPrinter)
			{
				return;
			}

			PrinterSelected?.Invoke(this, new PrinterSelectedEventArgs(SelectedPrinter, NumberOfCopies));

			Close(false, CloseSource.Self);
		}

		private void Cancel()
		{
			SelectionCanceled?.Invoke(this, EventArgs.Empty);

			Close(false, CloseSource.Cancel);
		}
	}

	public class PrinterSelectedEventArgs : EventArgs
	{
		public PrinterSelectedEventArgs(string printerName, int numberOfCopies)
		{
			PrinterName = printerName;
			NumberOfCopies = numberOfCopies;
		}

		public string PrinterName { get; }
		public int NumberOfCopies { get; }
	}
}
