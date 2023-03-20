using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vodovoz.Domain.Documents;
using Vodovoz.ViewModels.Journals.JournalNodes.Store;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Store
{
	public class WarehouseDocumentsItemsJournalReport
	{
		private readonly List<WarehouseDocumentsItemsJournalNode> _nodes = new List<WarehouseDocumentsItemsJournalNode>();

		private WarehouseDocumentsItemsJournalReport(
			DateTime? stertDate,
			DateTime? endDate,
			DocumentType? documentType,
			MovementDocumentStatus? movementDocumentStatus,
			string author,
			string lastEditor,
			string driver,
			string nomenclature,
			bool showNotAffectedBalance,
			TargetSource? targetSource,
			IEnumerable<string> counterparties,
			IEnumerable<string> warhouses,
			IEnumerable<WarehouseDocumentsItemsJournalNode> nodes)
		{
			StertDate = stertDate;
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
			_nodes.AddRange(nodes);
		}

		public DateTime? StertDate { get; }
		public DateTime? EndDate { get; }
		public DocumentType? DocumentType { get; }
		public MovementDocumentStatus? MovementDocumentStatus { get; }
		public string Author { get; }
		public string LastEditor { get; }
		public string Driver { get; }
		public string Nomenclature { get; }
		public bool ShowNotAffectedBalance { get; }
		public TargetSource? TargetSource { get; }
		public IEnumerable<string> Counterparties { get; }
		public IEnumerable<string> Warhouses { get; }

		public ReadOnlyCollection<WarehouseDocumentsItemsJournalNode> Lines => _nodes.AsReadOnly();

		public static WarehouseDocumentsItemsJournalReport Create(
			DateTime? stertDate,
			DateTime? endDate,
			DocumentType? documentType,
			MovementDocumentStatus? movementDocumentStatus,
			string author,
			string lastEditor,
			string driver,
			string nomenclature,
			bool showNotAffectedBalance,
			TargetSource? targetSource,
			IEnumerable<string> counterparties,
			IEnumerable<string> warhouses,
			IEnumerable<WarehouseDocumentsItemsJournalNode> nodes)
		{
			return new WarehouseDocumentsItemsJournalReport(
				stertDate,
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
				nodes);
		}
	}
}
