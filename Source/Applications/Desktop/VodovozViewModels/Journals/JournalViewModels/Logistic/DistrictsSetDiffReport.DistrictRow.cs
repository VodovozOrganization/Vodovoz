namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public partial class DistrictsSetDiffReport
	{
		public class DistrictRow
		{
			public int Id { get; set; }

			public string Name { get; set; }

			public string TariffZoneName { get; set; }

			public int MinimalBottlesCount { get; set; }

			public string DelikveryRulesGeneral { get; set; }

			public string DeliveryShiftsToday { get; set; }

			public string DeliveryShiftsMonday { get; set; }

			public string DeliveryShiftsTuesday { get; set; }

			public string DeliveryShiftsWednesday { get; set; }

			public string DeliveryShiftsThursday { get; set; }

			public string DeliveryShiftsFriday { get; set; }

			public string DeliveryShiftsSaturday { get; set; }

			public string DeliveryShiftsSunday { get; set; }

			public string DeliveryRulesSpecial { get; set; }
		}
	}
}
