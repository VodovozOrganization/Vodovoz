using DateTimeHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Vodovoz.ViewModels.Journals.JournalNodes.Store;

namespace Vodovoz.ViewModels.Journals.FilterViewModels.Store
{
	public class WarhouseAccountingCard
	{
		private readonly List<WarhouseAccountingCardRow> _rows = new List<WarhouseAccountingCardRow>();

		private WarhouseAccountingCard(
			DateTime startDate,
			DateTime endDate,
			int warhouseId,
			string warehouseName,
			int nomenclatureId,
			string nomenclatureName,
			IEnumerable<WarehouseDocumentsItemsJournalNode> rows)
		{
			StartDate = startDate;
			EndDate = endDate;
			WarehouseId = warhouseId;
			WarehouseName = warehouseName;
			NomenclatureId = nomenclatureId;
			NomenclatureName = nomenclatureName;
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
						&& x.ToWarehouseId == WarehouseId
						&& x.NomenclatureName == NomenclatureName)
					.Sum(x => x.Amount);

				var outcome = rows
					.Where(x => x.Date >= startDate
						&& x.Date <= endDate
						&& x.FromWarehouseId == WarehouseId
						&& x.NomenclatureName == NomenclatureName)
					.Sum(x => x.Amount);

				_rows.Add(WarhouseAccountingCardRow.Create(startDate, income, outcome, 0m));
			}
		}

		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public int WarehouseId { get; }
		public string WarehouseName { get; }
		public int NomenclatureId { get; }
		public string NomenclatureName { get; }
		public ReadOnlyCollection<WarhouseAccountingCardRow> Rows => _rows.AsReadOnly();

		public static WarhouseAccountingCard Create(
			DateTime startDate,
			DateTime endDate,
			int warehouseId,
			string warehouseName,
			int nomenclatureId,
			string nomenclatureName,
			IEnumerable<WarehouseDocumentsItemsJournalNode> rows)
		{
			return new WarhouseAccountingCard(
				startDate,
				endDate,
				warehouseId,
				warehouseName,
				nomenclatureId,
				nomenclatureName,
				rows);
		}
	}

	public class WarhouseAccountingCardRow
	{
		private WarhouseAccountingCardRow(DateTime date, decimal income, decimal outcome, decimal residue)
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

		public static WarhouseAccountingCardRow Create(DateTime date, decimal income, decimal outcome, decimal residue)
		{
			return new WarhouseAccountingCardRow(date, income, outcome, residue);
		}
	}
}
