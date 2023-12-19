using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.ConstraintSolver;
using Microsoft.Extensions.Logging;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Application.Logistics.RouteOptimization
{

	/// <summary>
	/// Класс обратного вызова возвращает расстояния с учетом всевозможных штрафов.
	/// </summary>
	public class CallbackDistanceDistrict : NodeEvaluator2
	{
		private readonly ILogger<CallbackDistanceDistrict> _logger;
#if DEBUG
		// Чисто для дебага, в резутьтате построения сможем понять, для какого из маршрутов сколько каждого типа событий происходило.
		public static Dictionary<PossibleTrip, int> SGoToBase = new Dictionary<PossibleTrip, int>();
		public static Dictionary<PossibleTrip, int> SFromExistPenality = new Dictionary<PossibleTrip, int>();
		public static Dictionary<PossibleTrip, int> SUnlikeDistrictPenality = new Dictionary<PossibleTrip, int>();
		public static Dictionary<PossibleTrip, int> SLargusPenality = new Dictionary<PossibleTrip, int>();
#endif

		private CalculatedOrder[] _nodes;
		private PossibleTrip _trip;
		private Dictionary<District, int> _priorites;
		private IDistanceCalculator _distanceCalculator;

		/// <summary>
		/// Этот штраф накладывается на каждый адрес для данного водителя. Потому что водитель имеет меньший приоритет, по стравнению с другими водителями.
		/// </summary>
		private long _fixedAddressPenality;

		/// <summary>
		/// Кеш уже рассчитанных значений. Алгоритм в процессе поиска решения, может неоднократно запрашивать одни и те же расстояния.
		/// </summary>
		private long?[,] _resultsCache;

		public CallbackDistanceDistrict(ILogger<CallbackDistanceDistrict> logger, CalculatedOrder[] nodes, PossibleTrip trip, IDistanceCalculator distanceCalculator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_nodes = nodes;
			_trip = trip;
			_priorites = trip.Districts.ToDictionary(x => x.District, x => x.Priority);
			_fixedAddressPenality = RouteOptimizer.DriverPriorityAddressPenalty * (_trip.DriverPriority - 1);
			_distanceCalculator = distanceCalculator;
			_resultsCache = new long?[_nodes.Length + 1, _nodes.Length + 1];
#if DEBUG
			SGoToBase[_trip] = 0;
			SFromExistPenality[_trip] = 0;
			SUnlikeDistrictPenality[_trip] = 0;
			SLargusPenality[_trip] = 0;
#endif
		}

		public override long Run(int first_index, int second_index)
		{
			//Возвращаем значение из кеша, иначе считаем.
			if(_resultsCache[first_index, second_index] == null)
			{
				_resultsCache[first_index, second_index] = Calculate(first_index, second_index);
			}

			return _resultsCache[first_index, second_index].Value;
		}

		private long Calculate(int first_index, int second_index)
		{
			if(first_index > _nodes.Length || second_index > _nodes.Length || first_index < 0 || second_index < 0)
			{
				_logger.LogError($"Get Distance {first_index} -> {second_index} out of orders ({_nodes.Length})");
				return 0;
			}

			//Возвращаем 0 при запросе расстояния из одной и тоже же точки, в нее же.
			//Такой запрос приходит обычно на точку склада, когда мы считаем больше 1 маршрута. 
			if(first_index == second_index)
			{
				return 0;
			}

			//Просто возвращаем расстояние до базы от точки на которой находимся.
			if(second_index == 0)
			{
#if DEBUG
				SGoToBase[_trip]++;
#endif
				var firstOrder = _nodes[first_index - 1];
				var firstBaseVersion = GetGroupVersion(firstOrder.ShippingBase, firstOrder.Order.DeliveryDate.Value);
				return _distanceCalculator.DistanceToBaseMeter(_nodes[first_index - 1].Order.DeliveryPoint, firstBaseVersion);
			}

			bool fromExistRoute = false;
			if(_nodes[second_index - 1].ExistRoute != null)
			{
				//Если этот адрес в предварительно заданном маршруте у другого водителя, добавлям расстояние со штрафом.
				if(_trip.OldRoute != _nodes[second_index - 1].ExistRoute)
				{
#if DEBUG
					SFromExistPenality[_trip]++;
#endif
					return RouteOptimizer.RemoveOrderFromExistRLPenalty + GetSimpleDistance(first_index, second_index);
				}
				fromExistRoute = true;
			}

			// малотоннажник
			bool isLightTonnage = _trip.Car.MaxBottles <= 55;

			bool isRightAddress = _nodes[second_index - 1].Bottles >= _trip.Car.MinBottlesFromAddress &&
									_nodes[second_index - 1].Bottles <= _trip.Car.MaxBottlesFromAddress;

			long distance = 0;

			//Если у нас малотоннажник, а адрес большой, то вкатываем оромный штраф.
			if(isLightTonnage && !isRightAddress)
			{
#if DEBUG
				SLargusPenality[_trip]++;
#endif
				return RouteOptimizer.LargusMaxBottlePenalty;
			}

			//Если не малотоннажник и адрес неподходящий, то вкатываем штраф.
			if(!isLightTonnage && !isRightAddress)
			{
				return RouteOptimizer.LargusMaxBottlePenalty;
			}

			var area = _nodes[second_index - 1].District;

			// Если адрес из уже существующего маршрута, не учитываем приоритеты районов.
			// Иначе добавляем штрафы за приоритеты по району.
			if(!fromExistRoute)
			{
				if(_priorites.ContainsKey(area))
				{
					distance += _priorites[area] * RouteOptimizer.DistrictPriorityPenalty;
				}
				else
				{
#if DEBUG
					SUnlikeDistrictPenality[_trip]++;
#endif
					distance += RouteOptimizer.UnlikeDistrictPenalty;
				}
			}

			bool isAddressFromForeignGeographicGroup = _nodes[second_index - 1].ShippingBase.Id != _trip.GeographicGroup.Id;
			if(isAddressFromForeignGeographicGroup)
			{
				distance += RouteOptimizer.AddressFromForeignGeographicGroupPenalty;
			}

			//Возвращаем расстояние в метрах либо от базы до первого адреса, либо между адресами.
			if(first_index == 0)
			{
				var firstOrder = _nodes[second_index - 1];
				var firstBaseVersion = GetGroupVersion(firstOrder.ShippingBase, firstOrder.Order.DeliveryDate.Value);
				distance += _distanceCalculator.DistanceFromBaseMeter(firstBaseVersion, _nodes[second_index - 1].Order.DeliveryPoint);
			}
			else
			{
				distance += _distanceCalculator.DistanceMeter(_nodes[first_index - 1].Order.DeliveryPoint, _nodes[second_index - 1].Order.DeliveryPoint);
			}

			return distance + _fixedAddressPenality;
		}

		private long GetSimpleDistance(int first_index, int second_index)
		{
			if(first_index == 0)//РАССТОЯНИЯ ПРЯМЫЕ без учета дорожной сети.
			{
				var firstOrder = _nodes[second_index - 1];
				var firstBaseVersion = GetGroupVersion(firstOrder.ShippingBase, firstOrder.Order.DeliveryDate.Value);

				return (long)(DistanceCalculator.GetDistanceFromBase(firstBaseVersion, _nodes[second_index - 1].Order.DeliveryPoint) * 1000);
			}
			return (long)(DistanceCalculator.GetDistance(_nodes[first_index - 1].Order.DeliveryPoint, _nodes[second_index - 1].Order.DeliveryPoint) * 1000);
		}

		private GeoGroupVersion GetGroupVersion(GeoGroup geoGroup, DateTime date)
		{
			var version = geoGroup.GetVersionOrNull(date);
			if(version == null)
			{
				throw new GeoGroupVersionNotFoundException($"Невозможно рассчитать расстояние, так как на {date} у части города ({geoGroup.Name}) нет актуальных данных."); ;
			}

			return version;
		}
	}
}
