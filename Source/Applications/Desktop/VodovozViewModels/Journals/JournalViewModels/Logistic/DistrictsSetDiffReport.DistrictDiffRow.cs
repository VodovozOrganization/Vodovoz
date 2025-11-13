namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public partial class DistrictsSetDiffReport
	{
		public class DistrictDiffRow
		{
			public int Id { get; set; }

			public string Name { get; set; }

			public string GeoGroupOld { get; set; }
			public string GeoGroupNew { get; set; }

			public string TariffZoneNameOld { get; set; }
			public string TariffZoneNameNew { get; set; }

			public int? MinimalBottlesCountOld { get; set; }
			public int? MinimalBottlesCountNew { get; set; }

			public string DelikveryRulesGeneralOld { get; set; }
			public string DelikveryRulesGeneralNew { get; set; }

			public string DeliveryShiftsTodayOld { get; set; }
			public string DeliveryShiftsTodayNew { get; set; }

			public string DeliveryShiftsMondayOld { get; set; }
			public string DeliveryShiftsMondayNew { get; set; }

			public string DeliveryShiftsTuesdayOld { get; set; }
			public string DeliveryShiftsTuesdayNew { get; set; }

			public string DeliveryShiftsWednesdayOld { get; set; }
			public string DeliveryShiftsWednesdayNew { get; set; }

			public string DeliveryShiftsThursdayOld { get; set; }
			public string DeliveryShiftsThursdayNew { get; set; }

			public string DeliveryShiftsFridayOld { get; set; }
			public string DeliveryShiftsFridayNew { get; set; }

			public string DeliveryShiftsSaturdayOld { get; set; }
			public string DeliveryShiftsSaturdayNew { get; set; }

			public string DeliveryShiftsSundayOld { get; set; }
			public string DeliveryShiftsSundayNew { get; set; }

			public bool RegionChanged { get; set; }

			public string DeliveryRulesSpecialOld { get; set; }
			public string DeliveryRulesSpecialNew { get; set; }
		}
	}
}
