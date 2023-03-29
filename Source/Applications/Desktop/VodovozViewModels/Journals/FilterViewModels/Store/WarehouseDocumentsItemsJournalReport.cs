﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Vodovoz.Domain.Documents;
using Vodovoz.ViewModels.Journals.JournalNodes.Store;
using Gamma.Utilities;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Store
{
	public class WarehouseDocumentsItemsJournalReport
	{
		private readonly List<WarehouseDocumentsItemsJournalNode> _rows = new List<WarehouseDocumentsItemsJournalNode>();

		private WarehouseDocumentsItemsJournalReport(
			DateTime? startDate,
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
			IEnumerable<WarehouseDocumentsItemsJournalNode> rows)
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

		public ReadOnlyCollection<WarehouseDocumentsItemsJournalNode> Rows => _rows.AsReadOnly();

		public static WarehouseDocumentsItemsJournalReport Create(
			DateTime? startDate,
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
			IEnumerable<WarehouseDocumentsItemsJournalNode> rows)
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
				rows);
		}
	}
}
