using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using QS.DomainModel.UoW;
using QSOsm;
using QSOsm.Osrm;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Sale;
using Vodovoz.Repositories.Sale;
using Vodovoz.Repository.Logistics;

namespace Vodovoz.Tools.Logistic
{
	public static class DeliveryPriceCalculator
	{
		static double fuelCost;
		static double distance;
		static DeliveryPoint deliveryPoint;

		public static DeliveryPriceNode Calculate(decimal? latitude, decimal? longitude)
		{
			return Calculate(latitude, longitude, null);
		}

		public static DeliveryPriceNode Calculate(DeliveryPoint point)
		{
			deliveryPoint = point;
			return Calculate(deliveryPoint.Latitude, deliveryPoint.Longitude, null);
		}

		private static void Calculate()
		{
			throw new NotImplementedException();
		}

		public static DeliveryPriceNode Calculate(decimal? latitude, decimal? longitude, int? bottlesCount)
		{
			IList<ScheduleRestrictedDistrict> districts;

			DeliveryPriceNode result = new DeliveryPriceNode();

			//Топливо
			using(var uow = UnitOfWorkFactory.CreateWithoutRoot("Расчет стоимости доставки")) {
				var fuel = FuelTypeRepository.GetDefaultFuel(uow);
				if(fuel == null) {
					result.ErrorMessage = string.Format("Топливо по умолчанию «АИ-92» не найдено в справочке.");
					return result;
				}
				fuelCost = (double)fuel.Cost;

				//Районы
				districts = ScheduleRestrictionRepository.AreaWithGeometry(uow);

				//Координаты
				if(latitude == null || longitude == null) {
					result.ErrorMessage = string.Format("Не указаны координаты. Невозможно расчитать расстояние.");
					return result;
				}

				//Расчет растояния
				if(deliveryPoint == null) {
					var route = new List<PointOnEarth>(2);
					route.Add(new PointOnEarth(Constants.BaseLatitude, Constants.BaseLongitude));
					route.Add(new PointOnEarth(latitude.Value, longitude.Value));
					var osrmResult = OsrmMain.GetRoute(route, false, GeometryOverview.False);
					if(osrmResult == null) {
						result.ErrorMessage = string.Format("Ошибка на сервере расчета расстояний, невозможно расчитать расстояние.");
						return result;
					}
					if(osrmResult.Code != "Ok") {
						result.ErrorMessage = string.Format("Сервер расчета расстояний вернул следующее сообщение: {0}", osrmResult.StatusMessageRus);
						return result;
					}
					distance = osrmResult.Routes[0].TotalDistance / 1000d;
				} else {
					distance = (double)(deliveryPoint.DistanceFromBaseMeters ?? 0) / 1000d;
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
				result.WarehousesList = district != null ? string.Join(", ", district.GeographicGroups.Select(w => w.Name)) : "Неизвестно";
				result.ByDistance = district == null || district.PriceType == DistrictWaterPrice.ByDistance;
				result.WithPrice = (
					(district != null && district.PriceType != DistrictWaterPrice.ByDistance)
					|| (result.ByDistance && bottlesCount.HasValue)
				);
				if(result.ByDistance) {
					//((а * 2/100)*20*б)/в+110
					//а - расстояние от границы города минус
					//б - стоимость литра топлива(есть в справочниках)
					//в - кол-во бут
					if(bottlesCount.HasValue) {
						result.Price = PriceByDistance(bottlesCount.Value).ToString("C2");
					}
				} else if(district.PriceType == DistrictWaterPrice.FixForDistrict)
					result.Price = district.WaterPrice.ToString("C2");
				else if(district.PriceType == DistrictWaterPrice.Standart)
					result.Price = "прайс";
				result.MinBottles = district?.MinBottles.ToString();
				result.Schedule = district != null && district.HaveRestrictions
					? string.Join(", ", district.GetSchedulesString())
					: "любой день";
			}
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
		public string WarehousesList { get; set; }

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
			WarehousesList = string.Empty;
			Prices = new List<DeliveryPriceRow>();
		}
	}

	public class DeliveryPriceRow
	{
		public int Amount { get; set; }
		public string Price { get; set; }
	}
}
