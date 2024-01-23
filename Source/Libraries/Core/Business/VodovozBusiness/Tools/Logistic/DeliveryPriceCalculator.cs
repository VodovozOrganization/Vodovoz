using NetTopologySuite.Geometries;
using QS.Osrm;
using QS.Project.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Factories;
using Vodovoz.Parameters;
using Vodovoz.Services;

namespace Vodovoz.Tools.Logistic
{
	public static class DeliveryPriceCalculator
	{
		private static readonly IGeographicGroupRepository _geographicGroupRepository = new GeographicGroupRepository();
		private static readonly IScheduleRestrictionRepository _scheduleRestrictionRepository = new ScheduleRestrictionRepository();
		private static readonly IFuelRepository _fuelRepository = new FuelRepository();
		private static readonly IGlobalSettings _globalSettings = new GlobalSettings(new ParametersProvider());
		private static void Calculate() => throw new NotImplementedException();

		static double fuelCost;
		static double distance;
		static DeliveryPoint deliveryPoint;

		public static DeliveryPriceNode Calculate(decimal? latitude, decimal? longitude) => Calculate(latitude, longitude, null);

		public static DeliveryPriceNode Calculate(DeliveryPoint point, int? bottlesCount = null)
		{
			deliveryPoint = point;
			return Calculate(deliveryPoint.Latitude, deliveryPoint.Longitude, bottlesCount);
		}

		public static DeliveryPriceNode Calculate(decimal? latitude, decimal? longitude, int? bottlesCount)
		{
			IList<District> districts;

			DeliveryPriceNode result = new DeliveryPriceNode();

			//Топливо
			using(var uow = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot("Расчет стоимости доставки")) {
				var fuel = _fuelRepository.GetDefaultFuel(uow);
				if(fuel == null) {
					result.ErrorMessage = string.Format("Топливо по умолчанию «АИ-92» не найдено в справочке.");
					return result;
				}
				fuelCost = (double)fuel.Cost;

				//Районы
				districts = _scheduleRestrictionRepository.GetDistrictsWithBorder(uow);
				result.WageDistrict = deliveryPoint?.District?.WageDistrict?.Name ?? "Неизвестно";

				//Координаты
				if(!latitude.HasValue || !longitude.HasValue) {
					result.ErrorMessage = string.Format("Не указаны координаты. Невозможно расчитать расстояние.");
					return result;
				}

				//Расчет растояния
				if(deliveryPoint == null) {
					var gg = _geographicGroupRepository.GeographicGroupByCoordinates((double)latitude.Value, (double)longitude.Value, districts);
					var route = new List<PointOnEarth>(2);
					GeoGroupVersion geoGroupVersion = null;
					if(gg != null)
					{
						geoGroupVersion = gg.GetActualVersionOrNull();
					}

					if(gg != null && geoGroupVersion != null && geoGroupVersion.BaseCoordinatesExist)
					{
						route.Add(new PointOnEarth((double)geoGroupVersion.BaseLatitude, (double)geoGroupVersion.BaseLongitude));
					}
					else if(gg == null)
					{
						//если не найдена часть города, то расстояние считается до его центра
						route.Add(new PointOnEarth(Constants.CenterOfCityLatitude, Constants.CenterOfCityLongitude));
					}
					else {
						result.ErrorMessage = "В подобранной части города не указаны координаты базы";
						return result;
					}
					route.Add(new PointOnEarth(latitude.Value, longitude.Value));
					var osrmResult = OsrmClientFactory.Instance.GetRoute(route, false, GeometryOverview.False, _globalSettings.ExcludeToll);
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
					distance = (deliveryPoint.DistanceFromBaseMeters ?? 0) / 1000d;
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
				var district = districts.FirstOrDefault(x => x.DistrictBorder.Contains(point));
				result.DistrictName = district?.DistrictName ?? string.Empty;
				result.GeographicGroups = district?.GeographicGroup != null ? district.GeographicGroup.Name : "Неизвестно";
				result.ByDistance = district == null || district.PriceType == DistrictWaterPrice.ByDistance;
				result.WithPrice = (district != null && district.PriceType != DistrictWaterPrice.ByDistance)
					|| (result.ByDistance && bottlesCount.HasValue);
				if(result.ByDistance) {
					if(bottlesCount.HasValue) {
						result.Price = PriceByDistance(bottlesCount.Value).ToString("C2");
					}
				} else if(district?.PriceType == DistrictWaterPrice.FixForDistrict)
					result.Price = district.WaterPrice.ToString("C2");
				else if(district?.PriceType == DistrictWaterPrice.Standart)
					result.Price = "прайс";
				result.MinBottles = district?.MinBottles.ToString();
				result.Schedule = district != null && district.HaveRestrictions
					? string.Join(", ", district.GetSchedulesString(true))
					: "любой день";
			}

			return result;
		}

		//((а * 2/100)*20*б)/в+110
		//а - расстояние от границы города минус
		//б - стоимость литра топлива(есть в справочниках)
		//в - кол-во бут
		static double PriceByDistance(int bootles) => ((distance * 2 / 100) * 20 * fuelCost) / bootles + 125;
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
		public string DistrictName { get; set; }
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
			DistrictName = string.Empty;
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
