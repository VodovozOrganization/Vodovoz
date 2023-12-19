using Google.OrTools.ConstraintSolver;
using Microsoft.Extensions.Logging;
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
		private readonly ILogger<CallbackDistance> _logger;
		private CalculatedOrder[] _nodes;
		private IDistanceCalculator _distanceCalculator;

		public CallbackDistance(ILogger<CallbackDistance> logger, CalculatedOrder[] nodes, IDistanceCalculator distanceCalculator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_nodes = nodes;
			_distanceCalculator = distanceCalculator;
		}

		public override long Run(int first_index, int second_index)
		{
			if(first_index > _nodes.Length || second_index > _nodes.Length || first_index < 0 || second_index < 0)
			{
				_logger.LogError($"Get Distance {first_index} -> {second_index} out of orders ({_nodes.Length})");
				return 0;
			}

			if(first_index == second_index)
			{
				return 0;
			}

			long distance;

			if(first_index == 0)
			{
				var firstOrder = _nodes[second_index - 1];
				var firstBaseVersion = GetGroupVersion(firstOrder.ShippingBase, firstOrder.Order.DeliveryDate.Value);
				distance = _distanceCalculator.DistanceFromBaseMeter(firstBaseVersion, _nodes[second_index - 1].Order.DeliveryPoint);
			}
			else if(second_index == 0)
			{
				var secondOrder = _nodes[first_index - 1];
				var secondBaseVersion = GetGroupVersion(secondOrder.ShippingBase, secondOrder.Order.DeliveryDate.Value);
				distance = _distanceCalculator.DistanceToBaseMeter(_nodes[first_index - 1].Order.DeliveryPoint, secondBaseVersion);
			}
			else
			{
				distance = _distanceCalculator.DistanceMeter(_nodes[first_index - 1].Order.DeliveryPoint, _nodes[second_index - 1].Order.DeliveryPoint);
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
