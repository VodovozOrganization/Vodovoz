using NetTopologySuite.Geometries;
using QS.DomainModel.UoW;
using QS.Osrm;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain.Sale;
using Vodovoz.EntityRepositories.Delivery;
using Vodovoz.EntityRepositories.Fuel;
using Vodovoz.EntityRepositories.Sale;
using Vodovoz.Settings.Common;

namespace Vodovoz.Tools.Logistic
{
	public class DeliveryPriceCalculator : IDeliveryPriceCalculator
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IGeographicGroupRepository _geographicGroupRepository;
		private readonly IScheduleRestrictionRepository _scheduleRestrictionRepository;
		private readonly IFuelRepository _fuelRepository;
		private readonly IOsrmSettings _globalSettings;
		private readonly IOsrmClient _osrmClient;
		private readonly IDeliveryRepository _deliveryRepository;

		public DeliveryPriceCalculator(
			IUnitOfWorkFactory unitOfWorkFactory,
			IGeographicGroupRepository geographicGroupRepository,
			IScheduleRestrictionRepository scheduleRestrictionRepository,
			IFuelRepository fuelRepository,
			IOsrmSettings globalSettings,
			IOsrmClient osrmClient,
			IDeliveryRepository deliveryRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_geographicGroupRepository = geographicGroupRepository ?? throw new ArgumentNullException(nameof(geographicGroupRepository));
			_scheduleRestrictionRepository = scheduleRestrictionRepository ?? throw new ArgumentNullException(nameof(scheduleRestrictionRepository));
			_fuelRepository = fuelRepository ?? throw new ArgumentNullException(nameof(fuelRepository));
			_globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));
			_osrmClient = osrmClient ?? throw new ArgumentNullException(nameof(osrmClient));
			_deliveryRepository = deliveryRepository ?? throw new ArgumentNullException(nameof(deliveryRepository));
		}


		private double _fuelCost;
		private double _distance;
		private DeliveryPoint _deliveryPoint;
		private OrderAddressType? _orderAddressType;

		private void Calculate() => throw new NotImplementedException();

		public DeliveryPriceNode Calculate(decimal? latitude, decimal? longitude) => Calculate(latitude, longitude, null);

		public DeliveryPriceNode Calculate(DeliveryPoint point, int? bottlesCount = null)
		{
			_deliveryPoint = point;
			return Calculate(_deliveryPoint.Latitude, _deliveryPoint.Longitude, bottlesCount);
		}

		public DeliveryPriceNode Calculate(decimal? latitude, decimal? longitude, int? bottlesCount)
		{
			IList<District> districts;

			var result = new DeliveryPriceNode();

			//Топливо
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Расчет стоимости доставки"))
			{
				var fuel = _fuelRepository.GetDefaultFuel(uow);

				if(fuel == null)
				{
					result.ErrorMessage = string.Format("Топливо по умолчанию «АИ-92» не найдено в справочке.");
					return result;
				}

				_fuelCost = (double)fuel.Cost;

				//Районы
				districts = _scheduleRestrictionRepository.GetDistrictsWithBorder(uow);
				result.WageDistrict = _deliveryPoint?.District?.WageDistrict?.Name ?? "Неизвестно";

				if(_deliveryPoint?.District != null)
				{
					result.DistrictId = _deliveryPoint.District.Id;
				}

				//Координаты
				if(!latitude.HasValue || !longitude.HasValue)
				{
					result.ErrorMessage = string.Format("Не указаны координаты. Невозможно расчитать расстояние.");
					return result;
				}

				//Расчет растояния
				if(_deliveryPoint == null)
				{
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
					else
					{
						result.ErrorMessage = "В подобранной части города не указаны координаты базы";
						return result;
					}

					route.Add(new PointOnEarth(latitude.Value, longitude.Value));
					var osrmResult = _osrmClient.GetRoute(route, false, GeometryOverview.False, _globalSettings.ExcludeToll);

					if(osrmResult == null)
					{
						result.ErrorMessage = "Ошибка на сервере расчета расстояний, невозможно расчитать расстояние.";
						return result;
					}

					if(osrmResult.Code != "Ok")
					{
						result.ErrorMessage = $"Сервер расчета расстояний вернул следующее сообщение: {osrmResult.StatusMessageRus}";
						return result;
					}

					_distance = osrmResult.Routes[0].TotalDistance / 1000d;
				}
				else
				{
					_distance = (_deliveryPoint.DistanceFromBaseMeters ?? 0) / 1000d;
				}
				result.Distance = _distance.ToString("N1") + " км";

				result.Prices = Enumerable
					.Range(1, 100)
					.Select(
						x => new DeliveryPriceRow
						{
							Amount = x,
							Price = PriceByDistance(x).ToString("C2")
						})
					.ToList();

				//Расчет цены
				var point = new Point((double)latitude, (double)longitude);
				var district = districts.FirstOrDefault(x => x.DistrictBorder.Contains(point));

				if(_deliveryPoint?.District == null && district != null)
				{
					result.DistrictId = district.Id;
					result.WageDistrict = district.WageDistrict?.Name ?? "Неизвестно";
				}

				result.DistrictName = district?.DistrictName ?? string.Empty;
				result.GeographicGroups = district?.GeographicGroup != null ? district.GeographicGroup.Name : "Неизвестно";
				result.ByDistance = district == null || district.PriceType == DistrictWaterPrice.ByDistance;
				result.WithPrice = (district != null && district.PriceType != DistrictWaterPrice.ByDistance)
					|| (result.ByDistance && bottlesCount.HasValue);

				if(result.ByDistance)
				{
					if(bottlesCount.HasValue)
					{
						result.Price = PriceByDistance(bottlesCount.Value).ToString("C2");
					}
				}
				else if(district?.PriceType == DistrictWaterPrice.FixForDistrict)
				{
					result.Price = district.WaterPrice.ToString("C2");
				}
				else if(district?.PriceType == DistrictWaterPrice.Standart)
				{
					result.Price = "прайс";
				}

				result.MinBottles = district?.MinBottles.ToString();
			}

			return result;
		}

		public DeliveryPriceNode CalculateForService(DeliveryPoint deliveryPoint)
		{
			var latitude = deliveryPoint.Latitude;
			var longitude = deliveryPoint.Longitude;

			var result = new DeliveryPriceNode();

			using(var uow = _unitOfWorkFactory.CreateWithoutRoot("Расчет стоимости доставки"))
			{
				if(!latitude.HasValue || !longitude.HasValue)
				{
					result.ErrorMessage = string.Format("Не указаны координаты. Невозможно расчитать стоимость сервисной доставки.");
					return result;
				}

				var district = _deliveryRepository.GetServiceDistrictByCoordinates(uow, latitude.Value, longitude.Value);

				if(district == null)
				{
					result.ErrorMessage = string.Format("Не удалось определить район. Невозможно расчитать стоимость сервисной доставки.");

					return result;
				}

				result.ServiceDistrictId = district.Id;
				result.DistrictName = district?.ServiceDistrictName ?? string.Empty;
				result.GeographicGroups = district?.GeographicGroup != null ? district.GeographicGroup.Name : "Неизвестно";
			}

			return result;
		}

		//((а * 2/100)*20*б)/в+110
		//а - расстояние от границы города минус
		//б - стоимость литра топлива(есть в справочниках)
		//в - кол-во бут
		private double PriceByDistance(int bootles) => ((_distance * 2 / 100) * 20 * _fuelCost) / bootles + 125;
	}
}
