using QS.DomainModel.Entity;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;

namespace Vodovoz.ViewModels.Logistic
{
	public class DriverWorkScheduleNode : PropertyChangedBase
	{
		public bool AtWork { get; set; }
		public WeekDayName WeekDay { get; set; }
		public DriverWorkSchedule DriverWorkSchedule { get; set; }
		public DeliveryDaySchedule DaySchedule { get; set; }
	}
}
