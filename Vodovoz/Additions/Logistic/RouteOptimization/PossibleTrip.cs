﻿using System;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	/// <summary>
	/// Клас содержит информацию для оптимизации о возможно поездке.
	/// То есть если водитель выше на смену на одну ходку, класс описывает эту ходку.
	/// Если ходки две, должно быть 2 экземпляра этого класса.
	/// </summary>
	public class PossibleTrip
	{
		AtWorkDriver atWorkDriver;

		/// <summary>
		/// Указывается диапазон времени в котором водитель на маршруте.
		/// </summary>
		public DeliveryShift Shift;

		/// <summary>
		/// Ссылка на старый маршрут. Это значит что эта ходна служит для перестройки уже имеющегося маршрута.
		/// Для новых маршрутов это поле не должно быть заполнено.
		/// </summary>
		public RouteList OldRoute;

		public Employee Driver => OldRoute?.Driver ?? atWorkDriver.Employee;

		public Employee Forwarder => OldRoute != null ? OldRoute.Forwarder : atWorkDriver.WithForwarder?.Employee;

		public Car Car => OldRoute != null ? OldRoute.Car : atWorkDriver.Car;

		public GeographicGroup GeographicGroup => OldRoute != null ? OldRoute.GeographicGroups.FirstOrDefault() : atWorkDriver.GeographicGroup;

		/// <summary>
		/// Если маршрут добавлен в ручную, то используем максимальный приоритет, чтобы этому водителю с большей вероятностью достались адреса.
		/// </summary>
		public int DriverPriority => OldRoute != null ? 1 : atWorkDriver.PriorityAtDay;

		/// <summary>
		/// Время раннего завершения работы водителя. Это нужно для случая когда водитель говорит логисту я сегодня хочу
		/// поехать домой пораньше. И работать допустим не до 18 а до 17. Это позволяет не создавать для частных случает укороченных смен.
		/// </summary>
		public TimeSpan? EarlyEnd => atWorkDriver?.EndOfDay;

		/// <summary>
		/// Приоритетные районы для этой ходки.
		/// </summary>
		public IDistrictPriority[] Districts { get; private set; }

		public PossibleTrip(AtWorkDriver driver, DeliveryShift shift)
		{
			atWorkDriver = driver;
			Shift = shift;
			Districts = atWorkDriver.DistrictsPriorities.Cast<IDistrictPriority>().ToArray();
		}
	
		/// <summary>
		/// Конструктор для создания ходки на основе уже имеющегося маршрута. Для его перестройки.
		/// </summary>
		public PossibleTrip(RouteList oldRoute)
		{
			OldRoute = oldRoute;
			Shift = oldRoute.Shift;

			var activeDistrictsSet = Driver.DriverDistrictPrioritySets.SingleOrDefault(x => x.IsActive);
			if(activeDistrictsSet != null && activeDistrictsSet.DriverDistrictPriorities.Any()) {
				Districts = activeDistrictsSet.DriverDistrictPriorities.Cast<IDistrictPriority>().ToArray();
			}
			else
			{
				Districts = new IDistrictPriority[0];
			}
		}
	}
}
