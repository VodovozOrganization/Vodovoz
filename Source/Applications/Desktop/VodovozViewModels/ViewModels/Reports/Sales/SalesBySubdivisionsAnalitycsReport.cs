using DateTimeHelpers;
using System;

namespace Vodovoz.ViewModels.ViewModels.Reports.Sales
{
	public class SalesBySubdivisionsAnalitycsReport
	{
		private SalesBySubdivisionsAnalitycsReport(
			DateTimePeriod firstPeriod,
			DateTimePeriod secondPeriod,
			bool splitByNomenclatures,
			bool splitBySubdivisions,
			bool splitByWarehouses)
		{
			FirstPeriod = firstPeriod;
			SecondPeriod = secondPeriod;
			SplitByNomenclatures = splitByNomenclatures;
			SplitBySubdivisions = splitBySubdivisions;
			SplitByWarehouses = splitByWarehouses;
		}

		public DateTimePeriod FirstPeriod { get; }

		public DateTimePeriod SecondPeriod { get; }

		public bool SplitByNomenclatures { get; }

		public bool SplitBySubdivisions { get; }

		public bool SplitByWarehouses { get; }

		public static SalesBySubdivisionsAnalitycsReport Create(
			DateTimePeriod firstPeriod,
			DateTimePeriod secondPeriod,
			bool splitByNomenclatures,
			bool splitBySubdivisions,
			bool splitByWarehouses)
		{
			ValidateParameters(
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
				splitByWarehouses);
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
				&& (firstPeriod.EndDateTime - firstPeriod.StartDateTime).TotalDays > 1)
			{
				throw new ArgumentException("Нельзя выбрать разбивку по складам для отчета с интервалом более одного дня",
					nameof(splitByWarehouses));
			}
		}
	}
}
