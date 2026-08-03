using System;
using Gamma.Utilities;
using Vodovoz.Domain.Logistic.Cars;
using VodovozCarTypeOfUse = Vodovoz.Domain.Logistic.Cars.CarTypeOfUse;

namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics.AverageFlowDiscrepanciesReport
{
	public class AverageFlowDiscrepanciesReportRow
	{
		public int CarId { get; internal set; }

		/// <summary>
		/// Авто (номер)
		/// </summary>
		public string Car { get; internal set; }

		/// <summary>
		/// Тип автомобиля
		/// </summary>
		public CarTypeOfUse? CarTypeOfUse { get; internal set; }

		/// <summary>
		/// Тип автомобиля
		/// </summary>
		public string CarTypeOfUseString => CarTypeOfUse?.GetEnumTitle();

		/// <summary>
		/// ФИО закрепленного за автомобилем водителя
		/// </summary>
		public string DriverFullName { get; internal set; }

		/// <summary>
		/// Прикрепление к площадке
		/// </summary>
		public string GeographicGroups { get; internal set; }

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
		/// Сумма км по МЛ на выполнение адресов (бывш. Подтверждённое расстояние)
		/// </summary>
		public decimal? ConfirmedDistance { get; internal set; }

		/// <summary>
		/// Факт по одометру (разница между последним и предпоследним показанием)
		/// </summary>
		public decimal? OdometerFact { get; internal set; }

		/// <summary>
		/// Полезный Пробег в процентах
		/// </summary>
		public string UsefulMileagePercent =>
			ConfirmedDistance.HasValue && OdometerFact.HasValue && ConfirmedDistance != 0
				? $"{(100 * OdometerFact / ConfirmedDistance): # ##0.00}"
				: string.Empty;

		/// <summary>
		/// Полезный Пробег в процентах
		/// </summary>
		public decimal? UsefulMileagePercentValue =>
			ConfirmedDistance.HasValue && OdometerFact.HasValue && ConfirmedDistance != 0
				? 100 * OdometerFact / ConfirmedDistance
				: null;

		/// <summary>
		/// Факт. расход
		/// </summary>
		public decimal? ConsumptionFact { get; internal set; }

		/// <summary>
		/// План. расход - вычисляется как OdometerFact * коэффициент в зависимости от типа ТС
		/// </summary>
		public decimal? ConsumptionPlan
		{
			get
			{
				if(OdometerFact.HasValue && CarTypeOfUse.HasValue)
				{
					return OdometerFact.Value * GetFuelCoefficient(CarTypeOfUse.Value);
				}
				return null;
			}
		}

		/// <summary>
		/// Разница по топливу между факт.расход и план.расход
		/// </summary>
		public decimal DiscrepancyFuel => (ConsumptionFact ?? 0) - (ConsumptionPlan ?? 0);

		/// <summary>
		/// Цена топлива на конец периода между калибровками
		/// </summary>
		public decimal? LastFuelCost { get; internal set; }

		/// <summary>
		/// Факт. расход на 100 км (расход / одометр * 100)
		/// </summary>
		public decimal Consumption100KmFact => OdometerFact.HasValue && OdometerFact != 0
			? (ConsumptionFact ?? 0) / (OdometerFact.Value / 100)
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

		/// <summary>
		/// Получить коэффициент расхода топлива в зависимости от типа ТС
		/// </summary>
		private decimal GetFuelCoefficient(CarTypeOfUse carType)
		{
			switch(carType)
			{
				case VodovozCarTypeOfUse.Largus:
					return 0.12m; 
				case VodovozCarTypeOfUse.Minivan:
				case VodovozCarTypeOfUse.GAZelle:
				case VodovozCarTypeOfUse.Truck:
					return 0.17m;
				default:
					return 0.17m;
			}
		}
	}
}
