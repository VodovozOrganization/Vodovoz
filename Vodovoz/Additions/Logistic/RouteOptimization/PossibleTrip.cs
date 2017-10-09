﻿using System;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic;
using System.Linq;

namespace Vodovoz.Additions.Logistic.RouteOptimization
{
	public class PossibleTrip
	{
		AtWorkDriver atWorkDriver;

		public DeliveryShift Shift;

		public RouteList OldRoute;

		public Employee Driver{
			get{
				return OldRoute?.Driver ?? atWorkDriver.Employee;
			}
		}

		public Employee Forwarder {
			get {
				if(OldRoute != null)
					return OldRoute.Forwarder;
				else
					return atWorkDriver.WithForwarder?.Employee;
			}
		}

		public Car Car{
			get{
				return OldRoute != null ? OldRoute.Car : atWorkDriver.Car;
			}
		}

		public int DriverPriority{
			get{ // Если маршрут добавлен в ручную, то используем максимальный приоритет, чтобы этому водителю с большей вероятностью достались адреса.
				return OldRoute != null ? 1 : atWorkDriver.PriorityAtDay;
			}
		}

		public TimeSpan? EarlyEnd{
			get{
				return atWorkDriver?.EndOfDay;
			}
		}

		public IDistrictPriority[] Districts { get; private set; }

		public PossibleTrip(AtWorkDriver driver, DeliveryShift shift)
		{
			atWorkDriver = driver;
			Shift = shift;
			Districts = atWorkDriver.Districts.Cast<IDistrictPriority>().ToArray();
		}

		public PossibleTrip(RouteList oldRoute)
		{
			OldRoute = oldRoute;
			Shift = oldRoute.Shift;
			Districts = Driver.Districts.Cast<IDistrictPriority>().ToArray();
		}
	}
}
