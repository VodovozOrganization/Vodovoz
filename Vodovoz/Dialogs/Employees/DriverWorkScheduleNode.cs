using QS.DomainModel.Entity;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Dialogs.Employees
{
	public class DriverWorkScheduleNode : PropertyChangedBase
	{
		private bool atWork;
		public bool AtWork {
			get => atWork;
			set => SetField(ref atWork, value);
		}

		public WeekDayName WeekDay { get; set; }

		private DriverWorkSchedule drvWorkSchedule;
		public DriverWorkSchedule DrvWorkSchedule {
			get => drvWorkSchedule;
			set => SetField(ref drvWorkSchedule, value);
		}

		private DeliveryDaySchedule daySchedule;
		public DeliveryDaySchedule DaySchedule {
			get => daySchedule;
			set => SetField(ref daySchedule, value);
		}
	}
}
