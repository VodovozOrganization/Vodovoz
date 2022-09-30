using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Profitability;
using Vodovoz.EntityRepositories.Profitability;
using Vodovoz.Factories;
using Vodovoz.Services;

namespace Vodovoz.Controllers
{
	public class RouteListProfitabilityController : IRouteListProfitabilityController
	{
		private readonly IRouteListProfitabilityFactory _routeListProfitabilityFactory;
		private readonly IProfitabilityConstantsRepository _profitabilityConstantsRepository;
		private readonly IRouteListProfitabilityRepository _routeListProfitabilityRepository;
		private readonly int[] _paidDeliveriesNomenclaturesIds;

		public RouteListProfitabilityController(
			IRouteListProfitabilityFactory routeListProfitabilityFactory,
			INomenclatureParametersProvider nomenclatureParametersProvider,
			IProfitabilityConstantsRepository profitabilityConstantsRepository,
			IRouteListProfitabilityRepository routeListProfitabilityRepository)
		{
			_routeListProfitabilityFactory =
				routeListProfitabilityFactory ?? throw new ArgumentNullException(nameof(routeListProfitabilityFactory));
			_profitabilityConstantsRepository =
				profitabilityConstantsRepository ?? throw new ArgumentNullException(nameof(profitabilityConstantsRepository));
			_routeListProfitabilityRepository =
				routeListProfitabilityRepository ?? throw new ArgumentNullException(nameof(routeListProfitabilityRepository));
			_paidDeliveriesNomenclaturesIds =
				(nomenclatureParametersProvider ?? throw new ArgumentNullException(nameof(nomenclatureParametersProvider)))
				.PaidDeliveriesNomenclaturesIds();
		}

		public void CalculateNewRouteListProfitability(IUnitOfWork uow, RouteList routeList)
		{
			routeList.RouteListProfitability = CreateNewRouteListProfitability(routeList);
			CalculateRouteListProfitability(uow, routeList);
		}

		public void ReCalculateRouteListProfitability(IUnitOfWork uow, RouteList routeList)
		{
			//Для старых МЛ у которых не будет рассчитанных рентабельностей
			if(routeList.RouteListProfitability is null)
			{
				routeList.RouteListProfitability = CreateNewRouteListProfitability(routeList);
			}
			
			CalculateRouteListProfitability(uow, routeList);
		}

		public void RecalculateRouteListProfitabilitiesByCalculatedMonth(IUnitOfWork uow, DateTime date)
		{
			var routeListsWithProfitabilities =
				_routeListProfitabilityRepository.GetAllRouteListsWithProfitabilitiesByCalculatedMonth(uow, date);

			foreach(var routeList in routeListsWithProfitabilities)
			{
				CalculateRouteListProfitability(uow, routeList);
			}
		}
		
		public void RecalculateRouteListProfitabilitiesByDate(IUnitOfWork uow, DateTime date)
		{
			var routeListsWithProfitabilities =
				_routeListProfitabilityRepository.GetAllRouteListsWithProfitabilitiesByDate(uow, date);

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
		
		private RouteListProfitability CreateNewRouteListProfitability(RouteList routeList)
		{
			return _routeListProfitabilityFactory.CreateRouteListProfitability(routeList);
		}
		
		private void CalculateRouteListProfitability(IUnitOfWork uow, RouteList routeList)
		{
			var routeListProfitability = routeList.RouteListProfitability;
			var carVersion = routeList.Car?.CarVersions
				.Where(cv => cv.StartDate <= routeList.Date)
				.SingleOrDefault(cv => cv.EndDate == null || cv.EndDate > routeList.Date);

			CalculateGeneralDataRouteListProfitability(routeList, routeListProfitability);
			
			if(routeList.HasFixedShippingPrice)
			{
				CalculateRouteListProfitabilityWithFixedShippingPrice(routeList, routeListProfitability);
			}
			else if(carVersion != null && carVersion.IsCompanyCar)
			{
				CalculateRouteListProfitabilityForCompanyCar(uow, routeList, routeListProfitability, carVersion);
			}
			else
			{
				CalculateRouteListProfitabilityForNotCompanyCar(routeList, routeListProfitability);
			}
			
			routeListProfitability.RouteListExpensesPerKg = routeListProfitability.TotalGoodsWeight > 0
				? routeListProfitability.RouteListExpenses / routeListProfitability.TotalGoodsWeight
				: default(decimal);
		}

		private void CalculateGeneralDataRouteListProfitability(RouteList routeList, RouteListProfitability routeListProfitability)
		{
			var paidDelivery = GetPaidDeliveriesSumFromRouteList(routeList);

			routeListProfitability.Mileage = GetMileageFromRouteList(routeList);
			routeListProfitability.PaidDelivery = paidDelivery;
			routeListProfitability.TotalGoodsWeight = routeList.GetTotalWeight();
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
			IUnitOfWork uow, RouteList routeList, RouteListProfitability routeListProfitability, CarVersion carVersion)
		{
			CalculateAmortisationAndRepairCosts(uow, routeList, routeListProfitability, carVersion);
			routeListProfitability.FuelCosts = CalculateFuelCosts(routeList.Car, routeListProfitability.Mileage, routeList.Date);
			routeListProfitability.DriverAndForwarderWages = routeList.GetDriversTotalWage() + routeList.GetForwardersTotalWage();
			routeListProfitability.RouteListExpenses =
				routeListProfitability.Amortisation +
				routeListProfitability.RepairCosts +
				routeListProfitability.FuelCosts +
				routeListProfitability.DriverAndForwarderWages - routeListProfitability.PaidDelivery;
		}

		private void CalculateAmortisationAndRepairCosts(
			IUnitOfWork uow, RouteList routeList, RouteListProfitability routeListProfitability, CarVersion carVersion)
		{
			var amortisationPerKm = default(decimal);
			var repairCostsPerKm = default(decimal);
			var nearestProfitabilityConstants = _profitabilityConstantsRepository.GetNearestProfitabilityConstantsByDate(
					uow, new DateTime(routeList.Date.Year, routeList.Date.Month, 1));

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
			routeListProfitability.RouteListExpenses = routeList.FixedShippingPrice - routeListProfitability.PaidDelivery;
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

		private decimal GetPaidDeliveriesSumFromRouteList(RouteList routeList)
		{
			var paidDeliveriesSum = routeList.Addresses
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
