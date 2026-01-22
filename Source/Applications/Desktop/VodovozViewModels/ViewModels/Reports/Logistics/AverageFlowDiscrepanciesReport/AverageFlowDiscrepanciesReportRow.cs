using System;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics.AverageFlowDiscrepanciesReport
{
	public class AverageFlowDiscrepanciesReportRow
	{
		/// <summary>
		/// Авто (номер)
		/// </summary>
		public string Car { get; internal set; }

		/// <summary>
		/// Дата калибровки
		/// </summary>
		public DateTime? CalibrationDate { get; internal set; }

		/// <summary>
		/// Текущий баланс на момент сохранения события
		/// </summary>
		public decimal? CurrentBalance { get; internal set; }

		/// <summary>
		/// Актуальный баланс на момент сохранения события
		/// </summary>
		public decimal? ActualBalance { get; internal set; }

		/// <summary>
		/// Подтверждённое расстояние
		/// </summary>
		public decimal? ConfirmedDistance { get; internal set; }

		/// <summary>
		/// Полезный Пробег в процентах
		/// </summary>
		public string UsefulMileagePercent =>
			ConfirmedDistance.HasValue && RecalculatedDistance.HasValue && ConfirmedDistance != 0
				? $"{(100 * RecalculatedDistance / ConfirmedDistance): # ##0.00}"
				: string.Empty;

		/// <summary>
		/// Пересчитанное расстояние
		/// </summary>
		public decimal? RecalculatedDistance { get; internal set; }

		/// <summary>
		/// Факт. расход
		/// </summary>
		public decimal? ConsumptionFact { get; internal set; }

		/// <summary>
		/// План. расход
		/// </summary>
		public decimal? ConsumptionPlan =>
			ConfirmedDistance is null || ConfirmedDistance == 0
			? RecalculatedDistance / 100 * (decimal?)Consumption100KmPlan
			: ConfirmedDistance / 100 * (decimal?)Consumption100KmPlan;

		/// <summary>
		/// Разница по топливу между факт.расход и план.расход
		/// </summary>
		public decimal DiscrepancyFuel => (ConsumptionFact ?? 0) - (ConsumptionPlan ?? 0);

		/// <summary>
		/// Цена топлива на конец периода между калибровками
		/// </summary>
		public decimal? LastFuelCost { get; internal set; }

		/// <summary>
		/// Факт. расход на 100 км
		/// </summary>
		public decimal Consumption100KmFact => ConfirmedDistance.HasValue && ConfirmedDistance != 0
			? (ConsumptionFact ?? 0) / ((ConfirmedDistance ?? 0) / 100)
			: 0;

		/// <summary>
		/// План. расход на 100 км
		/// </summary>
		public double? Consumption100KmPlan { get; internal set; }

		/// <summary>
		/// Процент расхождения
		/// </summary>
		public decimal DiscrepancyPercent => Consumption100KmPlan > 0
			? ((Consumption100KmFact
				/ (((decimal?)Consumption100KmPlan) ?? 0)) - 1)
				* 100
			: 0;

		/// <summary>
		/// Дата следующей калибровки
		/// </summary>
		public DateTime? NextCalibrationDate { get; internal set; }

		/// <summary>
		/// Операция калибровки на конец периода между калибровками
		/// </summary>
		public decimal? NextCalibrationFuelOperation { get; internal set; }

		/// <summary>
		/// Единственная калибровка за период
		/// </summary>
		public bool IsSingleCalibrationForPeriod { get; internal set; }
	}
}
