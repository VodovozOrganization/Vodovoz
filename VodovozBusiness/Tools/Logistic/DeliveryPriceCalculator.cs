using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetTopologySuite.Geometries;
using QS.DomainModel.UoW;
using QS.Osm;
using QS.Osm.Osrm;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Sale;
<<<<<<< HEAD
using Vodovoz.Domain.Sectors;
using Vodovoz.EntityRepositories.Sectors;
using Vodovoz.Repositories;
using Vodovoz.Repositories.Sale;
=======
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Sale;
>>>>>>> develop

namespace Vodovoz.Tools.Logistic
{
	public static class DeliveryPriceCalculator
	{
		private static readonly IGeographicGroupRepository _geographicGroupRepository = new GeographicGroupRepository();
		private static readonly IScheduleRestrictionRepository _scheduleRestrictionRepository = new ScheduleRestrictionRepository();
		private static readonly IFuelRepository _fuelRepository = new FuelRepository();
		private static void Calculate() => throw new NotImplementedException();

		static double fuelCost;
		static double distance;
		static DeliveryPoint deliveryPoint;

		public static DeliveryPriceNode Calculate(decimal? latitude, decimal? longitude) => Calculate(latitude, longitude, null);

		public static DeliveryPriceNode Calculate(DeliveryPoint point, int? bottlesCount = null, DateTime? activationTime = null)
		{
			deliveryPoint = point;
			return Calculate(deliveryPoint?.GetActiveVersion(activationTime)?.Latitude, deliveryPoint?.GetActiveVersion(activationTime)?.Longitude, bottlesCount, activationTime);
		}

		public static DeliveryPriceNode Calculate(decimal? latitude, decimal? longitude, int? bottlesCount, DateTime? activationTime = null)
		{
			IList<SectorVersion> sectorVersions;

			DeliveryPriceNode result = new DeliveryPriceNode();

			//Топливо
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Расчет стоимости доставки")) {
				var fuel = _fuelRepository.GetDefaultFuel(uow);
				if(fuel == null) {
					result.ErrorMessage = string.Format("Топливо по умолчанию «АИ-92» не найдено в справочке.");
					return result;
				}
				fuelCost = (double)fuel.Cost;

				//Районы
<<<<<<< HEAD
				sectorVersions = ScheduleRestrictionRepository.GetSectorVersion(uow);
				result.WageDistrict = deliveryPoint?.GetActiveVersion(activationTime)?.Sector?.GetActiveSectorVersion()?.WageSector?.Name ?? "Неизвестно";
=======
				districts = _scheduleRestrictionRepository.GetDistrictsWithBorder(uow);
				result.WageDistrict = deliveryPoint?.District?.WageDistrict?.Name ?? "Неизвестно";
>>>>>>> develop

				//Координаты
				if(!latitude.HasValue || !longitude.HasValue) {
					result.ErrorMessage = string.Format("Не указаны координаты. Невозможно расчитать расстояние.");
					return result;
				}

				//Расчет растояния
				if(deliveryPoint == null) {
<<<<<<< HEAD
					var gg = GeographicGroupRepository.GeographicGroupByCoordinates((double)latitude.Value, (double)longitude.Value, sectorVersions);
=======
					var gg =
						_geographicGroupRepository.GeographicGroupsWithCoordinatesQuery((double)latitude.Value, (double)longitude.Value, districts);
>>>>>>> develop
					var route = new List<PointOnEarth>(2);
					if(gg != null && gg.BaseCoordinatesExist)
						route.Add(new PointOnEarth((double)gg.BaseLatitude, (double)gg.BaseLongitude));
					else if(gg == null)
						//если не найдена часть города, то расстояние считается до его центра
						route.Add(new PointOnEarth(Constants.CenterOfCityLatitude, Constants.CenterOfCityLongitude));
					else {
						result.ErrorMessage = "В подобранной части города не указаны координаты базы";
						return result;
					}
					route.Add(new PointOnEarth(latitude.Value, longitude.Value));
					var osrmResult = OsrmMain.GetRoute(route, false, GeometryOverview.False);
					if(osrmResult == null) {
						result.ErrorMessage = "Ошибка на сервере расчета расстояний, невозможно расчитать расстояние.";
						return result;
					}
					if(osrmResult.Code != "Ok") {
						result.ErrorMessage = $"Сервер расчета расстояний вернул следующее сообщение: {osrmResult.StatusMessageRus}";
						return result;
					}
					distance = osrmResult.Routes[0].TotalDistance / 1000d;
				} else {
					distance = (deliveryPoint?.GetActiveVersion(activationTime)?.DistanceFromBaseMeters ?? 0) / 1000d;
				}
				result.Distance = distance.ToString("N1") + " км";

				result.Prices = Enumerable.Range(1, 100)
					.Select(
						x => new DeliveryPriceRow {
							Amount = x,
							Price = PriceByDistance(x).ToString("C2")
						}
					).ToList();

				//Расчет цены
				var point = new Point((double)latitude, (double)longitude);
				var sectorVersion = sectorVersions.FirstOrDefault(x => x.Polygon.Contains(point));
				result.SectorName = sectorVersion?.SectorName ?? string.Empty;
				result.GeographicGroups = sectorVersion?.GeographicGroup != null ? sectorVersion.GeographicGroup.Name : "Неизвестно";
				result.ByDistance = sectorVersion == null || sectorVersion.PriceType == SectorWaterPrice.ByDistance;
				result.WithPrice = (sectorVersion != null && sectorVersion.PriceType != SectorWaterPrice.ByDistance)
					|| (result.ByDistance && bottlesCount.HasValue);
				if(result.ByDistance) {
					if(bottlesCount.HasValue) {
						result.Price = PriceByDistance(bottlesCount.Value).ToString("C2");
					}
				} else if(sectorVersion?.PriceType == SectorWaterPrice.FixForDistrict)
					result.Price = sectorVersion.WaterPrice.ToString("C2");
				else if(sectorVersion?.PriceType == SectorWaterPrice.Standart)
					result.Price = "прайс";
				result.MinBottles = sectorVersion?.MinBottles.ToString();

				SectorsRepository sectorsRepository = new SectorsRepository();
				var sectorSchedule = sectorsRepository.GetSectorDeliveryRules(uow, sectorVersion?.Sector);
				var sectorScheduleWeekDay = sectorsRepository.GetSectorWeekDayRules(uow, sectorVersion?.Sector);
				
				// result.Schedule = sectorVersion != null && sectorVersion.HaveRestrictions
				// 	? string.Join(", ", sectorVersion.GetSchedulesString(true))
				// 	: "любой день";
			}

			return result;
		}
		//((а * 2/100)*20*б)/в+110
		//а - расстояние от границы города минус
		//б - стоимость литра топлива(есть в справочниках)
		//в - кол-во бут
		static double PriceByDistance(int bootles) => ((distance * 2 / 100) * 20 * fuelCost) / bootles + 125;

		// static string GetSchedulesString(IList<SectorDeliveryRuleVersion> deliveryRuleVersions,
		// 	IList<SectorWeekDayRulesVersion> weekDayRulesVersions)
		// {
		// 	var result = new StringBuilder();
		// 	foreach(var weekDayRules in weekDayRulesVersions)
		// 	{
		// 		weekDayRules.
		// 	}
		// };
		
	}

	public class DeliveryPriceNode
	{
		public string Distance { get; set; }
		public string Price { get; set; }
		public string MinBottles { get; set; }
		public string Schedule { get; set; }
		public List<DeliveryPriceRow> Prices { get; set; }
		public bool ByDistance { get; set; }
		public bool WithPrice { get; set; }
		public string SectorName { get; set; }
		public string GeographicGroups { get; set; }
		public string WageDistrict { get; set; }

		string errorMessage;
		public string ErrorMessage {
			get => errorMessage;
			set {
				ClearValues();
				errorMessage = value;
			}
		}

		public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

		public DeliveryPriceNode()
		{
			ClearValues();
			ErrorMessage = string.Empty;
		}

		public void ClearValues()
		{
			Distance = string.Empty;
			Price = string.Empty;
			MinBottles = string.Empty;
			Schedule = string.Empty;
			SectorName = string.Empty;
			GeographicGroups = string.Empty;
			Prices = new List<DeliveryPriceRow>();
		}
	}

	public class DeliveryPriceRow
	{
		public int Amount { get; set; }
		public string Price { get; set; }
	}
}
