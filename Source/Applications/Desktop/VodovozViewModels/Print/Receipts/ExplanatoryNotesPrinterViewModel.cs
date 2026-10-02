using QS.Dialog;
using QS.Extensions.Observable.Collections.List;
using QS.Navigation;
using QS.Print;
using QS.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Infrastructure.Print;

namespace Vodovoz.ViewModels.Print.Receipts
{
	public class ExplanatoryNotesPrinterViewModel : TabViewModelBase
	{
		private readonly IDocumentPrinter _documentPrinter;
		private SelectablePrintDocument _selectedDocument;
		private ObservableList<SelectablePrintDocument> _printableDocuments = new ObservableList<SelectablePrintDocument>();

		public event Action PreviewDocument;

		public ExplanatoryNotesPrinterViewModel(
			IInteractiveService interactiveService,
			INavigationManager navigation,
			IDocumentPrinter documentPrinter)
			: base(interactiveService, navigation)
		{
			_documentPrinter = documentPrinter ?? throw new ArgumentNullException(nameof(documentPrinter));
			TabName = "Печать пояснительных записок";
		}

		public ObservableList<SelectablePrintDocument> PrintableDocuments
		{
			get => _printableDocuments;
			private set => SetField(ref _printableDocuments, value);
		}

		public SelectablePrintDocument SelectedDocument
		{
			get => _selectedDocument;
			set
			{
				if(SetField(ref _selectedDocument, value))
				{
					PreviewDocument?.Invoke();
				}
			}
		}

		public void Configure(IEnumerable<ExplanatoryNotePrintableDocument> documents)
		{
			if(documents == null)
			{
				throw new ArgumentNullException(nameof(documents));
			}

			var list = documents
				.Select(document => new SelectablePrintDocument(document) { Selected = true })
				.ToList();

			PrintableDocuments = new ObservableList<SelectablePrintDocument>(list);
			SelectedDocument = PrintableDocuments.FirstOrDefault();
			PreviewDocument?.Invoke();
		}

		public void PrintSelected()
		{
			var selected = PrintableDocuments
				.Where(x => x.Selected)
				.ToList();

			if(selected.Count == 0)
			{
				ShowWarningMessage("Выберите хотя бы один документ для печати.");
				return;
			}

			foreach(var item in selected)
			{
				item.Document.CopiesToPrint = item.Copies;
			}

			var documentsToPrint = new List<IPrintableDocument>();
			foreach(var item in selected)
			{
				for(var i = 0; i < item.Copies; i++)
				{
					documentsToPrint.Add(item.Document);
				}
			}

			_documentPrinter.PrintAllDocuments(documentsToPrint);
		}
	}
}
