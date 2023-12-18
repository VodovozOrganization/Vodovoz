using Google.OrTools.ConstraintSolver;
using System;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Application.Services.Logistics.RouteOptimization
{

	/// <summary>
	/// Класс обратного вызова, для рассчета расстояние без штрафов используется в построении одного маршрута.
	/// </summary>
	public class CallbackDistance : NodeEvaluator2
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private CalculatedOrder[] Nodes;
		private IDistanceCalculator distanceCalculator;

		public CallbackDistance(CalculatedOrder[] nodes, IDistanceCalculator distanceCalculator)
		{
			Nodes = nodes;
			this.distanceCalculator = distanceCalculator;
		}

		public override long Run(int first_index, int second_index)
		{
			if(first_index > Nodes.Length || second_index > Nodes.Length || first_index < 0 || second_index < 0)
			{
				logger.Error($"Get Distance {first_index} -> {second_index} out of orders ({Nodes.Length})");
				return 0;
			}

			if(first_index == second_index)
			{
				return 0;
			}

			long distance;

			if(first_index == 0)
			{
				var firstOrder = Nodes[second_index - 1];
				var firstBaseVersion = GetGroupVersion(firstOrder.ShippingBase, firstOrder.Order.DeliveryDate.Value);
				distance = distanceCalculator.DistanceFromBaseMeter(firstBaseVersion, Nodes[second_index - 1].Order.DeliveryPoint);
			}
			else if(second_index == 0)
			{
				var secondOrder = Nodes[first_index - 1];
				var secondBaseVersion = GetGroupVersion(secondOrder.ShippingBase, secondOrder.Order.DeliveryDate.Value);
				distance = distanceCalculator.DistanceToBaseMeter(Nodes[first_index - 1].Order.DeliveryPoint, secondBaseVersion);
			}
			else
			{
				distance = distanceCalculator.DistanceMeter(Nodes[first_index - 1].Order.DeliveryPoint, Nodes[second_index - 1].Order.DeliveryPoint);
			}

			return distance;
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
