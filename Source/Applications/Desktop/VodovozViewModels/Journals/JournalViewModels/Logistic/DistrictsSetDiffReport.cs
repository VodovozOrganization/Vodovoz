using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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

			var onlyDifferentDistricts = newDistricts
				.Where(d =>
					oldDistricts.Contains(d.CopyOf)
					&& (
						d.TariffZone.Id != d.CopyOf.TariffZone.Id
						|| d.MinBottles != d.CopyOf.MinBottles
						|| AllDistrictRuleItemsChanged(d)
						|| !d.AllDeliveryScheduleRestrictions.All(dsr => d.CopyOf.AllDeliveryScheduleRestrictions
							.Select(x => (x.AcceptBefore, x.WeekDay, x.DeliverySchedule.From, x.DeliverySchedule.To))
							.Contains((dsr.AcceptBefore, dsr.WeekDay, dsr.DeliverySchedule.From, dsr.DeliverySchedule.To)))
						|| !d.CopyOf.AllDeliveryScheduleRestrictions.All(dsr => d.AllDeliveryScheduleRestrictions
							.Select(x => (x.AcceptBefore, x.WeekDay, x.DeliverySchedule.From, x.DeliverySchedule.To))
							.Contains((dsr.AcceptBefore, dsr.WeekDay, dsr.DeliverySchedule.From, dsr.DeliverySchedule.To)))));

			foreach(var district in onlyDifferentDistricts)
			{
				if(oldDistricts.Contains(district.CopyOf))
				{
					var tarifZoneChanged = district.CopyOf.TariffZone.Name != district.TariffZone.Name;

					var minimalBottlesCountChanged = district.CopyOf.MinBottles != district.MinBottles;

					var delikveryRulesGeneralChanged = CommonDistrictRuleItemsChanged(district);

					districtsChanged.Add(new DistrictDiffRow
					{
						Id = district.Id,

						Name = district.DistrictName,

						TariffZoneNameOld = tarifZoneChanged
							? district.CopyOf.TariffZone.Name
							: "",

						TariffZoneNameNew = tarifZoneChanged
							? district.TariffZone.Name
							: "",

						MinimalBottlesCountOld = minimalBottlesCountChanged
							? (int?)district.CopyOf.MinBottles
							: null,

						MinimalBottlesCountNew = minimalBottlesCountChanged
							? (int?)district.MinBottles
							: null,

						DelikveryRulesGeneralOld =
							delikveryRulesGeneralChanged
							? string.Join("\n", district.CopyOf.CommonDistrictRuleItems.Select(cdri => cdri.Title))
							: "",

						DelikveryRulesGeneralNew =
							delikveryRulesGeneralChanged
							? string.Join("\n", district.CommonDistrictRuleItems.Select(cdri => cdri.Title))
							: "",

						DeliveryShiftsTodayOld = ConvertDiffRestrictionsOldToString(district.CopyOf.TodayDeliveryScheduleRestrictions, district.TodayDeliveryScheduleRestrictions),
						DeliveryShiftsTodayNew = ConvertDiffRestrictionsNewToString(district.CopyOf.TodayDeliveryScheduleRestrictions, district.TodayDeliveryScheduleRestrictions),

						DeliveryShiftsMondayOld = ConvertDiffRestrictionsOldToString(district.CopyOf.MondayDeliveryScheduleRestrictions, district.MondayDeliveryScheduleRestrictions),
						DeliveryShiftsMondayNew = ConvertDiffRestrictionsNewToString(district.CopyOf.MondayDeliveryScheduleRestrictions, district.MondayDeliveryScheduleRestrictions),

						DeliveryShiftsTuesdayOld = ConvertDiffRestrictionsOldToString(district.CopyOf.TuesdayDeliveryScheduleRestrictions, district.TuesdayDeliveryScheduleRestrictions),
						DeliveryShiftsTuesdayNew = ConvertDiffRestrictionsNewToString(district.CopyOf.TuesdayDeliveryScheduleRestrictions, district.TuesdayDeliveryScheduleRestrictions),

						DeliveryShiftsWednesdayOld = ConvertDiffRestrictionsOldToString(district.CopyOf.WednesdayDeliveryScheduleRestrictions, district.WednesdayDeliveryScheduleRestrictions),
						DeliveryShiftsWednesdayNew = ConvertDiffRestrictionsNewToString(district.CopyOf.WednesdayDeliveryScheduleRestrictions, district.WednesdayDeliveryScheduleRestrictions),

						DeliveryShiftsThursdayOld = ConvertDiffRestrictionsOldToString(district.CopyOf.ThursdayDeliveryScheduleRestrictions, district.ThursdayDeliveryScheduleRestrictions),
						DeliveryShiftsThursdayNew = ConvertDiffRestrictionsNewToString(district.CopyOf.ThursdayDeliveryScheduleRestrictions, district.ThursdayDeliveryScheduleRestrictions),

						DeliveryShiftsFridayOld = ConvertDiffRestrictionsOldToString(district.CopyOf.FridayDeliveryScheduleRestrictions, district.FridayDeliveryScheduleRestrictions),
						DeliveryShiftsFridayNew = ConvertDiffRestrictionsNewToString(district.CopyOf.FridayDeliveryScheduleRestrictions, district.FridayDeliveryScheduleRestrictions),

						DeliveryShiftsSaturdayOld = ConvertDiffRestrictionsOldToString(district.CopyOf.SaturdayDeliveryScheduleRestrictions, district.SaturdayDeliveryScheduleRestrictions),
						DeliveryShiftsSaturdayNew = ConvertDiffRestrictionsNewToString(district.CopyOf.SaturdayDeliveryScheduleRestrictions, district.SaturdayDeliveryScheduleRestrictions),

						DeliveryShiftsSundayOld = ConvertDiffRestrictionsOldToString(district.CopyOf.SundayDeliveryScheduleRestrictions, district.SundayDeliveryScheduleRestrictions),
						DeliveryShiftsSundayNew = ConvertDiffRestrictionsNewToString(district.CopyOf.SundayDeliveryScheduleRestrictions, district.SundayDeliveryScheduleRestrictions),

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
			}

			var addedDistrictsOnly = newDistricts.Where(d => d.CopyOf is null);

			foreach(var district in addedDistrictsOnly)
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

			var removedDistrictsOnly = oldDistricts.Where(d => !newDistricts.Select(nd => nd.CopyOf).Contains(d));

			foreach(var district in removedDistrictsOnly)
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

			return new DistrictsSetDiffReport(districtsChanged, districtsAdded, districtsRemoved);
		}

		private static string ConvertDiffRestrictionsNewToString(IEnumerable<DeliveryScheduleRestriction> deliveryScheduleRestrictionsOld, IEnumerable<DeliveryScheduleRestriction> deliveryScheduleRestrictionsNew) =>
			string.Join("\n",
					deliveryScheduleRestrictionsNew
						.OrderBy(x => x.AcceptBefore == null)
						.ThenBy(x => x.DeliverySchedule.Name)
						.Select(x =>
						{
							var formattedDeliveryShift = FormatDeliveryShift(x);
							if(deliveryScheduleRestrictionsOld
								.Select(FormatDeliveryShift)
								.Contains(formattedDeliveryShift))
							{
								return formattedDeliveryShift;
							}
							return "\t" + formattedDeliveryShift;
						}))
				.Trim('\n');

		private static string ConvertDiffRestrictionsOldToString(IEnumerable<DeliveryScheduleRestriction> deliveryScheduleRestrictionsOld, IEnumerable<DeliveryScheduleRestriction> deliveryScheduleRestrictionsNew) =>
			string.Join("\n",
					deliveryScheduleRestrictionsOld
						.OrderBy(x => x.AcceptBefore == null)
						.ThenBy(x => x.DeliverySchedule.Name)
						.Select(x =>
						{
							var formattedDeliveryShift = FormatDeliveryShift(x);
							if(deliveryScheduleRestrictionsNew
								.Select(FormatDeliveryShift)
								.Contains(formattedDeliveryShift))
							{
								return formattedDeliveryShift;
							}
							return "\t" + formattedDeliveryShift;
						}))
				.Trim('\n');

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

		private static bool AllDistrictRuleItemsChanged(District district) =>
			!district.AllDistrictRuleItems.All(dri => district.CopyOf.AllDistrictRuleItems
				.Select(x => (x.Price, x.DeliveryPriceRule.Id))
				.Contains((dri.Price, dri.DeliveryPriceRule.Id)))
			|| !district.CopyOf.AllDistrictRuleItems.All(dri => district.AllDistrictRuleItems
				.Select(x => (x.Price, x.DeliveryPriceRule.Id))
				.Contains((dri.Price, dri.DeliveryPriceRule.Id)));

		private static bool CommonDistrictRuleItemsChanged(District district) =>
			!district.CommonDistrictRuleItems.All(dri => district.CopyOf.CommonDistrictRuleItems
				.Select(x => (x.Price, x.DeliveryPriceRule.Id))
				.Contains((dri.Price, dri.DeliveryPriceRule.Id)))
			|| !district.CopyOf.CommonDistrictRuleItems.All(dri => district.CommonDistrictRuleItems
				.Select(x => (x.Price, x.DeliveryPriceRule.Id))
				.Contains((dri.Price, dri.DeliveryPriceRule.Id)));
	}
}
