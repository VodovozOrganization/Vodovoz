using System;
using System.Collections.Generic;
using System.Linq;
using Google.OrTools.ConstraintSolver;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Application.Services.Logistics.RouteOptimization
{

	/// <summary>
	/// Класс обратного вызова возвращает расстояния с учетом всевозможных штрафов.
	/// </summary>
	public class CallbackDistanceDistrict : NodeEvaluator2
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
#if DEBUG
		// Чисто для дебага, в резутьтате построения сможем понять, для какого из маршрутов сколько каждого типа событий происходило.
		public static Dictionary<PossibleTrip, int> SGoToBase = new Dictionary<PossibleTrip, int>();
		public static Dictionary<PossibleTrip, int> SFromExistPenality = new Dictionary<PossibleTrip, int>();
		public static Dictionary<PossibleTrip, int> SUnlikeDistrictPenality = new Dictionary<PossibleTrip, int>();
		public static Dictionary<PossibleTrip, int> SLargusPenality = new Dictionary<PossibleTrip, int>();
#endif

		private CalculatedOrder[] Nodes;
		private PossibleTrip Trip;
		private Dictionary<District, int> priorites;
		private IDistanceCalculator distanceCalculator;

		/// <summary>
		/// Этот штраф накладывается на каждый адрес для данного водителя. Потому что водитель имеет меньший приоритет, по стравнению с другими водителями.
		/// </summary>
		private long fixedAddressPenality;

		/// <summary>
		/// Кеш уже рассчитанных значений. Алгоритм в процессе поиска решения, может неоднократно запрашивать одни и те же расстояния.
		/// </summary>
		private long?[,] resultsCache;

		public CallbackDistanceDistrict(CalculatedOrder[] nodes, PossibleTrip trip, IDistanceCalculator distanceCalculator)
		{
			Nodes = nodes;
			Trip = trip;
			priorites = trip.Districts.ToDictionary(x => x.District, x => x.Priority);
			fixedAddressPenality = RouteOptimizer.DriverPriorityAddressPenalty * (Trip.DriverPriority - 1);
			this.distanceCalculator = distanceCalculator;
			resultsCache = new long?[Nodes.Length + 1, Nodes.Length + 1];
#if DEBUG
			SGoToBase[Trip] = 0;
			SFromExistPenality[Trip] = 0;
			SUnlikeDistrictPenality[Trip] = 0;
			SLargusPenality[Trip] = 0;
#endif
		}

		public override long Run(int first_index, int second_index)
		{
			//Возвращаем значение из кеша, иначе считаем.
			if(resultsCache[first_index, second_index] == null)
			{
				resultsCache[first_index, second_index] = Calculate(first_index, second_index);
			}

			return resultsCache[first_index, second_index].Value;
		}

		private long Calculate(int first_index, int second_index)
		{
			if(first_index > Nodes.Length || second_index > Nodes.Length || first_index < 0 || second_index < 0)
			{
				logger.Error($"Get Distance {first_index} -> {second_index} out of orders ({Nodes.Length})");
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
				SGoToBase[Trip]++;
#endif
				var firstOrder = Nodes[first_index - 1];
				var firstBaseVersion = GetGroupVersion(firstOrder.ShippingBase, firstOrder.Order.DeliveryDate.Value);
				return distanceCalculator.DistanceToBaseMeter(Nodes[first_index - 1].Order.DeliveryPoint, firstBaseVersion);
			}

			bool fromExistRoute = false;
			if(Nodes[second_index - 1].ExistRoute != null)
			{
				//Если этот адрес в предварительно заданном маршруте у другого водителя, добавлям расстояние со штрафом.
				if(Trip.OldRoute != Nodes[second_index - 1].ExistRoute)
				{
#if DEBUG
					SFromExistPenality[Trip]++;
#endif
					return RouteOptimizer.RemoveOrderFromExistRLPenalty + GetSimpleDistance(first_index, second_index);
				}
				fromExistRoute = true;
			}

			// малотоннажник
			bool isLightTonnage = Trip.Car.MaxBottles <= 55;

			bool isRightAddress = Nodes[second_index - 1].Bottles >= Trip.Car.MinBottlesFromAddress &&
									Nodes[second_index - 1].Bottles <= Trip.Car.MaxBottlesFromAddress;

			long distance = 0;

			//Если у нас малотоннажник, а адрес большой, то вкатываем оромный штраф.
			if(isLightTonnage && !isRightAddress)
			{
#if DEBUG
				SLargusPenality[Trip]++;
#endif
				return RouteOptimizer.LargusMaxBottlePenalty;
			}

			//Если не малотоннажник и адрес неподходящий, то вкатываем штраф.
			if(!isLightTonnage && !isRightAddress)
			{
				return RouteOptimizer.LargusMaxBottlePenalty;
			}

			var area = Nodes[second_index - 1].District;

			// Если адрес из уже существующего маршрута, не учитываем приоритеты районов.
			// Иначе добавляем штрафы за приоритеты по району.
			if(!fromExistRoute)
			{
				if(priorites.ContainsKey(area))
				{
					distance += priorites[area] * RouteOptimizer.DistrictPriorityPenalty;
				}
				else
				{
#if DEBUG
					SUnlikeDistrictPenality[Trip]++;
#endif
					distance += RouteOptimizer.UnlikeDistrictPenalty;
				}
			}

			bool isAddressFromForeignGeographicGroup = Nodes[second_index - 1].ShippingBase.Id != Trip.GeographicGroup.Id;
			if(isAddressFromForeignGeographicGroup)
			{
				distance += RouteOptimizer.AddressFromForeignGeographicGroupPenalty;
			}

			//Возвращаем расстояние в метрах либо от базы до первого адреса, либо между адресами.
			if(first_index == 0)
			{
				var firstOrder = Nodes[second_index - 1];
				var firstBaseVersion = GetGroupVersion(firstOrder.ShippingBase, firstOrder.Order.DeliveryDate.Value);
				distance += distanceCalculator.DistanceFromBaseMeter(firstBaseVersion, Nodes[second_index - 1].Order.DeliveryPoint);
			}
			else
			{
				distance += distanceCalculator.DistanceMeter(Nodes[first_index - 1].Order.DeliveryPoint, Nodes[second_index - 1].Order.DeliveryPoint);
			}

			return distance + fixedAddressPenality;
		}

		private long GetSimpleDistance(int first_index, int second_index)
		{
			if(first_index == 0)//РАССТОЯНИЯ ПРЯМЫЕ без учета дорожной сети.
			{
				var firstOrder = Nodes[second_index - 1];
				var firstBaseVersion = GetGroupVersion(firstOrder.ShippingBase, firstOrder.Order.DeliveryDate.Value);

				return (long)(DistanceCalculator.GetDistanceFromBase(firstBaseVersion, Nodes[second_index - 1].Order.DeliveryPoint) * 1000);
			}
			return (long)(DistanceCalculator.GetDistance(Nodes[first_index - 1].Order.DeliveryPoint, Nodes[second_index - 1].Order.DeliveryPoint) * 1000);
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
