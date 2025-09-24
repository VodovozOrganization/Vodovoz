using Microsoft.Extensions.Logging;
using QS.Dialog;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Profitability;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Profitability;
using Vodovoz.Factories;
using Vodovoz.Services;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Controllers
{
	public class RouteListProfitabilityController : IRouteListProfitabilityController
	{
		private readonly ILogger<RouteListProfitabilityController> _logger;
		private readonly IRouteListProfitabilityFactory _routeListProfitabilityFactory;
		private readonly IProfitabilityConstantsRepository _profitabilityConstantsRepository;
		private readonly IRouteListProfitabilityRepository _routeListProfitabilityRepository;
		private readonly IRouteListRepository _routeListRepository;
		private readonly int[] _paidDeliveriesNomenclaturesIds;

		public RouteListProfitabilityController(
			ILogger<RouteListProfitabilityController> logger,
			IRouteListProfitabilityFactory routeListProfitabilityFactory,
			INomenclatureSettings nomenclatureSettings,
			IProfitabilityConstantsRepository profitabilityConstantsRepository,
			IRouteListProfitabilityRepository routeListProfitabilityRepository,
			IRouteListRepository routeListRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_routeListProfitabilityFactory =
				routeListProfitabilityFactory ?? throw new ArgumentNullException(nameof(routeListProfitabilityFactory));
			_profitabilityConstantsRepository =
				profitabilityConstantsRepository ?? throw new ArgumentNullException(nameof(profitabilityConstantsRepository));
			_routeListProfitabilityRepository =
				routeListProfitabilityRepository ?? throw new ArgumentNullException(nameof(routeListProfitabilityRepository));
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_paidDeliveriesNomenclaturesIds =
				(nomenclatureSettings ?? throw new ArgumentNullException(nameof(nomenclatureSettings)))
				.PaidDeliveriesNomenclaturesIds;
		}

		public void CalculateNewRouteListProfitability(IUnitOfWork uow, RouteList routeList)
		{
			routeList.RouteListProfitability = CreateNewRouteListProfitability();
			CalculateRouteListProfitability(uow, routeList);
		}

		public void ReCalculateRouteListProfitability(IUnitOfWork uow, RouteList routeList, bool useDataFromDataBase = false)
		{
			//Для старых МЛ у которых не будет рассчитанных рентабельностей
			if(routeList.RouteListProfitability is null)
			{
				routeList.RouteListProfitability = CreateNewRouteListProfitability();
			}
			
			CalculateRouteListProfitability(uow, routeList, useDataFromDataBase);
		}

		public void RecalculateRouteListProfitabilitiesByCalculatedMonth(
			IUnitOfWork uow,
			DateTime date,
			bool useDataFromDataBase,
			IProgressBarDisplayable progressBarDisplayable)
		{
			progressBarDisplayable.Update("Готовимся к пересчету рентабельностей МЛ. Получаем необходимые данные для обработки...");
			var routeListsWithProfitabilities =
				_routeListProfitabilityRepository.GetAllRouteListsWithProfitabilitiesByCalculatedMonth(uow, date);

			var count = routeListsWithProfitabilities.Count();
			progressBarDisplayable.Start(count, 0, "Начинаем обработку...");
			
			var i = 0;
			foreach(var routeList in routeListsWithProfitabilities)
			{
				CalculateRouteListProfitability(uow, routeList, useDataFromDataBase);
				i++;
				progressBarDisplayable.Add(1, $"Обработано {i} из {count} рентабельностей МЛ");
			}
		}
		
		public void RecalculateRouteListProfitabilitiesByDate(IUnitOfWork uow, DateTime date, CarModel carModel = null, FuelType fuelType = null)
		{
			var routeListsWithProfitabilities =
				_routeListProfitabilityRepository.GetAllRouteListsWithProfitabilitiesByDate(uow, date, carModel, fuelType);

			foreach(var routeList in routeListsWithProfitabilities)
			{
				CalculateRouteListProfitability(uow, routeList);
			}
		}
		
		public void RecalculateRouteListProfitabilitiesBetweenDates(IUnitOfWork uow, DateTime dateFrom, DateTime dateTo)
		{
			var routeListsWithProfitabilities =
				_routeListProfitabilityRepository.GetAllRouteListsWithProfitabilitiesBetweenDates(uow, dateFrom, dateTo);

			foreach(var routeList in routeListsWithProfitabilities)
			{
				CalculateRouteListProfitability(uow, routeList);
			}
		}
		
		private RouteListProfitability CreateNewRouteListProfitability()
		{
			return _routeListProfitabilityFactory.CreateRouteListProfitability();
		}
		
		private void CalculateRouteListProfitability(IUnitOfWork uow, RouteList routeList, bool useDataFromDataBase = false)
		{
			_logger.LogDebug("Ищем версию авто...");

			var routeListProfitability = routeList.RouteListProfitability;
			var carVersion = routeList.Car?.CarVersions
				.Where(cv => cv.StartDate <= routeList.Date)
				.SingleOrDefault(cv => cv.EndDate == null || cv.EndDate > routeList.Date);

			_logger.LogDebug("Рассчитываем основные показатели рентабельности...");
			CalculateGeneralDataRouteListProfitability(uow, routeList, routeListProfitability, useDataFromDataBase);
			
			var nearestProfitabilityConstants = _profitabilityConstantsRepository.GetNearestProfitabilityConstantsByDate(
				uow, new DateTime(routeList.Date.Year, routeList.Date.Month, 1));
			
			_logger.LogDebug("Рассчитываем остальные показатели рентабельности...");
			if(routeList.HasFixedShippingPrice)
			{
				CalculateRouteListProfitabilityWithFixedShippingPrice(routeList, routeListProfitability);
			}
			else if(carVersion != null && carVersion.IsCompanyCar)
			{
				CalculateRouteListProfitabilityForCompanyCar(routeList, routeListProfitability, carVersion, nearestProfitabilityConstants);
			}
			else
			{
				CalculateRouteListProfitabilityForNotCompanyCar(routeList, routeListProfitability);
			}
			
			_logger.LogDebug("Рассчитываем затраты на кг...");
			routeListProfitability.RouteListExpensesPerKg = routeListProfitability.TotalGoodsWeight > 0
				? Math.Round(routeListProfitability.RouteListExpenses / routeListProfitability.TotalGoodsWeight, 2)
				: default(decimal);
			
			_logger.LogDebug("Рассчитываем валовую маржу...");
			CalculateRouteListProfitabilityGrossMargin(uow, routeList, routeListProfitability);
		}
		
		/// <summary>
		/// Считаем сумму продаж в МЛ, расходы и валовую маржу с показателем в процентах
		///
		/// !!!Важно!!! Если поменяется расчет в отчете <see cref="ProfitabilitySalesReportViewModel"/>, то нужно менять и здесь
		/// логику расчета и наоборот, при смене алгоритма в контроллере менять его механизм в отчете
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <param name="routeList">МЛ</param>
		/// <param name="routeListProfitability">Рентабельность МЛ</param>
		/// <param name="nearestProfitabilityConstants">Ближайшие контсанты рентабельности</param>
		private void CalculateRouteListProfitabilityGrossMargin(
			IUnitOfWork uow,
			RouteList routeList,
			RouteListProfitability routeListProfitability)
		{
			var routeListProfitabilitySpendings = _routeListRepository.GetRouteListSpendings(uow, routeList.Id, routeListProfitability.RouteListExpensesPerKg);
			routeListProfitability.SalesSum = routeListProfitabilitySpendings.TotalSales;
			routeListProfitability.ExpensesSum = routeListProfitabilitySpendings.GetTotalSpending();
			routeListProfitability.GrossMarginSum = routeListProfitability.SalesSum - routeListProfitability.ExpensesSum;
			routeListProfitability.GrossMarginPercents = routeListProfitability.SalesSum != 0
				? Math.Round(routeListProfitability.GrossMarginSum.Value / routeListProfitability.SalesSum.Value * 100, 2)
				: default(decimal);
		}

		private void CalculateGeneralDataRouteListProfitability(
			IUnitOfWork uow,
			RouteList routeList,
			RouteListProfitability routeListProfitability,
			bool useDataFromDataBase)
		{
			var paidDelivery = GetPaidDeliveriesSumFromRouteList(uow, routeList, useDataFromDataBase);

			routeListProfitability.Mileage = GetMileageFromRouteList(routeList);
			routeListProfitability.PaidDelivery = paidDelivery;
			routeListProfitability.TotalGoodsWeight = useDataFromDataBase
				? Math.Round(_routeListRepository.GetRouteListTotalSalesGoodsWeight(uow, routeList.Id), 2)
				: routeList.GetTotalSalesGoodsWeight();
		}

		private void CalculateRouteListProfitabilityWithFixedShippingPrice(
			RouteList routeList, RouteListProfitability routeListProfitability)
		{
			routeListProfitability.Amortisation = default(decimal);
			routeListProfitability.RepairCosts = default(decimal);
			routeListProfitability.FuelCosts = default(decimal);
			routeListProfitability.DriverAndForwarderWages = default(decimal);
			routeListProfitability.RouteListExpenses = routeList.FixedShippingPrice - routeListProfitability.PaidDelivery;
		}

		private void CalculateRouteListProfitabilityForCompanyCar(
			RouteList routeList,
			RouteListProfitability routeListProfitability,
			CarVersion carVersion,
			ProfitabilityConstants nearestProfitabilityConstants)
		{
			CalculateAmortisationAndRepairCosts(routeListProfitability, carVersion, nearestProfitabilityConstants);
			routeListProfitability.FuelCosts = CalculateFuelCosts(routeList.Car, routeListProfitability.Mileage, routeList.Date);
			routeListProfitability.DriverAndForwarderWages = routeList.GetDriversTotalWage() + routeList.GetForwardersTotalWage();
			routeListProfitability.RouteListExpenses =
				routeListProfitability.Amortisation +
				routeListProfitability.RepairCosts +
				routeListProfitability.FuelCosts +
				routeListProfitability.DriverAndForwarderWages - routeListProfitability.PaidDelivery;
		}

		private void CalculateAmortisationAndRepairCosts(
			RouteListProfitability routeListProfitability,
			CarVersion carVersion,
			ProfitabilityConstants nearestProfitabilityConstants)
		{
			var amortisationPerKm = default(decimal);
			var repairCostsPerKm = default(decimal);

			if(nearestProfitabilityConstants != null)
			{
				switch(carVersion.Car.CarModel.CarTypeOfUse)
				{
					case CarTypeOfUse.GAZelle:
						amortisationPerKm = nearestProfitabilityConstants.GazelleAmortisationPerKm;
						repairCostsPerKm = nearestProfitabilityConstants.GazelleRepairCostPerKm;
						break;
					case CarTypeOfUse.Largus:
						amortisationPerKm = nearestProfitabilityConstants.LargusAmortisationPerKm;
						repairCostsPerKm = nearestProfitabilityConstants.LargusRepairCostPerKm;
						break;
					case CarTypeOfUse.Truck:
						amortisationPerKm = nearestProfitabilityConstants.TruckAmortisationPerKm;
						repairCostsPerKm = nearestProfitabilityConstants.TruckRepairCostPerKm;
						break;
				}

				routeListProfitability.ProfitabilityConstantsCalculatedMonth = nearestProfitabilityConstants.CalculatedMonth;
			}

			routeListProfitability.Amortisation = amortisationPerKm * routeListProfitability.Mileage;
			routeListProfitability.RepairCosts = repairCostsPerKm * routeListProfitability.Mileage;
		}

		private void CalculateRouteListProfitabilityForNotCompanyCar(RouteList routeList, RouteListProfitability routeListProfitability)
		{
			routeListProfitability.Amortisation = default(decimal);
			routeListProfitability.RepairCosts = default(decimal);
			routeListProfitability.FuelCosts = default(decimal);
			routeListProfitability.DriverAndForwarderWages = routeList.GetDriversTotalWage() + routeList.GetForwardersTotalWage();
			routeListProfitability.RouteListExpenses =
				routeListProfitability.Amortisation +
				routeListProfitability.RepairCosts +
				routeListProfitability.FuelCosts +
				routeListProfitability.DriverAndForwarderWages - routeListProfitability.PaidDelivery;
		}

		private decimal GetMileageFromRouteList(RouteList routeList)
		{
			if(routeList.ConfirmedDistance > 0)
			{
				return routeList.ConfirmedDistance;
			}
			if(routeList.RecalculatedDistance.HasValue)
			{
				return routeList.RecalculatedDistance.Value;
			}
			if(routeList.PlanedDistance.HasValue)
			{
				return routeList.PlanedDistance.Value;
			}

			return default(decimal);
		}

		private decimal GetPaidDeliveriesSumFromRouteList(IUnitOfWork uow, RouteList routeList, bool useDataFromDataBase)
		{
			var paidDeliveriesSum = useDataFromDataBase ?
				_routeListRepository.GetRouteListPaidDeliveriesSum(uow, routeList.Id, _paidDeliveriesNomenclaturesIds)
				: routeList.Addresses
					.Where(a => a.Status != RouteListItemStatus.Transfered)
					.SelectMany(ri => ri.Order.OrderItems)
					.Where(oi => _paidDeliveriesNomenclaturesIds.Contains(oi.Nomenclature.Id))
					.Sum(oi => oi.ActualSum);

			return paidDeliveriesSum;
		}

		private decimal CalculateFuelCosts(Car car, decimal mileage, DateTime date)
		{
			var fuelConsumption = car.CarModel.CarFuelVersions
				.Where(v => v.StartDate <= date)
				.Where(v => v.EndDate == null || v.EndDate > date)
				.Select(v => v.FuelConsumption)
				.SingleOrDefault();

			var fuelCost = car.FuelType.FuelPriceVersions
				.Where(v => v.StartDate <= date)
				.Where(v => v.EndDate == null || v.EndDate > date)
				.Select(v => v.FuelPrice)
				.SingleOrDefault();

			return fuelConsumption > 0
				? mileage / 100 * (decimal)fuelConsumption * fuelCost
				: default(decimal);
		}
	}
}
