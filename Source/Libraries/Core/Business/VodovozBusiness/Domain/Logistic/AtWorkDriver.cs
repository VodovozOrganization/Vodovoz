using Gamma.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain.Logistic
{
	public class AtWorkDriver : AtWorkBase
	{
		private Car _car;
		private CarVersion _carVersion;
		private short _priorityAtDay;
		private TimeSpan? _endOfDay;
		private DeliveryDaySchedule _daySchedule;
		private AtWorkForwarder _withForwarder;
		private DriverStatus _driverStatus;
		private string _reason;
		private string _comment;
		private DateTime _removedDate;
		private DateTime _commentLastEditedDate;
		private Employee _authorRemovedDriver;
		private Employee _commentLastEditedAuthor;
		GeoGroup _geographicGroup;
		private IList<AtWorkDriverDistrictPriority> _districtsPriorities = new List<AtWorkDriverDistrictPriority>();
		GenericObservableList<AtWorkDriverDistrictPriority> _observableDistrictsPriorities;

		protected AtWorkDriver()
		{ }

		public AtWorkDriver(Employee driver, DateTime date, Car car, DeliveryDaySchedule daySchedule = null)
		{
			Id = driver.Id;
			Date = date;
			Employee = driver;
			PriorityAtDay = driver.TripPriority;
			Car = car;
			DaySchedule = daySchedule;

			var activePrioritySet = driver.DriverDistrictPrioritySets.SingleOrDefault(x => x.IsActive);

			if(activePrioritySet != null && activePrioritySet.DriverDistrictPriorities.Any())
			{
				_districtsPriorities = new List<AtWorkDriverDistrictPriority>(
					activePrioritySet.DriverDistrictPriorities.Select(x => x.CreateAtDay(this)));
			}

			if(car?.GeographicGroups.Count() == 1)
			{
				GeographicGroup = car.GeographicGroups[0];
			}
		}

		public enum DriverStatus
		{
			[Display(Name = "Работает")]
			IsWorking,
			[Display(Name = "Снят")]
			NotWorking
		}

		[Display(Name = "Автомобиль")]
		public virtual Car Car
		{
			get => _car;
			set => SetField(ref _car, value);
		}

		[Display(Name = "Приоритет для текущего дня")]
		public virtual short PriorityAtDay
		{
			get => _priorityAtDay;
			set => SetField(ref _priorityAtDay, value);
		}


		[Display(Name = "Конец рабочего дня")]
		public virtual TimeSpan? EndOfDay
		{
			get => _endOfDay;
			set => SetField(ref _endOfDay, value);
		}

		public virtual string EndOfDayText
		{
			get => EndOfDay?.ToString("hh\\:mm");
			set
			{
				if(string.IsNullOrWhiteSpace(value))
				{
					EndOfDay = null;
					return;
				}

				if(TimeSpan.TryParse(value, out TimeSpan temp))
				{
					EndOfDay = temp;
				}
			}
		}

		[Display(Name = "График работы")]
		public virtual DeliveryDaySchedule DaySchedule
		{
			get => _daySchedule;
			set => SetField(ref _daySchedule, value);
		}

		[Display(Name = "С экспедитором")]
		public virtual AtWorkForwarder WithForwarder
		{
			get => _withForwarder;
			set => SetField(ref _withForwarder, value);
		}

		[Display(Name = "Статус")]
		public virtual DriverStatus Status
		{
			get => _driverStatus;
			set => SetField(ref _driverStatus, value);
		}

		[Display(Name = "Причина")]
		public virtual string Reason
		{
			get => _reason;
			set => SetField(ref _reason, value);
		}

		[Display(Name = "Комментарий")]
		public virtual string Comment
		{
			get => _comment;
			set => SetField(ref _comment, value);
		}

		[Display(Name = "Время снятия")]
		public virtual DateTime RemovedDate
		{
			get => _removedDate;
			set => SetField(ref _removedDate, value);
		}

		[Display(Name = "Дата последнего изменения комментария")]
		public virtual DateTime CommentLastEditedDate
		{
			get => _commentLastEditedDate;
			set => SetField(ref _commentLastEditedDate, value);
		}

		[Display(Name = "Автор снявший водителя")]
		public virtual Employee AuthorRemovedDriver
		{
			get => _authorRemovedDriver;
			set => SetField(ref _authorRemovedDriver, value);
		}

		[Display(Name = "Автор последнего изменения комментария")]
		public virtual Employee CommentLastEditedAuthor
		{
			get => _commentLastEditedAuthor;
			set => SetField(ref _commentLastEditedAuthor, value);
		}

		[Display(Name = "База")]
		public virtual GeoGroup GeographicGroup
		{
			get => _geographicGroup;
			set => SetField(ref _geographicGroup, value);
		}

		[Display(Name = "Районы")]
		public virtual IList<AtWorkDriverDistrictPriority> DistrictsPriorities
		{
			get => _districtsPriorities;
			set => SetField(ref _districtsPriorities, value);
		}

		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<AtWorkDriverDistrictPriority> ObservableDistrictsPriorities
		{
			get
			{
				if(_observableDistrictsPriorities == null)
				{
					_observableDistrictsPriorities = new GenericObservableList<AtWorkDriverDistrictPriority>(_districtsPriorities);
					_observableDistrictsPriorities.ElementAdded += ObservableDistrictsPrioritiesElementAdded;
					_observableDistrictsPriorities.ElementRemoved += ObservableDistrictsPrioritiesElementRemoved;
				}
				return _observableDistrictsPriorities;
			}
		}

		public virtual CarVersion CarVersion => _carVersion ?? (_carVersion = Car?.GetActiveCarVersionOnDate(Date));

		public virtual string CarOwnTypeDisplayName => CarVersion?.CarOwnType.GetEnumTitle() ?? "Не установлена принадлежность авто!";

		public virtual string CarTypeOfUseDisplayName => Car?.CarModel?.CarTypeOfUse.GetEnumTitle() ?? "Не установлен тип авто!";

		#region Функции

		private void CheckDistrictsPriorities()
		{
			for(int i = 0; i < DistrictsPriorities.Count; i++)
			{
				if(DistrictsPriorities[i] == null)
				{
					DistrictsPriorities.RemoveAt(i);
					i--;
					continue;
				}

				if(DistrictsPriorities[i].Priority != i)
				{
					DistrictsPriorities[i].Priority = i;
				}
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
