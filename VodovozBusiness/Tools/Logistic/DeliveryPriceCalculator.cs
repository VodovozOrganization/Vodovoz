using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using QS.DomainModel.UoW;
using QSOsm;
using QSOsm.Osrm;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Sale;
using Vodovoz.Repositories;
using Vodovoz.Repositories.Sale;
using NLog;

namespace Vodovoz.Tools.Logistic
{
	public static class DeliveryPriceCalculator
	{
		private static void Calculate() => throw new NotImplementedException();
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
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
			IList<ScheduleRestrictedDistrict> districts;

			DeliveryPriceNode result = new DeliveryPriceNode();

			//Топливо
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Расчет стоимости доставки")) {
				logger.Debug(40);
				var fuel = FuelRepository.GetDefaultFuel(uow);
				logger.Debug(42);
				if(fuel == null) {
					result.ErrorMessage = string.Format("Топливо по умолчанию «АИ-92» не найдено в справочке.");
					return result;
				}
				fuelCost = (double)fuel.Cost;

				logger.Debug(49);
				//Районы
				districts = ScheduleRestrictionRepository.AreasWithGeometry(uow);

				logger.Debug(53);
				//Координаты
				if(!latitude.HasValue || !longitude.HasValue) {
					result.ErrorMessage = string.Format("Не указаны координаты. Невозможно расчитать расстояние.");
					return result;
				}

				logger.Debug(60);
				//Расчет растояния
				if(deliveryPoint == null) {
					logger.Debug(63);
					var gg = GeographicGroupRepository.GeographicGroupByCoordinates((double)latitude.Value, (double)longitude.Value, districts);
					var route = new List<PointOnEarth>(2);
					logger.Debug(66);
					if(gg != null && gg.BaseCoordinatesExist)
						route.Add(new PointOnEarth((double)gg.BaseLatitude, (double)gg.BaseLongitude));
					else if(gg == null)
						//если не найдена часть города, то расстояние считается до его центра
						route.Add(new PointOnEarth(Constants.CenterOfCityLatitude, Constants.CenterOfCityLongitude));
					else {
						logger.Debug(73);
						result.ErrorMessage = string.Format("В подобранной части города не указаны координаты базы");
						return result;
					}
					logger.Debug(77);
					route.Add(new PointOnEarth(latitude.Value, longitude.Value));
					logger.Debug(79);
					var osrmResult = OsrmMain.GetRoute(route, false, GeometryOverview.False);
					logger.Debug(81);
					if(osrmResult == null) {
						result.ErrorMessage = string.Format("Ошибка на сервере расчета расстояний, невозможно расчитать расстояние.");
						return result;
					}
					logger.Debug(86);
					if(osrmResult.Code != "Ok") {
						result.ErrorMessage = string.Format("Сервер расчета расстояний вернул следующее сообщение: {0}", osrmResult.StatusMessageRus);
						return result;
					}
					logger.Debug(91);
					distance = osrmResult.Routes[0].TotalDistance / 1000d;
				} else {
					distance = (double)(deliveryPoint.DistanceFromBaseMeters ?? 0) / 1000d;
					logger.Debug(95);
				}
				result.Distance = distance.ToString("N1") + " км";

				logger.Debug(99);
				result.Prices = Enumerable.Range(1, 100)
					.Select(
						x => new DeliveryPriceRow {
							Amount = x,
							Price = PriceByDistance(x).ToString("C2")
						}
					).ToList();

				logger.Debug(108);
				//Расчет цены
				var point = new Point((double)latitude, (double)longitude);
				logger.Debug(111);
				var district = districts.FirstOrDefault(x => x.DistrictBorder.Contains(point));
				logger.Debug(113);
				result.DistrictName = district?.DistrictName ?? string.Empty;
				logger.Debug(115);
				result.GeographicGroups = district != null ? string.Join(", ", district.GeographicGroups.Select(w => w.Name)) : "Неизвестно";
				logger.Debug(117);
				result.ByDistance = district == null || district.PriceType == DistrictWaterPrice.ByDistance;
				logger.Debug(119);
				result.WithPrice = (
					(district != null && district.PriceType != DistrictWaterPrice.ByDistance)
					|| (result.ByDistance && bottlesCount.HasValue)
				);
				logger.Debug(124);
				if(result.ByDistance) {
					//((а * 2/100)*20*б)/в+110
					//а - расстояние от границы города минус
					//б - стоимость литра топлива(есть в справочниках)
					//в - кол-во бут
					if(bottlesCount.HasValue) {
						result.Price = PriceByDistance(bottlesCount.Value).ToString("C2");
						logger.Debug(132);
					}
					logger.Debug(134);
				} else if(district.PriceType == DistrictWaterPrice.FixForDistrict)
					result.Price = district.WaterPrice.ToString("C2");
				else if(district.PriceType == DistrictWaterPrice.Standart)
					result.Price = "прайс";
				logger.Debug(139);
				result.MinBottles = district?.MinBottles.ToString();
				logger.Debug(141);
				result.Schedule = district != null && district.HaveRestrictions
					? string.Join(", ", district.GetSchedulesString())
					: "любой день";
			}
			logger.Debug(146);
			return result;
		}

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
