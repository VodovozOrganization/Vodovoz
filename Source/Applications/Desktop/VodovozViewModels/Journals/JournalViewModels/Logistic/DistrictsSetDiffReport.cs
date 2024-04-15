using NHibernate.Linq;
using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using Vodovoz.Domain.Logistic;
using Vodovoz.Errors;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	public partial class DistrictsSetDiffReport
	{
		private DistrictsSetDiffReport(
			IEnumerable<DistrictDiffRow> districtChanged,
			IEnumerable<DistrictRow> districtAdded,
			IEnumerable<DistrictRow> districtRemoved)
		{
			DistrictDiffs = districtChanged;
			DistrictAdded = districtAdded;
			DistrictRemoved = districtRemoved;
		}

		public IEnumerable<DistrictDiffRow> DistrictDiffs { get; set; }
		public IEnumerable<DistrictRow> DistrictAdded { get; set; }
		public IEnumerable<DistrictRow> DistrictRemoved { get; set; }

		public static Result<DistrictsSetDiffReport> Generate(IUnitOfWork unitOfWork, int? diffSourceDistrictSetVersionId, int? diffTargetDistrictSetVersionId)
		{
			var sourceDistrictSet =
				(from districtSet in unitOfWork.Session.Query<DistrictsSet>()
				 where districtSet.Id == diffSourceDistrictSetVersionId.Value
				 select districtSet)
				.FetchMany(ds => ds.Districts)
				.ThenFetchMany(d => d.AllDeliveryScheduleRestrictions)
				.FetchMany(ds => ds.Districts)
				.ThenFetchMany(d => d.AllDistrictRuleItems)
				.FetchMany(ds => ds.Districts)
				.ThenFetch(d => d.TariffZone)
				.FirstOrDefault();

			if(sourceDistrictSet is null)
			{
				return Result.Failure<DistrictsSetDiffReport>(Errors.Logistics.DistrictSet.NotFound(diffSourceDistrictSetVersionId.Value));
			}

			var targetDistrictSet =
				(from districtSet in unitOfWork.Session.Query<DistrictsSet>()
				 where districtSet.Id == diffTargetDistrictSetVersionId.Value
				 select districtSet)
				.FetchMany(ds => ds.Districts)
				.ThenFetchMany(d => d.AllDeliveryScheduleRestrictions)
				.FetchMany(ds => ds.Districts)
				.ThenFetchMany(d => d.AllDistrictRuleItems)
				.FetchMany(ds => ds.Districts)
				.ThenFetch(d => d.TariffZone)
				.FirstOrDefault();

			if(targetDistrictSet is null)
			{
				return Result.Failure<DistrictsSetDiffReport>(Errors.Logistics.DistrictSet.NotFound(diffTargetDistrictSetVersionId.Value));
			}

			var oldDistricts = sourceDistrictSet.Districts;
			var newDistricts = targetDistrictSet.Districts;
			
			var districtsChanged = new List<DistrictDiffRow>();
			var districtsAdded = new List<DistrictRow>();
			var districtsRemoved = new List<DistrictRow>();

			foreach(var district in newDistricts)
			{
				if(oldDistricts.Contains(district.CopyOf))
				{
					districtsChanged.Add(new DistrictDiffRow
					{
						Id = district.Id,
						Name = district.DistrictName,

						TariffZoneNameOld = district.CopyOf.TariffZone.Name,
						TariffZoneNameNew = district.TariffZone.Name,
						MinimalBottlesCountOld = district.CopyOf.MinBottles,
						MinimalBottlesCountNew = district.MinBottles,

						DelikveryRulesGeneralOld = string.Join("\n", district.CopyOf.CommonDistrictRuleItems.Select(cdri => cdri.Title)),
						DelikveryRulesGeneralNew = string.Join("\n", district.CommonDistrictRuleItems.Select(cdri => cdri.Title)),

						DeliveryShiftsTodayOld = string.Join("\n", district.CopyOf.TodayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsTodayNew = string.Join("\n", district.TodayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsMondayOld = string.Join("\n", district.CopyOf.MondayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsMondayNew = string.Join("\n", district.MondayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsTuesdayOld = string.Join("\n", district.CopyOf.TuesdayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsTuesdayNew = string.Join("\n", district.TuesdayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsWednesdayOld = string.Join("\n", district.CopyOf.WednesdayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsWednesdayNew = string.Join("\n", district.WednesdayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsThursdayOld = string.Join("\n", district.CopyOf.ThursdayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsThursdayNew = string.Join("\n", district.ThursdayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsFridayOld = string.Join("\n", district.CopyOf.FridayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsFridayNew = string.Join("\n", district.FridayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsSaturdayOld = string.Join("\n", district.CopyOf.SaturdayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsSaturdayNew = string.Join("\n", district.SaturdayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsSundayOld = string.Join("\n", district.CopyOf.SundayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsSundayNew = string.Join("\n", district.SundayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),

						RegionChanged = district.DistrictBorder != district.CopyOf.DistrictBorder,

						DeliveryRulesSpecialOld = string.Join("\n",
							district.CopyOf.TodayDistrictRuleItems.Select(cdri => cdri.Title),
							district.CopyOf.MondayDistrictRuleItems.Select(cdri => cdri.Title),
							district.CopyOf.TuesdayDistrictRuleItems.Select(cdri => cdri.Title),
							district.CopyOf.WednesdayDistrictRuleItems.Select(cdri => cdri.Title),
							district.CopyOf.ThursdayDistrictRuleItems.Select(cdri => cdri.Title),
							district.CopyOf.FridayDistrictRuleItems.Select(cdri => cdri.Title),
							district.CopyOf.SaturdayDistrictRuleItems.Select(cdri => cdri.Title),
							district.CopyOf.SundayDistrictRuleItems.Select(cdri => cdri.Title)),

						DeliveryRulesSpecialNew = string.Join("\n",
							district.TodayDistrictRuleItems.Select(cdri => cdri.Title),
							district.MondayDistrictRuleItems.Select(cdri => cdri.Title),
							district.TuesdayDistrictRuleItems.Select(cdri => cdri.Title),
							district.WednesdayDistrictRuleItems.Select(cdri => cdri.Title),
							district.ThursdayDistrictRuleItems.Select(cdri => cdri.Title),
							district.FridayDistrictRuleItems.Select(cdri => cdri.Title),
							district.SaturdayDistrictRuleItems.Select(cdri => cdri.Title),
							district.SundayDistrictRuleItems.Select(cdri => cdri.Title)),
					});
					continue;
				}

				if(district.CopyOf is null)
				{
					districtsAdded.Add(new DistrictRow
					{
						Id = district.Id,
						Name = district.DistrictName,
						TariffZoneName = district.TariffZone.Name,
						MinimalBottlesCount = district.MinBottles,

						DelikveryRulesGeneral = string.Join("\n", district.CommonDistrictRuleItems.Select(cdri => cdri.Title)),

						DeliveryShiftsToday = string.Join("\n", district.TodayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsMonday = string.Join("\n", district.MondayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsTuesday = string.Join("\n", district.TuesdayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsWednesday = string.Join("\n", district.WednesdayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsThursday = string.Join("\n", district.ThursdayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsFriday = string.Join("\n", district.FridayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsSaturday = string.Join("\n", district.SaturdayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsSunday = string.Join("\n", district.SundayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),

						DeliveryRulesSpecial = string.Join("\n",
							district.TodayDistrictRuleItems.Select(cdri => cdri.Title),
							district.MondayDistrictRuleItems.Select(cdri => cdri.Title),
							district.TuesdayDistrictRuleItems.Select(cdri => cdri.Title),
							district.WednesdayDistrictRuleItems.Select(cdri => cdri.Title),
							district.ThursdayDistrictRuleItems.Select(cdri => cdri.Title),
							district.FridayDistrictRuleItems.Select(cdri => cdri.Title),
							district.SaturdayDistrictRuleItems.Select(cdri => cdri.Title),
							district.SundayDistrictRuleItems.Select(cdri => cdri.Title)),
					});
				}
			}

			foreach(var district in oldDistricts)
			{
				if(!newDistricts.Select(nd => nd.CopyOf).Contains(district))
				{
					districtsRemoved.Add(new DistrictRow
					{
						Id = district.Id,
						Name = district.DistrictName,
						TariffZoneName = district.TariffZone.Name,
						MinimalBottlesCount = district.MinBottles,

						DelikveryRulesGeneral = string.Join("\n", district.CommonDistrictRuleItems.Select(cdri => cdri.Title)),

						DeliveryShiftsToday = string.Join("\n", district.TodayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsMonday = string.Join("\n", district.MondayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsTuesday = string.Join("\n", district.TuesdayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsWednesday = string.Join("\n", district.WednesdayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsThursday = string.Join("\n", district.ThursdayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsFriday = string.Join("\n", district.FridayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsSaturday = string.Join("\n", district.SaturdayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),
						DeliveryShiftsSunday = string.Join("\n", district.SundayDeliveryScheduleRestrictions.Select(dsr => dsr.AcceptBeforeTitle + dsr.DeliverySchedule.Name)),

						DeliveryRulesSpecial = string.Join("\n",
							district.TodayDistrictRuleItems.Select(cdri => cdri.Title), 
							district.MondayDistrictRuleItems.Select(cdri => cdri.Title), 
							district.TuesdayDistrictRuleItems.Select(cdri => cdri.Title), 
							district.WednesdayDistrictRuleItems.Select(cdri => cdri.Title), 
							district.ThursdayDistrictRuleItems.Select(cdri => cdri.Title), 
							district.FridayDistrictRuleItems.Select(cdri => cdri.Title), 
							district.SaturdayDistrictRuleItems.Select(cdri => cdri.Title), 
							district.SundayDistrictRuleItems.Select(cdri => cdri.Title)),
					});
				}
			}

			return new DistrictsSetDiffReport(districtsChanged, districtsAdded, districtsRemoved);
		}
	}
}
