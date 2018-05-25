using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using NetTopologySuite.Geometries;
using QSOrmProject;
using QSOsm;
using QSOsm.Osrm;
using Vodovoz.Domain.Sale;
using Vodovoz.Repositories.Sale;
using Vodovoz.Repository.Logistics;

namespace Vodovoz.Tools.Logistic
{
	public static class DeliveryPriceCalculator
	{
		static double fuelCost;
		static double distance;

		public static DeliveryPriceNode Calculate(decimal? latitude, decimal? longitude)
		{
			return Calculate(latitude, longitude, null);
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
			var uow = UnitOfWorkFactory.CreateWithoutRoot();
			var fuel = FuelTypeRepository.GetDefaultFuel(uow);
			if(fuel == null) {
				result.ErrorMessage = String.Format("Топливо по умолчанию «АИ-92» не найдено в справочке.");
				return result;
			}
			fuelCost = (double)fuel.Cost;

			//Районы
			districts = ScheduleRestrictionRepository.AreaWithGeometry(uow);

			//Координаты
			if(latitude == null || longitude == null) {
				result.ErrorMessage = String.Format("Не указаны координаты. Невозможно расчитать растояние.");
				return result;
			}

			//Расчет растояния
			var route = new List<PointOnEarth>(2);
			route.Add(new PointOnEarth(Constants.BaseLatitude, Constants.BaseLongitude));
			route.Add(new PointOnEarth(latitude.Value, longitude.Value));
			var osrmResult = OsrmMain.GetRoute(route, false, GeometryOverview.False);
			if(osrmResult == null) {
				result.ErrorMessage = String.Format("Ошибка на сервере расчета расстояний, не возможно расчитать растояние.");
				return result;
			}
			if(osrmResult.Code != "Ok") {
				result.ErrorMessage = String.Format("Сервер расчета расстояний вернул следующее сообщение: {0}", osrmResult.StatusMessageRus);
				return result;
			}
			distance = osrmResult.Routes[0].TotalDistance / 1000d;
			result.Prices = Enumerable.Range(1, 100).Select(x => new DeliveryPriceRow {
				Amount = x,
				Price = priceByDistance(x).ToString("C2")
			}).ToList();
			result.Distance = distance.ToString("N1") + " км.";

			//Расчет цены
			var point = new Point((double)latitude, (double)longitude);
			var district = districts.FirstOrDefault(x => x.DistrictBorder.Contains(point));
			result.ByDistance = district == null || district.PriceType == DistrictWaterPrice.ByDistance;
			result.WithPrice = district.PriceType != DistrictWaterPrice.ByDistance || (result.ByDistance && bottlesCount.HasValue);
			if(result.ByDistance) {
				//((а * 2/100)*20*б)/в+110
				//а - расстояние от границы города минус
				//б - стоимость литра топлива(есть в справочниках)
				//в - кол-во бут
				if(bottlesCount.HasValue) {
					result.Price = priceByDistance(bottlesCount.Value).ToString("C2");
				}
			} else if(district.PriceType == DistrictWaterPrice.FixForDistrict)
				result.Price = district.WaterPrice.ToString("C2");
			else if(district.PriceType == DistrictWaterPrice.Standart)
				result.Price = "прайс";
			result.MinBottles = district?.MinBottles.ToString();
			result.Schedule = district != null && district.ScheduleRestrictions.Count > 0
				? String.Join(", ", district.ScheduleRestrictions.Select(x => $"{x.WeekDay.GetEnumTitle()} {x.Schedule?.Name}"))
				: "любой день";
			
			return result;
		}

		static double priceByDistance(int bootles)
		{
			return ((distance * 2 / 100) * 20 * fuelCost) / bootles + 125;
		}
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
		private string errorMessage;
		public string ErrorMessage { 
			get => errorMessage; 
			set {
				ClearValues();
				errorMessage = value;
			}
		}

		public bool HaveError {
			get{
				return !String.IsNullOrEmpty(errorMessage);
			}
		}

		public DeliveryPriceNode()
		{
			ClearValues();
			ErrorMessage = "";
		}

		public void ClearValues()
		{
			Distance = "";
			Price = "";
			MinBottles = "";
			Schedule = "";
			Prices = new List<DeliveryPriceRow>();
		}
	}

	public class DeliveryPriceRow
	{
		public int Amount { get; set; }
		public string Price { get; set; }
	}
}
