using DateTimeHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Vodovoz.ViewModels.Journals.JournalNodes.Store;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Store
{
	public class WarehouseAccountingCard
	{
		private readonly List<WarehouseAccountingCardRow> _rows = new List<WarehouseAccountingCardRow>();
		private readonly Func<int, int, DateTime, decimal> _getWarehouseBalance;

		private WarehouseAccountingCard(
			DateTime startDate,
			DateTime endDate,
			int warehouseId,
			string warehouseName,
			int nomenclatureId,
			string nomenclatureName,
			IEnumerable<WarehouseDocumentsItemsJournalNode> rows,
			Func<int, int, DateTime, decimal> getWarehouseBalance)
		{
			StartDate = startDate;
			EndDate = endDate;
			WarehouseId = warehouseId;
			WarehouseName = warehouseName;
			NomenclatureId = nomenclatureId;
			NomenclatureName = nomenclatureName;
			_getWarehouseBalance = getWarehouseBalance;
			ProcessData(rows);
		}

		private void ProcessData(IEnumerable<WarehouseDocumentsItemsJournalNode> rows)
		{
			var slices = DateTimeSliceFactory.CreateDaysSlices(StartDate, EndDate);

			foreach (var slice in slices)
			{
				var startDate = slice.StartDate;
				var endDate = slice.EndDate;
				var income = rows
					.Where(x => x.Date >= startDate
						&& x.Date <= endDate
						&& !string.IsNullOrWhiteSpace(x.Target)
						&& x.ToStorageId == WarehouseId
						&& x.NomenclatureName == NomenclatureName)
					.Sum(x => Math.Abs(x.Amount));

				var outcome = rows
					.Where(x => x.Date >= startDate
						&& x.Date <= endDate
						&& !string.IsNullOrWhiteSpace(x.Source)
						&& x.FromStorageId == WarehouseId
						&& x.NomenclatureName == NomenclatureName)
					.Sum(x => Math.Abs(x.Amount));

				var residue = _getWarehouseBalance(NomenclatureId, WarehouseId, slice.EndDate);

				_rows.Add(WarehouseAccountingCardRow.Create(startDate, income, outcome, residue));
			}
		}

		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public int WarehouseId { get; }
		public string WarehouseName { get; }
		public int NomenclatureId { get; }
		public string NomenclatureName { get; }
		public ReadOnlyCollection<WarehouseAccountingCardRow> Rows => _rows.AsReadOnly();

		public static WarehouseAccountingCard Create(
			DateTime startDate,
			DateTime endDate,
			int warehouseId,
			string warehouseName,
			int nomenclatureId,
			string nomenclatureName,
			IEnumerable<WarehouseDocumentsItemsJournalNode> rows,
			Func<int, int, DateTime, decimal> getWarhouseBalance)
		{
			return new WarehouseAccountingCard(
				startDate,
				endDate,
				warehouseId,
				warehouseName,
				nomenclatureId,
				nomenclatureName,
				rows,
				getWarhouseBalance);
		}
	}

	public class WarehouseAccountingCardRow
	{
		private WarehouseAccountingCardRow(DateTime date, decimal income, decimal outcome, decimal residue)
		{
			Date = date;
			Income = income;
			Outcome = outcome;
			Residue = residue;
		}

		public DateTime Date { get; }

		public decimal Income { get; }

		public decimal Outcome { get; }

		public decimal Residue { get; }

		public static WarehouseAccountingCardRow Create(DateTime date, decimal income, decimal outcome, decimal residue)
		{
			return new WarehouseAccountingCardRow(date, income, outcome, residue);
		}
	}
}
