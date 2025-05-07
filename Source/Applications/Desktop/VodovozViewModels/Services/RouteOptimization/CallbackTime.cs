using System;
using Google.OrTools.ConstraintSolver;
using Microsoft.Extensions.Logging;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools;

namespace Vodovoz.ViewModels.Services.RouteOptimization
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
		private readonly ILogger<CallbackTime> _logger;
		private CalculatedOrder[] _nodes;
		private PossibleTrip _trip;
		private IExtDistanceCalculator _distanceCalculator;

		public CallbackTime(ILogger<CallbackTime> logger, CalculatedOrder[] nodes, PossibleTrip trip, IExtDistanceCalculator distanceCalculator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_nodes = nodes;
			_trip = trip;
			_distanceCalculator = distanceCalculator;
		}

		public override long Run(int firstIndex, int secondIndex)
		{
			if(firstIndex > _nodes.Length || secondIndex > _nodes.Length || firstIndex < 0 || secondIndex < 0)
			{
				_logger.LogError("Get Time {FirstIndex} -> {SecondIndex} out of orders ({NodesLength})", firstIndex, secondIndex, _nodes.Length);
				return 0;
			}

			if(firstIndex == secondIndex)
			{
				return 0;
			}

			// ^ Смотири описание класса выше ^
			long serviceTime = 0, travelTime = 0;

			if(secondIndex == 0)
			{
				var calcOrder = _nodes[firstIndex - 1];
				var baseVersion = GetGroupVersion(calcOrder.ShippingBase, calcOrder.Order.DeliveryDate.Value);
				travelTime = _distanceCalculator.TimeToBaseSec(
					calcOrder.Order.DeliveryPoint.PointCoordinates,
					baseVersion.PointCoordinates);
			}
			else if(firstIndex == 0)
			{
				var calcOrder = _nodes[secondIndex - 1];
				var baseVersion = GetGroupVersion(calcOrder.ShippingBase, calcOrder.Order.DeliveryDate.Value);
				travelTime = _distanceCalculator.TimeFromBaseSec(
					baseVersion.PointCoordinates,
					calcOrder.Order.DeliveryPoint.PointCoordinates);
			}
			else
			{
				travelTime = _distanceCalculator.TimeSec(
					_nodes[firstIndex - 1].Order.DeliveryPoint.PointCoordinates,
					_nodes[secondIndex - 1].Order.DeliveryPoint.PointCoordinates);
			}

			if(firstIndex != 0)
			{
				serviceTime = _nodes[firstIndex - 1].Order.CalculateTimeOnPoint(_trip.Forwarder != null);
			}

			return (long)_trip.Driver.TimeCorrection(serviceTime + travelTime);
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
