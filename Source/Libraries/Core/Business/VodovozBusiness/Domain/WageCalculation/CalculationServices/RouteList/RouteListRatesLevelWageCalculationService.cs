using Gamma.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;

namespace Vodovoz.Domain.WageCalculation.CalculationServices.RouteList
{
	public class RouteListRatesLevelWageCalculationService : IRouteListWageCalculationService
	{
		private readonly RatesLevelWageParameterItem _wageParameterItem;
		private readonly IRouteListWageCalculationSource _wageCalculationSource;

		public RouteListRatesLevelWageCalculationService(RatesLevelWageParameterItem wageParameterItem, IRouteListWageCalculationSource wageCalculationSource)
		{
			_wageParameterItem = wageParameterItem ?? throw new ArgumentNullException(nameof(wageParameterItem));
			_wageCalculationSource = wageCalculationSource ?? throw new ArgumentNullException(nameof(wageCalculationSource));
		}

		public RouteListWageResult CalculateWage()
		{
			var wage = _wageCalculationSource.ItemSources.Sum(s => CalculateWageForRouteListItem(s).Wage);
			return new RouteListWageResult(wage, 0);
		}

		public RouteListItemWageResult CalculateWageForRouteListItem(IRouteListItemWageCalculationSource src)
		{
			decimal resultSum = 0;

			if(!src.IsValidForWageCalculation || !Car.GetCarTypesOfUseForRatesLevelWageCalculation().Contains(src.CarTypeOfUse))
			{
				return new RouteListItemWageResult(0, GetCurrentWageDistrictLevelRate(src));
			}

			#region Оплата оборудования, если нет 19л воды в заказе

			var wageForBottlesOrEquipment = CalculateWageForFull19LBottles(src);

			if(wageForBottlesOrEquipment <= 0)
			{
				wageForBottlesOrEquipment = CalculateWageForEquipment(src);
			}

			#endregion Оплата оборудования, если нет 19л воды в заказе

			resultSum += CalculateWageForAddress(src);
			resultSum += wageForBottlesOrEquipment;
			resultSum += CalculateWageForEmpty19LBottles(src);
			resultSum += CalculateWageFor600mlBottles(src);
			resultSum += CalculateWageFor6LBottles(src);
			resultSum += CalculateWageFor1500mlBottles(src);
			resultSum += CalculateWageFor500mlBottles(src);
			resultSum += CalculateWageForFastDelivery(src);

			return new RouteListItemWageResult(
				resultSum,
				GetCurrentWageDistrictLevelRate(src)
			);
		}

		private decimal GetRateValue(IRouteListItemWageCalculationSource src, WageRate rate)
		{
			if(rate == null)
			{
				return 0;
			}

			switch(_wageCalculationSource.EmployeeCategory) {
				case EmployeeCategory.driver:
					return src.WasVisitedByForwarder ? rate.WageForDriverWithForwarder(src): rate.WageForDriverWithoutForwarder(src);
				case EmployeeCategory.forwarder:
					return rate.WageForForwarder(src);
				case EmployeeCategory.office:
				default:
					throw new InvalidOperationException($"Для указанного типа сотрудника \"{_wageCalculationSource.EmployeeCategory.GetEnumTitle()}\" не предусмотрен расчет зарплаты по уровням");
			}
		}

		/// <summary>
		/// Оплата адреса
		/// </summary>
		decimal CalculateWageForAddress(IRouteListItemWageCalculationSource src)
		{
			if(!src.HasFirstOrderForDeliveryPoint)
			{
				return 0;
			}

			var rate = GetCurrentWageDistrictLevelRate(src).WageRates.FirstOrDefault(r => r.WageRateType == (src.IsDriverForeignDistrict? WageRateTypes.ForeignAddress : WageRateTypes.Address));

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
						addressWithBigOrder
						? WageRateTypes.Bottle19LInBigOrder
						: WageRateTypes.Bottle19L));

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
						addressWithBigOrder
						? WageRateTypes.EmptyBottle19LInBigOrder
						: WageRateTypes.EmptyBottle19L));

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
			{
				return 0;
			}

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
		/// Оплата доставки 0,5л бутылей
		/// </summary>
		decimal CalculateWageFor500mlBottles(IRouteListItemWageCalculationSource src)
		{
			WageDistrictLevelRate wageCalcMethodic = GetCurrentWageDistrictLevelRate(src);

			var rate = wageCalcMethodic.WageRates.FirstOrDefault(r => r.WageRateType == WageRateTypes.Bottle500ml);

			decimal paymentForOne = GetRateValue(src, rate);

			return paymentForOne * src.Bottle500mlCount;
		}

		/// <summary>
		/// Оплата доставки за час
		/// </summary>
		decimal CalculateWageForFastDelivery(IRouteListItemWageCalculationSource src)
		{
			if(!src.IsFastDelivery)
			{
				return 0;
			}

			var fastDeliveryWageRateType = src.GetFastDeliveryWageRateType();

			WageDistrictLevelRate wageCalcMethodic = GetCurrentWageDistrictLevelRate(src);

			var rate = wageCalcMethodic.WageRates.FirstOrDefault(r => r.WageRateType == fastDeliveryWageRateType);

			return GetRateValue(src, rate);
		}

		/// <summary>
		/// Возврат текущей методики расчёта ЗП. Берёться значение либо
		/// актуальное из сотрудника, в случае первого расчёта, либо из
		/// сохранённого в уже посчитанном адресе МЛ
		/// </summary>
		WageDistrictLevelRate GetCurrentWageDistrictLevelRate(IRouteListItemWageCalculationSource src)
		{
			return src.WageCalculationMethodic
				?? _wageParameterItem.WageDistrictLevelRates.LevelRates
					.FirstOrDefault(r => r.WageDistrict == src.WageDistrictOfAddress && r.CarTypeOfUse == src.CarTypeOfUse);
		}

		public RouteListItemWageCalculationDetails GetWageCalculationDetailsForRouteListItem(IRouteListItemWageCalculationSource src)
		{
			var levelRate = GetCurrentWageDistrictLevelRate(src);
			var addressWageDetails = new RouteListItemWageCalculationDetails
			{
				RouteListItemWageCalculationName = $"{WageParameterItemTypes.RatesLevel.GetEnumTitle()}, {levelRate.WageDistrictLevelRates.Name} №{levelRate.WageDistrictLevelRates.Id}. ",
				WageCalculationEmployeeCategory = src.EmployeeCategory
			};

			if(!src.IsValidForWageCalculation || !Car.GetCarTypesOfUseForRatesLevelWageCalculation().Contains(src.CarTypeOfUse))
			{
				return addressWageDetails;
			}

			IList<WageRate> wageRates = GetCurrentWageDistrictLevelRate(src).WageRates;

			if(!src.HasFirstOrderForDeliveryPoint)
			{
				addressWageDetails.WageCalculationDetailsList.Add(
					new WageCalculationDetailsItem()
					{
						Name = $"Не первый заказ на точку доставки",
					});
			}
			else
			{
				var rateAddress = wageRates.FirstOrDefault(r =>
					r.WageRateType == (src.IsDriverForeignDistrict ? WageRateTypes.ForeignAddress : WageRateTypes.Address));

				if(rateAddress != null)
				{
					addressWageDetails.WageCalculationDetailsList.Add(
						new WageCalculationDetailsItem()
						{
							Name = $"{rateAddress.WageRateType.GetEnumTitle()}",
							Count = 1,
							Price = GetRateValue(src, rateAddress)
						});
				}
			}

			bool addressWithBigOrder = HasBigOrder(src);
			var rateFullBottle19L = wageRates
				.FirstOrDefault(r => r.WageRateType == (addressWithBigOrder ? WageRateTypes.Bottle19LInBigOrder : WageRateTypes.Bottle19L));

			var priceFullBottle19L = GetRateValue(src, rateFullBottle19L);
			if(priceFullBottle19L * src.FullBottle19LCount > 0)
			{
				addressWageDetails.WageCalculationDetailsList.Add(
					new WageCalculationDetailsItem()
					{
						Name = rateFullBottle19L.WageRateType.GetEnumTitle(),
						Count = src.FullBottle19LCount,
						Price = priceFullBottle19L
					});
			}
			else
			{
				if(src.NeedTakeOrDeliverEquipment)
				{
					addressWageDetails.WageCalculationDetailsList.Add(
						new WageCalculationDetailsItem()
						{
							Name = WageRateTypes.Equipment.GetEnumTitle(),
							Count = 1,
							Price = GetRateValue(src, wageRates.FirstOrDefault(r => r.WageRateType == WageRateTypes.Equipment))
						});
				}
			}

			addressWageDetails.WageCalculationDetailsList.Add(
				new WageCalculationDetailsItem()
				{
					Name = addressWithBigOrder ? WageRateTypes.EmptyBottle19LInBigOrder.GetEnumTitle() : WageRateTypes.EmptyBottle19L.GetEnumTitle(),
					Count = src.EmptyBottle19LCount,
					Price = GetRateValue(src,
						wageRates
							.FirstOrDefault(r => r.WageRateType == (addressWithBigOrder ? WageRateTypes.EmptyBottle19LInBigOrder : WageRateTypes.EmptyBottle19L))
						)
				});

			addressWageDetails.WageCalculationDetailsList.Add(
				new WageCalculationDetailsItem()
				{
					Name = WageRateTypes.PackOfBottles600ml.GetEnumTitle(),
					Count = src.Bottle600mlCount,
					Price = GetRateValue(src, wageRates.FirstOrDefault(r => r.WageRateType == WageRateTypes.PackOfBottles600ml)) / 36
				});

			addressWageDetails.WageCalculationDetailsList.Add(
				new WageCalculationDetailsItem()
				{
					Name = WageRateTypes.Bottle6L.GetEnumTitle(),
					Count = src.Bottle6LCount,
					Price = GetRateValue(src, wageRates.FirstOrDefault(r => r.WageRateType == WageRateTypes.Bottle6L))
				});

			addressWageDetails.WageCalculationDetailsList.Add(
				new WageCalculationDetailsItem()
				{
					Name = WageRateTypes.Bottle1500ml.GetEnumTitle(),
					Count = src.Bottle1500mlCount,
					Price = GetRateValue(src, wageRates.FirstOrDefault(r => r.WageRateType == WageRateTypes.Bottle1500ml))
				});

			addressWageDetails.WageCalculationDetailsList.Add(
				new WageCalculationDetailsItem()
				{
					Name = WageRateTypes.Bottle500ml.GetEnumTitle(),
					Count = src.Bottle500mlCount,
					Price = GetRateValue(src, wageRates.FirstOrDefault(r => r.WageRateType == WageRateTypes.Bottle500ml))
				});

			if(src.IsFastDelivery)
			{
				var fastDeliveryWageRateType = src.GetFastDeliveryWageRateType();

				addressWageDetails.WageCalculationDetailsList.Add(
					new WageCalculationDetailsItem()
					{
						Name = fastDeliveryWageRateType.GetEnumTitle(),
						Count = 1,
						Price = GetRateValue(src, wageRates.FirstOrDefault(r => r.WageRateType == fastDeliveryWageRateType))
					});
			}

			return addressWageDetails;
		}
	}
}
