using System;
using System.Linq;
using Gamma.Utilities;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class RouteListRatesLevelWageCalculationService : IRouteListWageCalculationService
	{
		private readonly RatesLevelWageParameterItem wageParameterItem;
		private readonly IRouteListWageCalculationSource wageCalculationSource;

		public RouteListRatesLevelWageCalculationService(RatesLevelWageParameterItem wageParameterItem, IRouteListWageCalculationSource wageCalculationSource)
		{
			this.wageParameterItem = wageParameterItem ?? throw new ArgumentNullException(nameof(wageParameterItem));
			this.wageCalculationSource = wageCalculationSource ?? throw new ArgumentNullException(nameof(wageCalculationSource));
		}

		public RouteListWageResult CalculateWage()
		{
			var wage = wageCalculationSource.ItemSources.Sum(s => CalculateWageForRouteListItem(s).Wage);
			return new RouteListWageResult(wage, 0);
		}

		public RouteListItemWageResult CalculateWageForRouteListItem(IRouteListItemWageCalculationSource src)
		{
			decimal resultSum = 0;

			if(!src.IsDelivered) {
				return new RouteListItemWageResult(
					0,
					GetCurrentWageDistrictLevelRate(src)
				);
			}

			#region Оплата оборудования, если нет 19л воды в заказе
			var wageForBottlesOrEquipment = CalculateWageForFull19LBottles(src);
			if(wageForBottlesOrEquipment <= 0)
				wageForBottlesOrEquipment = CalculateWageForEquipment(src);
			#endregion Оплата оборудования, если нет 19л воды в заказе

			resultSum += CalculateWageForAddress(src);
			resultSum += wageForBottlesOrEquipment;
			resultSum += CalculateWageForEmpty19LBottles(src);
			resultSum += CalculateWageFor600mlBottles(src);
			resultSum += CalculateWageFor6LBottles(src);
			resultSum += CalculateWageFor1500mlBottles(src);

			return new RouteListItemWageResult(
				resultSum,
				GetCurrentWageDistrictLevelRate(src)
			);
		}

		private decimal GetRateValue(IRouteListItemWageCalculationSource src, WageRate rate)
		{
			if(rate == null)
				return 0;
			switch(wageCalculationSource.EmployeeCategory) {
				case EmployeeCategory.driver:
					return src.WasVisitedByForwarder ? rate.WageForDriverWithForwarder(src): rate.WageForDriverWithoutForwarder(src);
				case EmployeeCategory.forwarder:
					return rate.WageForForwarder(src);
				case EmployeeCategory.office:
				default:
					throw new InvalidOperationException($"Для указанного типа сотрудника \"{wageCalculationSource.EmployeeCategory.GetEnumTitle()}\" не предусмотрен расчет зарплаты по уровням");
			}
		}

		/// <summary>
		/// Оплата адреса
		/// </summary>
		decimal CalculateWageForAddress(IRouteListItemWageCalculationSource src)
		{
			if(!src.HasFirstOrderForDeliveryPoint)
				return 0;

			var rate = GetCurrentWageDistrictLevelRate(src).WageRates.FirstOrDefault(r => r.WageRateType == WageRateTypes.Address);

			return GetRateValue(src, rate);
		}

		/// <summary>
		/// Большой ли заказ
		/// </summary>
		bool HasBigOrder(IRouteListItemWageCalculationSource src)
		{
			var rate = GetCurrentWageDistrictLevelRate(src).WageRates.FirstOrDefault(r => r.WageRateType == WageRateTypes.MinBottlesQtyInBigOrder);

			return src.FullBottle19LCount >= GetRateValue(src, rate);
		}

		/// <summary>
		/// Оплата полных бутылей
		/// </summary>
		decimal CalculateWageForFull19LBottles(IRouteListItemWageCalculationSource src)
		{
			bool addressWithBigOrder = HasBigOrder(src);

			var rate = GetCurrentWageDistrictLevelRate(src).WageRates
														   .FirstOrDefault(
																r => r.WageRateType == (
																	addressWithBigOrder ? WageRateTypes.Bottle19LInBigOrder : WageRateTypes.Bottle19L
																)
															);

			decimal paymentForOne = GetRateValue(src, rate);

			return paymentForOne * src.FullBottle19LCount;
		}

		/// <summary>
		/// Оплата забора пустых бутылей
		/// </summary>
		decimal CalculateWageForEmpty19LBottles(IRouteListItemWageCalculationSource src)
		{
			bool addressWithBigOrder = HasBigOrder(src);

			var rate = GetCurrentWageDistrictLevelRate(src).WageRates
														   .FirstOrDefault(
																r => r.WageRateType == (
																	addressWithBigOrder ? WageRateTypes.EmptyBottle19LInBigOrder : WageRateTypes.EmptyBottle19L
																)
															);

			decimal paymentForOne = GetRateValue(src, rate);

			return paymentForOne * src.EmptyBottle19LCount;
		}

		/// <summary>
		/// Оплата 0.6л бутылей
		/// </summary>
		decimal CalculateWageFor600mlBottles(IRouteListItemWageCalculationSource src)
		{
			var rate = GetCurrentWageDistrictLevelRate(src).WageRates.FirstOrDefault(r => r.WageRateType == WageRateTypes.PackOfBottles600ml);

			decimal paymentForOnePack = GetRateValue(src, rate);

			return Math.Truncate(paymentForOnePack / 36 * src.Bottle600mlCount);
		}

		/// <summary>
		/// Оплата забор-доставки оборудования
		/// </summary>
		decimal CalculateWageForEquipment(IRouteListItemWageCalculationSource src)
		{
			if(!src.NeedTakeOrDeliverEquipment)
				return 0;

			var rate = GetCurrentWageDistrictLevelRate(src).WageRates.FirstOrDefault(r => r.WageRateType == WageRateTypes.Equipment);

			return GetRateValue(src, rate);
		}

		/// <summary>
		/// Оплата доставки 6л бутылей
		/// </summary>
		decimal CalculateWageFor6LBottles(IRouteListItemWageCalculationSource src)
		{
			WageDistrictLevelRate wageCalcMethodic = GetCurrentWageDistrictLevelRate(src);

			var rate = wageCalcMethodic.WageRates.FirstOrDefault(r => r.WageRateType == WageRateTypes.Bottle6L);

			decimal paymentForOne = GetRateValue(src, rate);

			return paymentForOne * src.Bottle6LCount;
		}

		/// <summary>
		/// Оплата доставки 1,5л бутылей
		/// </summary>
		decimal CalculateWageFor1500mlBottles(IRouteListItemWageCalculationSource src)
		{
			WageDistrictLevelRate wageCalcMethodic = GetCurrentWageDistrictLevelRate(src);

			var rate = wageCalcMethodic.WageRates.FirstOrDefault(r => r.WageRateType == WageRateTypes.Bottle1500ml);

			decimal paymentForOne = GetRateValue(src, rate);

			return paymentForOne * src.Bottle1500mlCount;
		}

		/// <summary>
		/// Возврат текущей методики расчёта ЗП. Берёться значение либо
		/// актуальное из сотрудника, в случае первого расчёта, либо из
		/// сохранённого в уже посчитанном адресе МЛ
		/// </summary>
		WageDistrictLevelRate GetCurrentWageDistrictLevelRate(IRouteListItemWageCalculationSource src)
		{
			return src.WageCalculationMethodic ?? wageParameterItem.WageDistrictLevelRates
															   .LevelRates
															   .FirstOrDefault(r => r.WageDistrict == src.WageDistrictOfAddress);
		}
	}
}