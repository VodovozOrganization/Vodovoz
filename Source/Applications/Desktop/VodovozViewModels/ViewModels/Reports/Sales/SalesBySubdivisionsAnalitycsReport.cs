using DateTimeHelpers;
using System;
using System.Collections.Generic;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public class SalesBySubdivisionsAnalitycsReport
	{
		private readonly List<Row> _rows = new List<Row>();

		private SalesBySubdivisionsAnalitycsReport(
			DateTimePeriod firstPeriod,
			DateTimePeriod secondPeriod,
			bool splitByNomenclatures,
			bool splitBySubdivisions,
			bool splitByWarehouses,
			IEnumerable<DataNode> dataNodes)
		{
			FirstPeriod = firstPeriod;
			SecondPeriod = secondPeriod;
			SplitByNomenclatures = splitByNomenclatures;
			SplitBySubdivisions = splitBySubdivisions;
			SplitByWarehouses = splitByWarehouses;

			Process(dataNodes);

			CreatedAt = DateTime.Now;
		}

		private void Process(IEnumerable<DataNode> dataNodes)
		{
			throw new NotImplementedException();
		}

		public DateTimePeriod FirstPeriod { get; }

		public DateTimePeriod SecondPeriod { get; }

		public bool SplitByNomenclatures { get; }

		public bool SplitBySubdivisions { get; }

		public bool SplitByWarehouses { get; }

		public DateTime CreatedAt { get; }

		public IReadOnlyCollection<Row> Rows => _rows.AsReadOnly();

		public static SalesBySubdivisionsAnalitycsReport Create(
			DateTimePeriod firstPeriod,
			DateTimePeriod secondPeriod,
			bool splitByNomenclatures,
			bool splitBySubdivisions,
			bool splitByWarehouses,
			Func<DateTimePeriod, DateTimePeriod, bool, bool, bool, IEnumerable<DataNode>> retrieveFunction)
		{
			if(retrieveFunction is null)
			{
				throw new ArgumentNullException(nameof(retrieveFunction));
			}

			ValidateParameters(
				firstPeriod,
				secondPeriod,
				splitByNomenclatures,
				splitBySubdivisions,
				splitByWarehouses);

			IEnumerable<DataNode> dataNodes = retrieveFunction.Invoke(
				firstPeriod,
				secondPeriod,
				splitByNomenclatures,
				splitBySubdivisions,
				splitByWarehouses);

			return new SalesBySubdivisionsAnalitycsReport(
				firstPeriod,
				secondPeriod,
				splitByNomenclatures,
				splitBySubdivisions,
				splitByWarehouses,
				dataNodes);
		}

		private static void ValidateParameters(
			DateTimePeriod firstPeriod,
			DateTimePeriod secondPeriod,
			bool splitByNomenclatures,
			bool splitBySubdivisions,
			bool splitByWarehouses)
		{
			if(firstPeriod == null)
			{
				throw new ArgumentNullException("Нельзя создать отчет не указав период",
					nameof(firstPeriod));
			}

			if(splitByWarehouses && secondPeriod != null)
			{
				throw new ArgumentException("Нельзя выбрать разбивку по складам для отчета с двумя периодами",
					nameof(splitByWarehouses));
			}

			if(splitByWarehouses
				&& (firstPeriod.EndDateTime - firstPeriod.StartDateTime)?.TotalDays > 1)
			{
				throw new ArgumentException("Нельзя выбрать разбивку по складам для отчета с интервалом более одного дня",
					nameof(splitByWarehouses));
			}
		}

		public class Row
		{

		}

		public class DataNode
		{
			public int nomenclatureId { get; set; }

			public string nomenclatureName { get; set; }

			public int productGroupId { get; set; }

			public string productGroupName { get; set; }

			public int subdivisionId { get; set; }

			public string subdivisionName { get; set; }
		}
	}
}
