using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Cryptography;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Errors;
using Vodovoz.ViewModels.Reports;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	[Appellative(Nominative = "Отчет об изменениях версий районов")]
	public partial class DistrictsSetDiffReport : IClosedXmlReport
	{
		public string TemplatePath { get; } = @".\Reports\Logistic\DistrictsSetDiffReport.xlsx";

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

		private static string ConvertRestrictionsToString(IEnumerable<DeliveryScheduleRestriction> deliveryScheduleRestrictions) =>
			string.Join("\n",
					deliveryScheduleRestrictions
						.OrderBy(x => x.AcceptBefore == null)
						.ThenBy(x => x.DeliverySchedule.Name)
						.Select(FormatDeliveryShift))
				.Trim('\n');

		private static string FormatDeliveryShift(DeliveryScheduleRestriction deliveryScheduleRestriction)
		{
			if(string.IsNullOrWhiteSpace(deliveryScheduleRestriction.AcceptBeforeTitle))
			{
				return deliveryScheduleRestriction.DeliverySchedule.Name;
			}

			return $"Прием до: {deliveryScheduleRestriction.AcceptBeforeTitle}: {deliveryScheduleRestriction.DeliverySchedule.Name}";
		}

		public static Result<DistrictsSetDiffReport> Generate(IUnitOfWork unitOfWork, int? diffSourceDistrictSetVersionId, int? diffTargetDistrictSetVersionId)
		{
			var sourceDistrictSet =
				(from districtSet in unitOfWork.Session.Query<DistrictsSet>()
				 where districtSet.Id == diffSourceDistrictSetVersionId.Value
				 select districtSet)
				.FirstOrDefault();

			if(sourceDistrictSet is null)
			{
				return Result.Failure<DistrictsSetDiffReport>(Errors.Logistics.DistrictSet.NotFound(diffSourceDistrictSetVersionId.Value));
			}

			var targetDistrictSet =
				(from districtSet in unitOfWork.Session.Query<DistrictsSet>()
				 where districtSet.Id == diffTargetDistrictSetVersionId.Value
				 select districtSet)
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

						DeliveryShiftsTodayOld = ConvertRestrictionsToString(district.CopyOf.TodayDeliveryScheduleRestrictions),
						DeliveryShiftsTodayNew = ConvertRestrictionsToString(district.TodayDeliveryScheduleRestrictions),

						DeliveryShiftsMondayOld = ConvertRestrictionsToString(district.CopyOf.MondayDeliveryScheduleRestrictions),
						DeliveryShiftsMondayNew = ConvertRestrictionsToString(district.MondayDeliveryScheduleRestrictions),

						DeliveryShiftsTuesdayOld = ConvertRestrictionsToString(district.CopyOf.TuesdayDeliveryScheduleRestrictions),
						DeliveryShiftsTuesdayNew = ConvertRestrictionsToString(district.TuesdayDeliveryScheduleRestrictions),

						DeliveryShiftsWednesdayOld = ConvertRestrictionsToString(district.CopyOf.WednesdayDeliveryScheduleRestrictions),
						DeliveryShiftsWednesdayNew = ConvertRestrictionsToString(district.WednesdayDeliveryScheduleRestrictions),

						DeliveryShiftsThursdayOld = ConvertRestrictionsToString(district.CopyOf.ThursdayDeliveryScheduleRestrictions),
						DeliveryShiftsThursdayNew = ConvertRestrictionsToString(district.ThursdayDeliveryScheduleRestrictions),

						DeliveryShiftsFridayOld = ConvertRestrictionsToString(district.CopyOf.FridayDeliveryScheduleRestrictions),
						DeliveryShiftsFridayNew = ConvertRestrictionsToString(district.FridayDeliveryScheduleRestrictions),

						DeliveryShiftsSaturdayOld = ConvertRestrictionsToString(district.CopyOf.SaturdayDeliveryScheduleRestrictions),
						DeliveryShiftsSaturdayNew = ConvertRestrictionsToString(district.SaturdayDeliveryScheduleRestrictions),

						DeliveryShiftsSundayOld = ConvertRestrictionsToString(district.CopyOf.SundayDeliveryScheduleRestrictions),
						DeliveryShiftsSundayNew = ConvertRestrictionsToString(district.SundayDeliveryScheduleRestrictions),

						RegionChanged = district.DistrictBorder != district.CopyOf.DistrictBorder,

						DeliveryRulesSpecialOld = string.Join(
								"\n",
								district.CopyOf.TodayDistrictRuleItems.Any()
									? "ДД: " + string.Join("\n", district.CopyOf.TodayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.CopyOf.MondayDistrictRuleItems.Any()
									? "ПН: " + string.Join("\n", district.CopyOf.MondayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.CopyOf.TuesdayDistrictRuleItems.Any()
									? "ВТ: " + string.Join("\n", district.CopyOf.TuesdayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.CopyOf.WednesdayDistrictRuleItems.Any()
									? "СР: " + string.Join("\n", district.CopyOf.WednesdayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.CopyOf.ThursdayDistrictRuleItems.Any()
									? "ЧТ: " + string.Join("\n", district.CopyOf.ThursdayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.CopyOf.FridayDistrictRuleItems.Any()
									? "ПТ: " + string.Join("\n", district.CopyOf.FridayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.CopyOf.SaturdayDistrictRuleItems.Any()
									? "СБ: " + string.Join("\n", district.CopyOf.SaturdayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.CopyOf.SundayDistrictRuleItems.Any()
									? "ВС: " + string.Join("\n", district.CopyOf.SundayDistrictRuleItems.Select(cdri => cdri.Title))
									: "")
							.Trim('\n'),

						DeliveryRulesSpecialNew = string.Join(
								"\n",
								district.TodayDistrictRuleItems.Any()
									? "ДД: " + string.Join("\n", district.TodayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.MondayDistrictRuleItems.Any()
									? "ПН: " + string.Join("\n", district.MondayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.TuesdayDistrictRuleItems.Any()
									? "ВТ: " + string.Join("\n", district.TuesdayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.WednesdayDistrictRuleItems.Any()
									? "СР: " + string.Join("\n", district.WednesdayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.ThursdayDistrictRuleItems.Any()
									? "ЧТ: " + string.Join("\n", district.ThursdayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.FridayDistrictRuleItems.Any()
									? "ПТ: " + string.Join("\n", district.FridayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.SaturdayDistrictRuleItems.Any()
									? "СБ: " + string.Join("\n", district.SaturdayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.SundayDistrictRuleItems.Any()
									? "ВС: " + string.Join("\n", district.SundayDistrictRuleItems.Select(cdri => cdri.Title))
									: "")
							.Trim('\n'),
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

						DeliveryShiftsToday = ConvertRestrictionsToString(district.TodayDeliveryScheduleRestrictions),
						DeliveryShiftsMonday = ConvertRestrictionsToString(district.MondayDeliveryScheduleRestrictions),
						DeliveryShiftsTuesday = ConvertRestrictionsToString(district.TuesdayDeliveryScheduleRestrictions),
						DeliveryShiftsWednesday = ConvertRestrictionsToString(district.WednesdayDeliveryScheduleRestrictions),
						DeliveryShiftsThursday = ConvertRestrictionsToString(district.ThursdayDeliveryScheduleRestrictions),
						DeliveryShiftsFriday = ConvertRestrictionsToString(district.FridayDeliveryScheduleRestrictions),
						DeliveryShiftsSaturday = ConvertRestrictionsToString(district.SaturdayDeliveryScheduleRestrictions),
						DeliveryShiftsSunday = ConvertRestrictionsToString(district.SundayDeliveryScheduleRestrictions),

						DeliveryRulesSpecial = string.Join(
								"\n",
								district.TodayDistrictRuleItems.Any()
									? "ДД: " + string.Join("\n", district.TodayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.MondayDistrictRuleItems.Any()
									? "ПН: " + string.Join("\n", district.MondayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.TuesdayDistrictRuleItems.Any()
									? "ВТ: " + string.Join("\n", district.TuesdayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.WednesdayDistrictRuleItems.Any()
									? "СР: " + string.Join("\n", district.WednesdayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.ThursdayDistrictRuleItems.Any()
									? "ЧТ: " + string.Join("\n", district.ThursdayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.FridayDistrictRuleItems.Any()
									? "ПТ: " + string.Join("\n", district.FridayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.SaturdayDistrictRuleItems.Any()
									? "СБ: " + string.Join("\n", district.SaturdayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.SundayDistrictRuleItems.Any()
									? "ВС: " + string.Join("\n", district.SundayDistrictRuleItems.Select(cdri => cdri.Title))
									: "")
							.Trim('\n'),
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

						DelikveryRulesGeneral = string.Join("\n", district.CommonDistrictRuleItems.Select(cdri => cdri.Title)).Trim('\n'),

						DeliveryShiftsToday = ConvertRestrictionsToString(district.TodayDeliveryScheduleRestrictions),
						DeliveryShiftsMonday = ConvertRestrictionsToString(district.MondayDeliveryScheduleRestrictions),
						DeliveryShiftsTuesday = ConvertRestrictionsToString(district.TuesdayDeliveryScheduleRestrictions),
						DeliveryShiftsWednesday = ConvertRestrictionsToString(district.WednesdayDeliveryScheduleRestrictions),
						DeliveryShiftsThursday = ConvertRestrictionsToString(district.ThursdayDeliveryScheduleRestrictions),
						DeliveryShiftsFriday = ConvertRestrictionsToString(district.FridayDeliveryScheduleRestrictions),
						DeliveryShiftsSaturday = ConvertRestrictionsToString(district.SaturdayDeliveryScheduleRestrictions),
						DeliveryShiftsSunday = ConvertRestrictionsToString(district.SundayDeliveryScheduleRestrictions),

						DeliveryRulesSpecial = string.Join(
								"\n",
								district.TodayDistrictRuleItems.Any()
									? "ДД: " + string.Join("\n", district.TodayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.MondayDistrictRuleItems.Any()
									? "ПН: " + string.Join("\n", district.MondayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.TuesdayDistrictRuleItems.Any()
									? "ВТ: " + string.Join("\n", district.TuesdayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.WednesdayDistrictRuleItems.Any()
									? "СР: " + string.Join("\n", district.WednesdayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.ThursdayDistrictRuleItems.Any()
									? "ЧТ: " + string.Join("\n", district.ThursdayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.FridayDistrictRuleItems.Any()
									? "ПТ: " + string.Join("\n", district.FridayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.SaturdayDistrictRuleItems.Any()
									? "СБ: " + string.Join("\n", district.SaturdayDistrictRuleItems.Select(cdri => cdri.Title))
									: "",
								district.SundayDistrictRuleItems.Any()
									? "ВС: " + string.Join("\n", district.SundayDistrictRuleItems.Select(cdri => cdri.Title))
									: "")
							.Trim('\n'),
					});
				}
			}

			return new DistrictsSetDiffReport(districtsChanged, districtsAdded, districtsRemoved);
		}
	}
}
