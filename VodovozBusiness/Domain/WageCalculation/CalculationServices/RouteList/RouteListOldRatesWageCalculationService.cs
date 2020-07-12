using System;
using System.Data.Bindings;
using System.Linq;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class RouteListOldRatesWageCalculationService : IRouteListWageCalculationService
	{
		private readonly OldRatesWageParameterItem wageParameterItem;
		private readonly IRouteListWageCalculationSource source;

		public RouteListOldRatesWageCalculationService(OldRatesWageParameterItem wageParameterItem, IRouteListWageCalculationSource source)
		{
			this.wageParameterItem = wageParameterItem ?? throw new ArgumentNullException(nameof(wageParameterItem));
			this.source = source ?? throw new ArgumentNullException(nameof(source));
		}

		public RouteListWageResult CalculateWage()
		{
			var wage = source.ItemSources.Sum(s => CalculateWageForRouteListItem(s).Wage);
			return new RouteListWageResult(wage, 0);
		}

		public RouteListItemWageResult CalculateWageForRouteListItem(IRouteListItemWageCalculationSource src)
		{
			decimal resultSum = 0;

			if(!src.IsDelivered) {
				return new RouteListItemWageResult(0);
			}

			if(source.IsTruck) {
				return new RouteListItemWageResult(0);
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

			return new RouteListItemWageResult(
				resultSum
			);
		}

		private decimal GetRateValue(IRouteListItemWageCalculationSource itemSource, WageRate rate)
		{
			switch(source.EmployeeCategory) {
				case EmployeeCategory.driver:
					return itemSource.WasVisitedByForwarder ? rate.ForDriverWithForwarder : rate.ForDriverWithoutForwarder;
				case EmployeeCategory.forwarder:
					return rate.ForForwarder;
				case EmployeeCategory.office:
				default:
					throw new InvalidOperationException($"Для указанного типа сотрудника \"{source.EmployeeCategory.GetEnumTitle()}\" не предусмотрен расчет зарплаты");
			}
		}

		private WageRate GetActualRate(WageRateTypes wageRateType)
		{
			if(source.DriverOfOurCar) {
				return wageParameterItem.GetRateForOurs(source.RouteListDate, wageRateType);
			} else {
				return wageParameterItem.GetRateForMercenaries(source.RouteListDate, wageRateType);
			}
		}

		/// <summary>
		/// Оплата адреса
		/// </summary>
		decimal CalculateWageForAddress(IRouteListItemWageCalculationSource src)
		{
			if(!src.HasFirstOrderForDeliveryPoint)
				return 0;

			var rate = GetActualRate(WageRateTypes.Address);
			return GetRateValue(src, rate);
		}

		/// <summary>
		/// Большой ли заказ
		/// </summary>
		bool HasBigOrder(IRouteListItemWageCalculationSource src)
		{
			var rate = GetActualRate(WageRateTypes.MinBottlesQtyInBigOrder);
			return src.FullBottle19LCount >= (GetRateValue(src, rate));
		}

		/// <summary>
		/// Оплата полных бутылей
		/// </summary>
		decimal CalculateWageForFull19LBottles(IRouteListItemWageCalculationSource src)
		{
			bool addressWithBigOrder = HasBigOrder(src);

			var rate = GetActualRate(addressWithBigOrder ? WageRateTypes.Bottle19LInBigOrder : WageRateTypes.Bottle19L);
			decimal paymentForOne = GetRateValue(src, rate);

			return paymentForOne * src.FullBottle19LCount;
		}

		/// <summary>
		/// Оплата забора пустых бутылей
		/// </summary>
		decimal CalculateWageForEmpty19LBottles(IRouteListItemWageCalculationSource src)
		{
			bool addressWithBigOrder = HasBigOrder(src);

			var rate = GetActualRate(addressWithBigOrder ? WageRateTypes.EmptyBottle19LInBigOrder : WageRateTypes.EmptyBottle19L);
			decimal paymentForOne = GetRateValue(src, rate);

			return paymentForOne * src.EmptyBottle19LCount;
		}

		/// <summary>
		/// Оплата 0.6л бутылей
		/// </summary>
		decimal CalculateWageFor600mlBottles(IRouteListItemWageCalculationSource src)
		{
			var rate = GetActualRate(WageRateTypes.PackOfBottles600ml);
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

			var rate = GetActualRate(WageRateTypes.Equipment);
			return GetRateValue(src, rate);
		}

		/// <summary>
		/// Оплата доставки 6л бутылей
		/// </summary>
		decimal CalculateWageFor6LBottles(IRouteListItemWageCalculationSource src)
		{
			var rate = GetActualRate(WageRateTypes.Bottle6L);
			decimal paymentForOne = GetRateValue(src, rate);

			return paymentForOne * src.Bottle6LCount;
		}
	}
}
