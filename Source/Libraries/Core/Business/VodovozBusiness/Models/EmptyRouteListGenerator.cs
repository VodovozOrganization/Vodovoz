using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;

namespace Vodovoz.Models
{
	public class EmptyRouteListGenerator : IValidatableObject
	{
		private readonly IRouteListRepository _routeListRepository;
		private readonly IEnumerable<AtWorkDriver> _workedDrivers;

		public EmptyRouteListGenerator(IRouteListRepository routeListRepository, IEnumerable<AtWorkDriver> workedDrivers)
		{
			_routeListRepository = routeListRepository ?? throw new ArgumentNullException(nameof(routeListRepository));
			_workedDrivers = workedDrivers ?? throw new ArgumentNullException(nameof(workedDrivers));
		}

		public IList<RouteList> Generate()
		{
			List<RouteList> _routeLists = new List<RouteList>();
			foreach (var workedDriver in _workedDrivers)
			{
				if(workedDriver.DaySchedule == null || !workedDriver.DaySchedule.Shifts.Any())
				{
					throw new InvalidOperationException("Должны быть заполнены графики работы");
				}

				foreach(var shift in workedDriver.DaySchedule.Shifts)
				{
					var hasRouteList = _routeListRepository.HasRouteList(workedDriver.Employee.Id, workedDriver.Date, shift.Id);
					if(hasRouteList)
					{
						continue;
					}

					if(workedDriver.Car == null)
					{
						throw new InvalidOperationException("Должен быть заполнен автомобиль");
					}

					if(workedDriver.GeographicGroup == null)
					{
						throw new InvalidOperationException("Должна быть заполнена часть города (база)");
					}

					var routeList = new RouteList();
					routeList.Driver = workedDriver.Employee;
					routeList.Car = workedDriver.Car;
					routeList.Date = workedDriver.Date;
					routeList.GeographicGroups.Clear();
					routeList.GeographicGroups.Add(workedDriver.GeographicGroup);
					routeList.Shift = shift;
					if(workedDriver.WithForwarder != null)
					{
						routeList.Forwarder = workedDriver.WithForwarder.Employee;
					}
					_routeLists.Add(routeList);
				}
			}
			return _routeLists;
		}

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			foreach(var workedDriver in _workedDrivers)
			{
				foreach(var validationResult in ValidateWorkedDriver(workedDriver))
				{
					yield return validationResult;
				}
			}
		}

		private IEnumerable<ValidationResult> ValidateWorkedDriver(AtWorkDriver workedDriver)
		{
			var driverName = workedDriver.Employee.GetPersonNameWithInitials();

			if(workedDriver.GeographicGroup == null)
			{
				yield return new ValidationResult($"У водителя {driverName} не заполнена часть города (база).", new[] { nameof(workedDriver.GeographicGroup) });
			}

			if(workedDriver.DaySchedule == null || !workedDriver.DaySchedule.Shifts.Any())
			{
				yield return new ValidationResult($"У водителя {driverName} не заполнены графики работы.", new[] { nameof(workedDriver.DaySchedule) });
			}

			if(workedDriver.Car == null)
			{
				yield return new ValidationResult($"У водителя {driverName} не заполнен автомобиль.", new[] { nameof(workedDriver.Car) });
			}
		}
	}
}
