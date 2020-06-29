using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Logistic
{
	public class AtWorkDriver : AtWorkBase
	{
		private Car car;

		[Display(Name = "Автомобиль")]
		public virtual Car Car {
			get => car;
			set => SetField(ref car, value, () => Car);
		}

		private short priorityAtDay;

		[Display(Name = "Приоритет для текущего дня")]
		public virtual short PriorityAtDay {
			get => priorityAtDay;
			set => SetField(ref priorityAtDay, value, () => PriorityAtDay);
		}

		private TimeSpan? endOfDay;

		[Display(Name = "Конец рабочего дня")]
		public virtual TimeSpan? EndOfDay {
			get => endOfDay;
			set => SetField(ref endOfDay, value, () => EndOfDay);
		}

		public virtual string EndOfDayText {
			get => EndOfDay?.ToString("hh\\:mm");
			set {
				if(String.IsNullOrWhiteSpace(value)) {
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
			get => daySchedule;
			set => SetField(ref daySchedule, value, () => DaySchedule);
		}

		private AtWorkForwarder withForwarder;

		[Display(Name = "С экспедитором")]
		public virtual AtWorkForwarder WithForwarder {
			get => withForwarder;
			set => SetField(ref withForwarder, value, () => WithForwarder);
		}

		GeographicGroup geographicGroup;
		[Display(Name = "База")]
		public virtual GeographicGroup GeographicGroup {
			get => geographicGroup;
			set => SetField(ref geographicGroup, value, () => GeographicGroup);
		}

		private IList<AtWorkDriverDistrictPriority> districtsPriorities = new List<AtWorkDriverDistrictPriority>();

		[Display(Name = "Районы")]
		public virtual IList<AtWorkDriverDistrictPriority> DistrictsPriorities {
			get => districtsPriorities;
			set => SetField(ref districtsPriorities, value, () => DistrictsPriorities);
		}

		GenericObservableList<AtWorkDriverDistrictPriority> observableDistrictsPriorities;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<AtWorkDriverDistrictPriority> ObservableDistrictsPriorities {
			get {
				if(observableDistrictsPriorities == null) {
					observableDistrictsPriorities = new GenericObservableList<AtWorkDriverDistrictPriority>(districtsPriorities);
					observableDistrictsPriorities.ElementAdded += ObservableDistrictsPrioritiesElementAdded;
					observableDistrictsPriorities.ElementRemoved += ObservableDistrictsPrioritiesElementRemoved;
				}
				return observableDistrictsPriorities;
			}
		}

		protected AtWorkDriver() { }

		public AtWorkDriver(Employee driver, DateTime date, Car car, DeliveryDaySchedule daySchedule = null)
		{
			Date = date;
			Employee = driver;
			priorityAtDay = driver.TripPriority;
			this.car = car;
			DaySchedule = daySchedule;

			districtsPriorities = new List<AtWorkDriverDistrictPriority>(driver.Districts.Select(x => x.CreateAtDay(this)));
			if(car?.GeographicGroups.Count() == 1)
				this.GeographicGroup = car.GeographicGroups[0];
		}

		#region Функции

		private void CheckDistrictsPriorities()
		{
			for(int i = 0; i < DistrictsPriorities.Count; i++) {
				if(DistrictsPriorities[i] == null) {
					DistrictsPriorities.RemoveAt(i);
					i--;
					continue;
				}

				if(DistrictsPriorities[i].Priority != i)
					DistrictsPriorities[i].Priority = i;
			}
		}

		#endregion

		void ObservableDistrictsPrioritiesElementAdded(object aList, int[] aIdx)
		{
			CheckDistrictsPriorities();
		}

		void ObservableDistrictsPrioritiesElementRemoved(object aList, int[] aIdx, object aObject)
		{
			CheckDistrictsPriorities();
		}
	}
}
