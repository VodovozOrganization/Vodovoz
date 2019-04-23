﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;

namespace Vodovoz.Domain.Logistic
{
	public class AtWorkDriver : AtWorkBase
	{
		private Car car;

		[Display(Name = "Автомобиль")]
		public virtual Car Car
		{
			get { return car; }
			set { SetField(ref car, value, () => Car); }
		}

		private short priorityAtDay;

		[Display(Name = "Приоритет для текущего дня")]
		public virtual short PriorityAtDay
		{
			get { return priorityAtDay; }
			set { SetField(ref priorityAtDay, value, () => PriorityAtDay); }
		}

		private TimeSpan? endOfDay;

		[Display(Name = "Конец рабочего дня")]
		public virtual TimeSpan? EndOfDay
		{
			get { return endOfDay; }
			set { SetField(ref endOfDay, value, () => EndOfDay); }
		}

		public virtual string EndOfDayText{
			get { return EndOfDay?.ToString("hh\\:mm"); }
			set{
				if (String.IsNullOrWhiteSpace(value))
				{
					EndOfDay = null;
					return;
				}
				TimeSpan temp;
				if(TimeSpan.TryParse(value, out temp))
					EndOfDay = temp;
			}
		}

		private DeliveryDaySchedule daySchedule;

		[Display(Name = "График работы")]
		public virtual DeliveryDaySchedule DaySchedule {
			get { return daySchedule; }
			set { SetField(ref daySchedule, value, () => DaySchedule); }
		}

		private AtWorkForwarder withForwarder;

		[Display(Name = "С экспедитором")]
		public virtual AtWorkForwarder WithForwarder {
			get { return withForwarder; }
			set { SetField(ref withForwarder, value, () => WithForwarder); }
		}

		private IList<AtWorkDriverDistrictPriority> districts = new List<AtWorkDriverDistrictPriority>();

		[Display(Name = "Районы")]
		public virtual IList<AtWorkDriverDistrictPriority> Districts
		{
			get { return districts; }
			set { SetField(ref districts, value, () => Districts); }
		}

		GenericObservableList<AtWorkDriverDistrictPriority> observableDistricts;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<AtWorkDriverDistrictPriority> ObservableDistricts
		{
			get
			{
				if (observableDistricts == null)
				{
					observableDistricts = new GenericObservableList<AtWorkDriverDistrictPriority>(districts);
					observableDistricts.ElementAdded += ObservableDistricts_ElementAdded;
					observableDistricts.ElementRemoved += ObservableDistricts_ElementRemoved;
				}
				return observableDistricts;
			}
		}

		protected AtWorkDriver()
		{
		}

		public AtWorkDriver(Employee driver, DateTime date, Car car)
		{
			Date = date;
			Employee = driver;
			priorityAtDay = driver.TripPriority;
			this.car = car;
			daySchedule = driver.DefaultDaySheldule;
			districts = new List<AtWorkDriverDistrictPriority>(driver.Districts.Select(x => x.CreateAtDay(this)));
		}

		#region Функции 
		private void CheckDistrictsPriorities()
		{
			for (int i = 0; i < Districts.Count; i++)
			{
				if (Districts[i] == null)
				{
					Districts.RemoveAt(i);
					i--;
					continue;
				}

				if (Districts[i].Priority != i)
					Districts[i].Priority = i;
			}
		}

		#endregion

		void ObservableDistricts_ElementAdded(object aList, int[] aIdx)
		{
			CheckDistrictsPriorities();
		}

		void ObservableDistricts_ElementRemoved(object aList, int[] aIdx, object aObject)
		{
			CheckDistrictsPriorities();
		}

	}
}
