﻿using System.Collections.Generic;
using System.Linq;
using Google.OrTools.ConstraintSolver;
using Vodovoz.Domain.Logistic;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class CallbackTime : NodeEvaluator2
	{
		static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
		private CalculatedOrder[] Nodes;
		PossibleTrip Trip;
		ExtDistanceCalculator distanceCalculator;

		public CallbackTime(CalculatedOrder[] nodes, PossibleTrip trip, ExtDistanceCalculator distanceCalculator)
		{
			Nodes = nodes;
			Trip = trip;
			this.distanceCalculator = distanceCalculator;
		}

		public override long Run(int first_index, int second_index)
		{
			if(first_index > Nodes.Length || second_index > Nodes.Length || first_index < 0 || second_index < 0)
			{
				logger.Error($"Get Time {first_index} -> {second_index} out of orders ({Nodes.Length})");
				return 0;
			}

			if(first_index == second_index)
				return 0;

			long serviceTime = 0, travelTime = 0;

			if(second_index == 0)
				travelTime = distanceCalculator.TimeToBaseSec(Nodes[first_index - 1].Order.DeliveryPoint);
			else if(first_index == 0)
				travelTime = distanceCalculator.TimeFromBaseSec(Nodes[second_index - 1].Order.DeliveryPoint);
			else
				travelTime = distanceCalculator.TimeSec(Nodes[first_index - 1].Order.DeliveryPoint, Nodes[second_index - 1].Order.DeliveryPoint);

			if (first_index != 0)
				serviceTime = Nodes[first_index - 1].Order.CalculateTimeOnPoint(Trip.Forwarder != null) * 60;

			return (long)Trip.Driver.TimeCorrection(serviceTime + travelTime);
		}
	}
}
