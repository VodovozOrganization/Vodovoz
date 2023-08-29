using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Logistic.Cars;
using Vodovoz.Domain.Sale;
using Gamma.Utilities;
using Vodovoz.Domain.Orders;

namespace Vodovoz.Domain.Logistic
{
	public class AtWorkDriver : AtWorkBase
	{
		private Car _car;
		private CarVersion _carVersion;

		public enum DriverStatus
		{
			[Display(Name = "Работает")]
			IsWorking,
			[Display(Name = "Снят")]
			NotWorking
		}

		[Display(Name = "Автомобиль")]
		public virtual Car Car {
			get => _car;
			set => SetField(ref _car, value);
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

		private DriverStatus driverStatus;
		[Display(Name = "Статус")]
		public virtual DriverStatus Status {
			get => driverStatus;
			set => SetField(ref driverStatus, value);
		}

		private string reason;
		[Display(Name = "Причина")]
		public virtual string Reason {
			get => reason;
			set => SetField(ref reason, value);
		}
		
		private string comment;
		[Display(Name = "Комментарий")]
		public virtual string Comment {
			get => comment;
			set => SetField(ref comment, value);
		}
		
		private DateTime removedDate;
		[Display(Name = "Время снятия")]
		public virtual DateTime RemovedDate {
			get => removedDate;
			set => SetField(ref removedDate, value);
		}
		
		private DateTime commentLastEditedDate;
		[Display(Name = "Дата последнего изменения комментария")]
		public virtual DateTime CommentLastEditedDate {
			get => commentLastEditedDate;
			set => SetField(ref commentLastEditedDate, value);
		}
		
		private Employee authorRemovedDriver;
		[Display(Name = "Автор снявший водителя")]
		public virtual Employee AuthorRemovedDriver {
			get => authorRemovedDriver;
			set => SetField(ref authorRemovedDriver, value, () => Employee);
		}
		
		private Employee commentLastEditedAuthor;
		[Display(Name = "Автор последнего изменения комментария")]
		public virtual Employee CommentLastEditedAuthor {
			get => commentLastEditedAuthor;
			set => SetField(ref commentLastEditedAuthor, value, () => Employee);
		}

		GeoGroup geographicGroup;
		[Display(Name = "База")]
		public virtual GeoGroup GeographicGroup {
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

		public virtual CarVersion CarVersion => _carVersion ?? (_carVersion = Car?.GetActiveCarVersionOnDate(Date));

		public virtual string CarOwnTypeDisplayName => CarVersion?.CarOwnType.GetEnumTitle() ?? "Не установлена принадлежность авто!";

		public virtual string CarTypeOfUseDisplayName => Car?.CarModel?.CarTypeOfUse.GetEnumTitle() ?? "Не установлен тип авто!";

		protected AtWorkDriver()
		{ }

		public AtWorkDriver(Employee driver, DateTime date, Car car, DeliveryDaySchedule daySchedule = null)
		{
			Date = date;
			Employee = driver;
			priorityAtDay = driver.TripPriority;
			_car = car;
			DaySchedule = daySchedule;

			var activePrioritySet = driver.DriverDistrictPrioritySets.SingleOrDefault(x => x.IsActive);
			if(activePrioritySet != null && activePrioritySet.DriverDistrictPriorities.Any()) {
				districtsPriorities = new List<AtWorkDriverDistrictPriority>(
					activePrioritySet.DriverDistrictPriorities.Select(x => x.CreateAtDay(this))
				);
			}
			if(car?.GeographicGroups.Count() == 1) {
				GeographicGroup = car.GeographicGroups[0];
			}
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
