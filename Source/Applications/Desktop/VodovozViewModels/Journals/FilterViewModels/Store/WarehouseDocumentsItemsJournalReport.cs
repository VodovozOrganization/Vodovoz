using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Vodovoz.ViewModels.Journals.JournalNodes.Store;
using Gamma.Utilities;
using Vodovoz.Domain.Documents.MovementDocuments;
using Vodovoz.Domain.Goods;
using Vodovoz.Presentation.ViewModels.Common;
using Vodovoz.Core.Domain.Warehouses.Documents;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Store
{
	public class WarehouseDocumentsItemsJournalReport
	{
		private readonly List<WarehouseDocumentsItemsJournalNode> _rows = new List<WarehouseDocumentsItemsJournalNode>();

		private WarehouseDocumentsItemsJournalReport(DateTime? startDate,
			DateTime? endDate,
			DocumentType? documentType,
			MovementDocumentStatus? movementDocumentStatus,
			string author,
			string lastEditor,
			string driver,
			string nomenclature,
			bool showNotAffectedBalance,
			TargetSource? targetSource,
			string counterparties,
			string warhouses,
			IEnumerable<WarehouseDocumentsItemsJournalNode> rows,
			IncludeExludeFiltersViewModel includeExcludeFilterViewModel)
		{
			StartDate = startDate;
			EndDate = endDate;
			DocumentType = documentType;
			MovementDocumentStatus = movementDocumentStatus;
			Author = author;
			LastEditor = lastEditor;
			Driver = driver;
			Nomenclature = nomenclature;
			ShowNotAffectedBalance = showNotAffectedBalance;
			TargetSource = targetSource;
			Counterparties = counterparties;
			Warhouses = warhouses;

			var nomenclatureElementsIncluded = includeExcludeFilterViewModel.GetIncludedElements<Nomenclature>().ToArray();
			var nomenclatureElementsExcluded = includeExcludeFilterViewModel.GetExcludedElements<Nomenclature>().ToArray();
			var productGroupElementsIncluded = includeExcludeFilterViewModel.GetIncludedElements<ProductGroup>().ToArray();
			var productGroupElementsExcluded = includeExcludeFilterViewModel.GetExcludedElements<ProductGroup>().ToArray();

			NomenclaturesIncluded = nomenclatureElementsIncluded.Length > 5 ? $"{nomenclatureElementsIncluded.Length} шт." : string.Join(", ", nomenclatureElementsIncluded.Select(x => x.Title));
			NomenclaturesExcluded = nomenclatureElementsExcluded.Length > 5 ? $"{nomenclatureElementsExcluded.Length} шт." : string.Join(", ", nomenclatureElementsExcluded.Select(x => x.Title));
			ProductGroupsIncluded = productGroupElementsIncluded.Length > 5 ? $"{productGroupElementsIncluded.Length} шт." : string.Join(", ", productGroupElementsIncluded.Select(x => x.Title));
			ProductGroupsExcluded = productGroupElementsExcluded.Length > 5 ? $"{productGroupElementsExcluded.Length} шт." : string.Join(", ", productGroupElementsExcluded.Select(x => x.Title));

			_rows.AddRange(rows);
		}

		public DateTime? StartDate { get; }
		public DateTime? EndDate { get; }
		public DocumentType? DocumentType { get; }
		public string DocumentTypeString => DocumentType?.GetEnumTitle() ?? string.Empty;
		public MovementDocumentStatus? MovementDocumentStatus { get; }
		public string MovementDocumentStatusString => MovementDocumentStatus?.GetEnumTitle() ?? string.Empty;
		public string Author { get; }
		public string LastEditor { get; }
		public string Driver { get; }
		public string Nomenclature { get; }
		public bool ShowNotAffectedBalance { get; }
		public string ShowNotAffectedBalanceString => ShowNotAffectedBalance ? "Да" : "Нет";
		public TargetSource? TargetSource { get; }
		public string TargetSourceString => TargetSource?.GetEnumTitle() ?? string.Empty;
		public string Counterparties { get; }
		public string Warhouses { get; }
		public string NomenclaturesIncluded { get; }
		public string NomenclaturesExcluded { get; }
		public string ProductGroupsIncluded { get; }
		public string ProductGroupsExcluded { get; }
		

		public ReadOnlyCollection<WarehouseDocumentsItemsJournalNode> Rows => _rows.AsReadOnly();

		public static WarehouseDocumentsItemsJournalReport Create(DateTime? startDate,
			DateTime? endDate,
			DocumentType? documentType,
			MovementDocumentStatus? movementDocumentStatus,
			string author,
			string lastEditor,
			string driver,
			string nomenclature,
			bool showNotAffectedBalance,
			TargetSource? targetSource,
			string counterparties,
			string warhouses,
			IEnumerable<WarehouseDocumentsItemsJournalNode> rows,
			IncludeExludeFiltersViewModel includeExcludeFilterViewModel)
		{
			return new WarehouseDocumentsItemsJournalReport(
				startDate,
				endDate,
				documentType,
				movementDocumentStatus,
				author,
				lastEditor,
				driver,
				nomenclature,
				showNotAffectedBalance,
				targetSource,
				counterparties,
				warhouses,
				rows,
				includeExcludeFilterViewModel);
		}
	}
}
