using System;
using Google.OrTools.ConstraintSolver;
using Microsoft.Extensions.Logging;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.ViewModels.Services.RouteOptimization
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

		public override long Run(int firstIndex, int secondIndex)
		{
			if(firstIndex > _nodes.Length || secondIndex > _nodes.Length || firstIndex < 0 || secondIndex < 0)
			{
				_logger.LogError("Get Distance {FirstIndex} -> {SecondIndex} out of orders ({NodesLength})", firstIndex, secondIndex, _nodes.Length);
				return 0;
			}

			if(firstIndex == secondIndex)
			{
				return 0;
			}

			long distance;

			if(firstIndex == 0)
			{
				var firstOrder = _nodes[secondIndex - 1];
				var firstBaseVersion = GetGroupVersion(firstOrder.ShippingBase, firstOrder.Order.DeliveryDate.Value);
				distance = _distanceCalculator.DistanceFromBaseMeter(
					firstBaseVersion.PointCoordinates,
					_nodes[secondIndex - 1].Order.DeliveryPoint.PointCoordinates);
			}
			else if(secondIndex == 0)
			{
				var secondOrder = _nodes[firstIndex - 1];
				var secondBaseVersion = GetGroupVersion(secondOrder.ShippingBase, secondOrder.Order.DeliveryDate.Value);
				distance = _distanceCalculator.DistanceToBaseMeter(
					_nodes[firstIndex - 1].Order.DeliveryPoint.PointCoordinates,
					secondBaseVersion.PointCoordinates);
			}
			else
			{
				distance = _distanceCalculator.DistanceMeter(
					_nodes[firstIndex - 1].Order.DeliveryPoint.PointCoordinates,
					_nodes[secondIndex - 1].Order.DeliveryPoint.PointCoordinates);
			}

			return distance;
		}

		private GeoGroupVersion GetGroupVersion(GeoGroup geoGroup, DateTime date)
		{
			var version = geoGroup.GetVersionOrNull(date);
			if(version == null)
			{
				throw new GeoGroupVersionNotFoundException($"Невозможно рассчитать расстояние, так как на {date} у части города ({geoGroup.Name}) нет актуальных данных.");
			}

			return version;
		}
	}
}
