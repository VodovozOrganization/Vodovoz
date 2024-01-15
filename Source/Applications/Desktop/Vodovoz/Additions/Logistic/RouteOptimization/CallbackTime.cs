﻿using Google.OrTools.ConstraintSolver;
using System;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools;
using Vodovoz.Tools.Logistic;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	/// <summary>
	/// Класс обратного вызова для расчета времени движения по маршруту.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>serviceTime</c> - время затраченое на адресе. <c>travelTime</c> - время затраченое на путь к адресу.
	/// </para>
	/// <para>
	/// *Обратите внимание* при запросе пути от точки А к точке Б. Класс возвращает время в пути
	/// от точки А к точке Б и время затраченое на разгрузку в точке А. То есть не очень интуитивно.
	/// Сделано это потому что в измерении, учитывающем время, для точек хранится диапазон от времени приезда на адрес
	/// до максимального времени минус стандартное время разгрузки. Данная схема позволяет в измерении указывать точно
	/// разрешенное время приезда. Получается что мы берем время приезда на предыдущую точку (значение в измерении),
	/// к нему прибавляем время разгрузки на предыдущей точке <c>serviceTime</c> и прибавляем путь от предыдущей
	/// точке к следующей(текущей). Получаем время приезда на текущую точку и можем сравнивать с разрешенным временем
	/// приезда хранимом для точек в измерении.
	/// </para>
	/// <para> Возвращаемое время обрабатывается с помощью <see cref="Domain.Employees.Employee.TimeCorrection(long)"/>,
	/// для корректировки на скорости водителя.
	/// </para>
	/// </remarks>
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

			// ^ Смотири описание класса выше ^
			long serviceTime = 0, travelTime = 0;

			if(second_index == 0)
			{
				var calcOrder = Nodes[first_index - 1];
				var baseVersion = GetGroupVersion(calcOrder.ShippingBase, calcOrder.Order.DeliveryDate.Value);
				travelTime = distanceCalculator.TimeToBaseSec(
					calcOrder.Order.DeliveryPoint.PointCoordinates,
					baseVersion.PointCoordinates);
			}
			else if(first_index == 0)
			{
				var calcOrder = Nodes[second_index - 1];
				var baseVersion = GetGroupVersion(calcOrder.ShippingBase, calcOrder.Order.DeliveryDate.Value);
				travelTime = distanceCalculator.TimeFromBaseSec(
					baseVersion.PointCoordinates,
					calcOrder.Order.DeliveryPoint.PointCoordinates);
			}
			else
			{
				travelTime = distanceCalculator.TimeSec(
					Nodes[first_index - 1].Order.DeliveryPoint.PointCoordinates,
					Nodes[second_index - 1].Order.DeliveryPoint.PointCoordinates);
			}

			if(first_index != 0)
			{
				serviceTime = Nodes[first_index - 1].Order.CalculateTimeOnPoint(Trip.Forwarder != null);
			}

			return (long)Trip.Driver.TimeCorrection(serviceTime + travelTime);
		}

		private GeoGroupVersion GetGroupVersion(GeoGroup geoGroup, DateTime date)
		{
			var version = geoGroup.GetVersionOrNull(date);
			if(version == null)
			{
				throw new GeoGroupVersionNotFoundException($"Невозможно рассчитать время, так как на {date} у части города ({geoGroup.Name}) нет актуальных данных."); ;
			}

			return version;
		}
	}
}
