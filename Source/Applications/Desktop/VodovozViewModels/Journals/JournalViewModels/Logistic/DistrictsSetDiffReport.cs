using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Sale;
using Vodovoz.Errors;
using Vodovoz.Extensions;
using Vodovoz.ViewModels.Reports;
using District = Vodovoz.Domain.Sale.District;

namespace Vodovoz.ViewModels.Journals.JournalViewModels.Logistic
{
	[Appellative(Nominative = "Отчет об изменениях версий районов")]
	public partial class DistrictsSetDiffReport : IClosedXmlReport
	{
		private const string _stringIndent = "    ";
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

			var oldDistricts = sourceDistrictSet.Districts.ToDictionary(x => x.DistrictName, x => x);
			var newDistricts = targetDistrictSet.Districts.ToDictionary(x => x.DistrictName, x => x);

			var districtsChanged = new List<DistrictDiffRow>();
			var districtsAdded = new List<DistrictRow>();
			var districtsRemoved = new List<DistrictRow>();

			var onlyDifferentDistricts = newDistricts
				.Where(d =>
					oldDistricts.ContainsKey(d.Key)
					&& (
						d.Value.TariffZone.Id != oldDistricts[d.Key].TariffZone.Id
						|| d.Value.CommonDistrictRuleItems.Max(c => c.DeliveryPriceRule.Water19LCount) != oldDistricts[d.Key].CommonDistrictRuleItems.Max(c => c.DeliveryPriceRule.Water19LCount)
						|| AllDistrictRuleItemsChanged(d.Value.AllDistrictRuleItems, oldDistricts[d.Key].AllDistrictRuleItems)
						|| !d.Value.AllDeliveryScheduleRestrictions.All(dsr => oldDistricts[d.Key].AllDeliveryScheduleRestrictions
							.Select(x => (x.AcceptBefore, x.WeekDay, x.DeliverySchedule.From, x.DeliverySchedule.To))
							.Contains((dsr.AcceptBefore, dsr.WeekDay, dsr.DeliverySchedule.From, dsr.DeliverySchedule.To)))
						|| !oldDistricts[d.Key].AllDeliveryScheduleRestrictions.All(dsr => d.Value.AllDeliveryScheduleRestrictions
							.Select(x => (x.AcceptBefore, x.WeekDay, x.DeliverySchedule.From, x.DeliverySchedule.To))
							.Contains((dsr.AcceptBefore, dsr.WeekDay, dsr.DeliverySchedule.From, dsr.DeliverySchedule.To)))));

			foreach(var districtPair in onlyDifferentDistricts)
			{
				var tarifZoneChanged = oldDistricts[districtPair.Key].TariffZone.Name != districtPair.Value.TariffZone.Name;

				var minimalBottlesCountChanged = oldDistricts[districtPair.Key].CommonDistrictRuleItems.Max(c => c.DeliveryPriceRule.Water19LCount) != districtPair.Value.CommonDistrictRuleItems.Max(c => c.DeliveryPriceRule.Water19LCount);

				var delikveryRulesGeneralChanged = CommonDistrictRuleItemsChanged(districtPair.Value.CommonDistrictRuleItems, oldDistricts[districtPair.Key].CommonDistrictRuleItems);

				districtsChanged.Add(new DistrictDiffRow
				{
					Id = districtPair.Value.Id,
					Name = districtPair.Value.DistrictName,

					TariffZoneNameOld = tarifZoneChanged
						? oldDistricts[districtPair.Key].TariffZone.Name
						: "",

					TariffZoneNameNew = tarifZoneChanged
						? districtPair.Value.TariffZone.Name
						: "",

					MinimalBottlesCountOld = minimalBottlesCountChanged
						? (int?)oldDistricts[districtPair.Key].CommonDistrictRuleItems.Max(c => c.DeliveryPriceRule.Water19LCount)
						: null,

					MinimalBottlesCountNew = minimalBottlesCountChanged
						? (int?)districtPair.Value.CommonDistrictRuleItems.Max(c => c.DeliveryPriceRule.Water19LCount)
						: null,

					DelikveryRulesGeneralOld = 
						delikveryRulesGeneralChanged
						? ConvertDiffDeliveryRulesGeneralOld(oldDistricts[districtPair.Key].CommonDistrictRuleItems, districtPair.Value.CommonDistrictRuleItems)
						: "",

					DelikveryRulesGeneralNew = 
						delikveryRulesGeneralChanged
						? ConvertDiffDeliveryRulesGeneralNew(oldDistricts[districtPair.Key].CommonDistrictRuleItems, districtPair.Value.CommonDistrictRuleItems)
						: "",

					DeliveryShiftsTodayOld = ConvertDiffRestrictionsOldToString(oldDistricts[districtPair.Key].TodayDeliveryScheduleRestrictions, districtPair.Value.TodayDeliveryScheduleRestrictions),
					DeliveryShiftsTodayNew = ConvertDiffRestrictionsNewToString(oldDistricts[districtPair.Key].TodayDeliveryScheduleRestrictions, districtPair.Value.TodayDeliveryScheduleRestrictions),

					DeliveryShiftsMondayOld = ConvertDiffRestrictionsOldToString(oldDistricts[districtPair.Key].MondayDeliveryScheduleRestrictions, districtPair.Value.MondayDeliveryScheduleRestrictions),
					DeliveryShiftsMondayNew = ConvertDiffRestrictionsNewToString(oldDistricts[districtPair.Key].MondayDeliveryScheduleRestrictions, districtPair.Value.MondayDeliveryScheduleRestrictions),

					DeliveryShiftsTuesdayOld = ConvertDiffRestrictionsOldToString(oldDistricts[districtPair.Key].TuesdayDeliveryScheduleRestrictions, districtPair.Value.TuesdayDeliveryScheduleRestrictions),
					DeliveryShiftsTuesdayNew = ConvertDiffRestrictionsNewToString(oldDistricts[districtPair.Key].TuesdayDeliveryScheduleRestrictions, districtPair.Value.TuesdayDeliveryScheduleRestrictions),

					DeliveryShiftsWednesdayOld = ConvertDiffRestrictionsOldToString(oldDistricts[districtPair.Key].WednesdayDeliveryScheduleRestrictions, districtPair.Value.WednesdayDeliveryScheduleRestrictions),
					DeliveryShiftsWednesdayNew = ConvertDiffRestrictionsNewToString(oldDistricts[districtPair.Key].WednesdayDeliveryScheduleRestrictions, districtPair.Value.WednesdayDeliveryScheduleRestrictions),

					DeliveryShiftsThursdayOld = ConvertDiffRestrictionsOldToString(oldDistricts[districtPair.Key].ThursdayDeliveryScheduleRestrictions, districtPair.Value.ThursdayDeliveryScheduleRestrictions),
					DeliveryShiftsThursdayNew = ConvertDiffRestrictionsNewToString(oldDistricts[districtPair.Key].ThursdayDeliveryScheduleRestrictions, districtPair.Value.ThursdayDeliveryScheduleRestrictions),

					DeliveryShiftsFridayOld = ConvertDiffRestrictionsOldToString(oldDistricts[districtPair.Key].FridayDeliveryScheduleRestrictions, districtPair.Value.FridayDeliveryScheduleRestrictions),
					DeliveryShiftsFridayNew = ConvertDiffRestrictionsNewToString(oldDistricts[districtPair.Key].FridayDeliveryScheduleRestrictions, districtPair.Value.FridayDeliveryScheduleRestrictions),

					DeliveryShiftsSaturdayOld = ConvertDiffRestrictionsOldToString(oldDistricts[districtPair.Key].SaturdayDeliveryScheduleRestrictions, districtPair.Value.SaturdayDeliveryScheduleRestrictions),
					DeliveryShiftsSaturdayNew = ConvertDiffRestrictionsNewToString(oldDistricts[districtPair.Key].SaturdayDeliveryScheduleRestrictions, districtPair.Value.SaturdayDeliveryScheduleRestrictions),

					DeliveryShiftsSundayOld = ConvertDiffRestrictionsOldToString(oldDistricts[districtPair.Key].SundayDeliveryScheduleRestrictions, districtPair.Value.SundayDeliveryScheduleRestrictions),
					DeliveryShiftsSundayNew = ConvertDiffRestrictionsNewToString(oldDistricts[districtPair.Key].SundayDeliveryScheduleRestrictions, districtPair.Value.SundayDeliveryScheduleRestrictions),

					RegionChanged = districtPair.Value.DistrictBorder != oldDistricts[districtPair.Key].DistrictBorder,

					DeliveryRulesSpecialOld = ConvertDiffDeliveryRulesSpecialOld(
						oldDistricts[districtPair.Key].TodayDistrictRuleItems,
						districtPair.Value.TodayDistrictRuleItems,
						oldDistricts[districtPair.Key].MondayDistrictRuleItems,
						districtPair.Value.MondayDistrictRuleItems,
						oldDistricts[districtPair.Key].TuesdayDistrictRuleItems,
						districtPair.Value.TuesdayDistrictRuleItems,
						oldDistricts[districtPair.Key].WednesdayDistrictRuleItems,
						districtPair.Value.WednesdayDistrictRuleItems,
						oldDistricts[districtPair.Key].ThursdayDistrictRuleItems,
						districtPair.Value.ThursdayDistrictRuleItems,
						oldDistricts[districtPair.Key].FridayDistrictRuleItems,
						districtPair.Value.FridayDistrictRuleItems,
						oldDistricts[districtPair.Key].SaturdayDistrictRuleItems,
						districtPair.Value.SaturdayDistrictRuleItems,
						oldDistricts[districtPair.Key].SundayDistrictRuleItems,
						districtPair.Value.SundayDistrictRuleItems),

					DeliveryRulesSpecialNew = ConvertDiffDeliveryRulesSpecialNew(
						oldDistricts[districtPair.Key].TodayDistrictRuleItems,
						districtPair.Value.TodayDistrictRuleItems,
						oldDistricts[districtPair.Key].MondayDistrictRuleItems,
						districtPair.Value.MondayDistrictRuleItems,
						oldDistricts[districtPair.Key].TuesdayDistrictRuleItems,
						districtPair.Value.TuesdayDistrictRuleItems,
						oldDistricts[districtPair.Key].WednesdayDistrictRuleItems,
						districtPair.Value.WednesdayDistrictRuleItems,
						oldDistricts[districtPair.Key].ThursdayDistrictRuleItems,
						districtPair.Value.ThursdayDistrictRuleItems,
						oldDistricts[districtPair.Key].FridayDistrictRuleItems,
						districtPair.Value.FridayDistrictRuleItems,
						oldDistricts[districtPair.Key].SaturdayDistrictRuleItems,
						districtPair.Value.SaturdayDistrictRuleItems,
						oldDistricts[districtPair.Key].SundayDistrictRuleItems,
						districtPair.Value.SundayDistrictRuleItems),
				});
			}

			var addedDistrictsOnly = newDistricts.Where(d => !oldDistricts.ContainsKey(d.Key));

			foreach(var district in addedDistrictsOnly)
			{
				districtsAdded.Add(new DistrictRow
				{
					Id = district.Value.Id,
					Name = district.Value.DistrictName,
					TariffZoneName = district.Value.TariffZone.Name,
					MinimalBottlesCount = district.Value.CommonDistrictRuleItems.Max(c => c.DeliveryPriceRule.Water19LCount),

					DelikveryRulesGeneral = string.Join("\n", district.Value.CommonDistrictRuleItems.Select(cdri => cdri.Title)),

					DeliveryShiftsToday = ConvertRestrictionsToString(district.Value.TodayDeliveryScheduleRestrictions),
					DeliveryShiftsMonday = ConvertRestrictionsToString(district.Value.MondayDeliveryScheduleRestrictions),
					DeliveryShiftsTuesday = ConvertRestrictionsToString(district.Value.TuesdayDeliveryScheduleRestrictions),
					DeliveryShiftsWednesday = ConvertRestrictionsToString(district.Value.WednesdayDeliveryScheduleRestrictions),
					DeliveryShiftsThursday = ConvertRestrictionsToString(district.Value.ThursdayDeliveryScheduleRestrictions),
					DeliveryShiftsFriday = ConvertRestrictionsToString(district.Value.FridayDeliveryScheduleRestrictions),
					DeliveryShiftsSaturday = ConvertRestrictionsToString(district.Value.SaturdayDeliveryScheduleRestrictions),
					DeliveryShiftsSunday = ConvertRestrictionsToString(district.Value.SundayDeliveryScheduleRestrictions),

					DeliveryRulesSpecial = ConvertDeliveryRulesSpecialToString(district.Value),
				});
			}

			var removedDistrictsOnly = oldDistricts.Where(d => !newDistricts.ContainsKey(d.Key));

			foreach(var district in removedDistrictsOnly)
			{
				districtsRemoved.Add(new DistrictRow
				{
					Id = district.Value.Id,
					Name = district.Value.DistrictName,
					TariffZoneName = district.Value.TariffZone.Name,
					MinimalBottlesCount = district.Value.CommonDistrictRuleItems.Max(c => c.DeliveryPriceRule.Water19LCount),

					DelikveryRulesGeneral = string.Join("\n", district.Value.CommonDistrictRuleItems.Select(cdri => cdri.Title)).Trim('\n'),

					DeliveryShiftsToday = ConvertRestrictionsToString(district.Value.TodayDeliveryScheduleRestrictions),
					DeliveryShiftsMonday = ConvertRestrictionsToString(district.Value.MondayDeliveryScheduleRestrictions),
					DeliveryShiftsTuesday = ConvertRestrictionsToString(district.Value.TuesdayDeliveryScheduleRestrictions),
					DeliveryShiftsWednesday = ConvertRestrictionsToString(district.Value.WednesdayDeliveryScheduleRestrictions),
					DeliveryShiftsThursday = ConvertRestrictionsToString(district.Value.ThursdayDeliveryScheduleRestrictions),
					DeliveryShiftsFriday = ConvertRestrictionsToString(district.Value.FridayDeliveryScheduleRestrictions),
					DeliveryShiftsSaturday = ConvertRestrictionsToString(district.Value.SaturdayDeliveryScheduleRestrictions),
					DeliveryShiftsSunday = ConvertRestrictionsToString(district.Value.SundayDeliveryScheduleRestrictions),

					DeliveryRulesSpecial = ConvertDeliveryRulesSpecialToString(district.Value),
				});
			}

			return new DistrictsSetDiffReport(districtsChanged, districtsAdded, districtsRemoved);
		}

		private static string ConvertDiffDeliveryRulesGeneralOld(
			GenericObservableList<CommonDistrictRuleItem> commonDistrictRuleItemsOld,
			GenericObservableList<CommonDistrictRuleItem> commonDistrictRuleItemsNew)
		{
			var stringBuilder = new StringBuilder();

			foreach(var ruleItem in commonDistrictRuleItemsOld)
			{
				if(commonDistrictRuleItemsNew
					.Select(x => (x.Price, x.DeliveryPriceRule.Id))
					.Contains((ruleItem.Price, ruleItem.DeliveryPriceRule.Id)))
				{
					stringBuilder.AppendLine(ruleItem.Title);
				}
				else
				{
					stringBuilder.AppendLine(_stringIndent + ruleItem.Title);
				}
			}
			return stringBuilder.ToString();
		}

		private static string ConvertDiffDeliveryRulesGeneralNew(
			GenericObservableList<CommonDistrictRuleItem> commonDistrictRuleItemsOld,
			GenericObservableList<CommonDistrictRuleItem> commonDistrictRuleItemsNew)
		{
			var stringBuilder = new StringBuilder();

			foreach(var ruleItem in commonDistrictRuleItemsNew)
			{
				if(commonDistrictRuleItemsOld
					.Select(x => (x.Price, x.DeliveryPriceRule.Id))
					.Contains((ruleItem.Price, ruleItem.DeliveryPriceRule.Id)))
				{
					stringBuilder.AppendLine(ruleItem.Title);
				}
				else
				{
					stringBuilder.AppendLine(_stringIndent + ruleItem.Title);
				}
			}
			return stringBuilder.ToString();
		}

		private static string ConvertDiffDeliveryRulesSpecialNew(
			GenericObservableList<WeekDayDistrictRuleItem> todayDistrictRuleItemsOld,
			GenericObservableList<WeekDayDistrictRuleItem> todayDistrictRuleItemsNew,
			GenericObservableList<WeekDayDistrictRuleItem> mondayDistrictRuleItemsOld,
			GenericObservableList<WeekDayDistrictRuleItem> mondayDistrictRuleItemsNew,
			GenericObservableList<WeekDayDistrictRuleItem> tuesdayDistrictRuleItemsOld,
			GenericObservableList<WeekDayDistrictRuleItem> tuesdayDistrictRuleItemsNew,
			GenericObservableList<WeekDayDistrictRuleItem> wednesdayDistrictRuleItemsOld,
			GenericObservableList<WeekDayDistrictRuleItem> wednesdayDistrictRuleItemsNew,
			GenericObservableList<WeekDayDistrictRuleItem> thursdayDistrictRuleItemsOld,
			GenericObservableList<WeekDayDistrictRuleItem> thursdayDistrictRuleItemsNew,
			GenericObservableList<WeekDayDistrictRuleItem> fridayDistrictRuleItemsOld,
			GenericObservableList<WeekDayDistrictRuleItem> fridayDistrictRuleItemsNew,
			GenericObservableList<WeekDayDistrictRuleItem> saturdayDistrictRuleItemsOld,
			GenericObservableList<WeekDayDistrictRuleItem> saturdayDistrictRuleItemsNew,
			GenericObservableList<WeekDayDistrictRuleItem> sundayDistrictRuleItemsOld,
			GenericObservableList<WeekDayDistrictRuleItem> sundayDistrictRuleItemsNew) =>
			string.Join(
					"\n",
					ConvertDiffDeliveryRulesSpecial(todayDistrictRuleItemsNew, todayDistrictRuleItemsOld),
					ConvertDiffDeliveryRulesSpecial(mondayDistrictRuleItemsNew, mondayDistrictRuleItemsOld),
					ConvertDiffDeliveryRulesSpecial(tuesdayDistrictRuleItemsNew, tuesdayDistrictRuleItemsOld),
					ConvertDiffDeliveryRulesSpecial(wednesdayDistrictRuleItemsNew, wednesdayDistrictRuleItemsOld),
					ConvertDiffDeliveryRulesSpecial(thursdayDistrictRuleItemsNew, thursdayDistrictRuleItemsOld),
					ConvertDiffDeliveryRulesSpecial(fridayDistrictRuleItemsNew, fridayDistrictRuleItemsOld),
					ConvertDiffDeliveryRulesSpecial(saturdayDistrictRuleItemsNew, saturdayDistrictRuleItemsOld),
					ConvertDiffDeliveryRulesSpecial(sundayDistrictRuleItemsNew, sundayDistrictRuleItemsOld))
				.Trim('\n');

		private static string ConvertDiffDeliveryRulesSpecialOld(
			GenericObservableList<WeekDayDistrictRuleItem> todayDistrictRuleItemsOld,
			GenericObservableList<WeekDayDistrictRuleItem> todayDistrictRuleItemsNew,
			GenericObservableList<WeekDayDistrictRuleItem> mondayDistrictRuleItemsOld,
			GenericObservableList<WeekDayDistrictRuleItem> mondayDistrictRuleItemsNew,
			GenericObservableList<WeekDayDistrictRuleItem> tuesdayDistrictRuleItemsOld,
			GenericObservableList<WeekDayDistrictRuleItem> tuesdayDistrictRuleItemsNew,
			GenericObservableList<WeekDayDistrictRuleItem> wednesdayDistrictRuleItemsOld,
			GenericObservableList<WeekDayDistrictRuleItem> wednesdayDistrictRuleItemsNew,
			GenericObservableList<WeekDayDistrictRuleItem> thursdayDistrictRuleItemsOld,
			GenericObservableList<WeekDayDistrictRuleItem> thursdayDistrictRuleItemsNew,
			GenericObservableList<WeekDayDistrictRuleItem> fridayDistrictRuleItemsOld,
			GenericObservableList<WeekDayDistrictRuleItem> fridayDistrictRuleItemsNew,
			GenericObservableList<WeekDayDistrictRuleItem> saturdayDistrictRuleItemsOld,
			GenericObservableList<WeekDayDistrictRuleItem> saturdayDistrictRuleItemsNew,
			GenericObservableList<WeekDayDistrictRuleItem> sundayDistrictRuleItemsOld,
			GenericObservableList<WeekDayDistrictRuleItem> sundayDistrictRuleItemsNew) =>
			string.Join(
					"\n",
					ConvertDiffDeliveryRulesSpecial(todayDistrictRuleItemsOld, todayDistrictRuleItemsNew),
					ConvertDiffDeliveryRulesSpecial(mondayDistrictRuleItemsOld, mondayDistrictRuleItemsNew),
					ConvertDiffDeliveryRulesSpecial(tuesdayDistrictRuleItemsOld, tuesdayDistrictRuleItemsNew),
					ConvertDiffDeliveryRulesSpecial(wednesdayDistrictRuleItemsOld, wednesdayDistrictRuleItemsNew),
					ConvertDiffDeliveryRulesSpecial(thursdayDistrictRuleItemsOld, thursdayDistrictRuleItemsNew),
					ConvertDiffDeliveryRulesSpecial(fridayDistrictRuleItemsOld, fridayDistrictRuleItemsNew),
					ConvertDiffDeliveryRulesSpecial(saturdayDistrictRuleItemsOld, saturdayDistrictRuleItemsNew),
					ConvertDiffDeliveryRulesSpecial(sundayDistrictRuleItemsOld, sundayDistrictRuleItemsNew))
				.Trim('\n');

		private static string ConvertDiffDeliveryRulesSpecial(
			GenericObservableList<WeekDayDistrictRuleItem> weekdayDistrictRuleItemsOld,
			GenericObservableList<WeekDayDistrictRuleItem> weekdayDistrictRuleItemsNew)
		{
			if(!weekdayDistrictRuleItemsOld.Any())
			{
				return "";
			}

			if(weekdayDistrictRuleItemsNew.All(wdrin => weekdayDistrictRuleItemsOld
					.Select(x => (x.Price, x.DeliveryPriceRule.Id))
					.Contains((wdrin.Price, wdrin.DeliveryPriceRule.Id)))
				&& weekdayDistrictRuleItemsOld.All(wdrio => weekdayDistrictRuleItemsNew
					.Select(x => (x.Price, x.DeliveryPriceRule.Id))
					.Contains((wdrio.Price, wdrio.DeliveryPriceRule.Id))))
			{
				return "";
			}

			var stringBuilder = new StringBuilder();

			stringBuilder.AppendLine(WeekDayToVeryShort(weekdayDistrictRuleItemsOld.First().WeekDay));

			foreach(var weekDayDistrictRuleItemOld in weekdayDistrictRuleItemsOld)
			{
				if(!weekdayDistrictRuleItemsNew.Select(x => (x.Price, x.DeliveryPriceRule.Id)).Contains((weekDayDistrictRuleItemOld.Price, weekDayDistrictRuleItemOld.DeliveryPriceRule.Id)))
				{
					stringBuilder.AppendLine(_stringIndent + weekDayDistrictRuleItemOld.Title);
				}
				else
				{
					stringBuilder.AppendLine(weekDayDistrictRuleItemOld.Title);
				}
			}

			return stringBuilder.ToString();
		}

		private static string WeekDayToVeryShort(WeekDayName weekDayName) =>
			weekDayName.GetEnumDisplayName(true) + ": ";

		private static string ConvertDiffRestrictionsNewToString(
			IEnumerable<DeliveryScheduleRestriction> deliveryScheduleRestrictionsOld,
			IEnumerable<DeliveryScheduleRestriction> deliveryScheduleRestrictionsNew)
		{
			if(deliveryScheduleRestrictionsOld
					.All(dsro => deliveryScheduleRestrictionsNew
						.Select(x => (x.AcceptBefore, x.DeliverySchedule.From, x.DeliverySchedule.To))
						.Contains((dsro.AcceptBefore, dsro.DeliverySchedule.From, dsro.DeliverySchedule.To)))
				&& deliveryScheduleRestrictionsNew
					.All(dsro => deliveryScheduleRestrictionsOld
						.Select(x => (x.AcceptBefore, x.DeliverySchedule.From, x.DeliverySchedule.To))
						.Contains((dsro.AcceptBefore, dsro.DeliverySchedule.From, dsro.DeliverySchedule.To))))
			{
				return string.Empty;
			}

			return string.Join("\n",
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
							return _stringIndent + formattedDeliveryShift;
						}))
				.Trim('\n');
		}

		private static string ConvertDiffRestrictionsOldToString(
			IEnumerable<DeliveryScheduleRestriction> deliveryScheduleRestrictionsOld,
			IEnumerable<DeliveryScheduleRestriction> deliveryScheduleRestrictionsNew)
		{
			if(deliveryScheduleRestrictionsOld
				.All(dsro => deliveryScheduleRestrictionsNew
					.Select(x => (x.AcceptBefore, x.DeliverySchedule.From, x.DeliverySchedule.To))
					.Contains((dsro.AcceptBefore, dsro.DeliverySchedule.From, dsro.DeliverySchedule.To)))
				&& deliveryScheduleRestrictionsNew
					.All(dsro => deliveryScheduleRestrictionsOld
						.Select(x => (x.AcceptBefore, x.DeliverySchedule.From, x.DeliverySchedule.To))
						.Contains((dsro.AcceptBefore, dsro.DeliverySchedule.From, dsro.DeliverySchedule.To))))
			{
				return string.Empty;
			}

			return string.Join("\n",
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
							return _stringIndent + formattedDeliveryShift;
						}))
				.Trim('\n');
		}

		private static string ConvertDeliveryRulesSpecialToString(District district)
		{
			var stringBuilder = new StringBuilder();

			if(district.TodayDistrictRuleItems.Any())
			{
				stringBuilder.AppendLine(ConvertDeliveryRulesSpecialToString(WeekDayName.Today, district.TodayDistrictRuleItems));
			}

			if(district.TodayDistrictRuleItems.Any())
			{
				stringBuilder.AppendLine(ConvertDeliveryRulesSpecialToString(WeekDayName.Monday, district.MondayDistrictRuleItems));
			}

			if(district.TodayDistrictRuleItems.Any())
			{
				stringBuilder.AppendLine(ConvertDeliveryRulesSpecialToString(WeekDayName.Tuesday, district.TuesdayDistrictRuleItems));
			}

			if(district.TodayDistrictRuleItems.Any())
			{
				stringBuilder.AppendLine(ConvertDeliveryRulesSpecialToString(WeekDayName.Wednesday, district.WednesdayDistrictRuleItems));
			}

			if(district.TodayDistrictRuleItems.Any())
			{
				stringBuilder.AppendLine(ConvertDeliveryRulesSpecialToString(WeekDayName.Thursday, district.ThursdayDistrictRuleItems));
			}

			if(district.TodayDistrictRuleItems.Any())
			{
				stringBuilder.AppendLine(ConvertDeliveryRulesSpecialToString(WeekDayName.Friday, district.FridayDistrictRuleItems));
			}

			if(district.TodayDistrictRuleItems.Any())
			{
				stringBuilder.AppendLine(ConvertDeliveryRulesSpecialToString(WeekDayName.Saturday, district.SaturdayDistrictRuleItems));
			}

			if(district.TodayDistrictRuleItems.Any())
			{
				stringBuilder.AppendLine(ConvertDeliveryRulesSpecialToString(WeekDayName.Sunday, district.SundayDistrictRuleItems));
			}

			return stringBuilder.ToString();
		}

		private static string ConvertDeliveryRulesSpecialToString(WeekDayName weekDayName, IEnumerable<WeekDayDistrictRuleItem> items)
		{
			if(!items.Any())
			{
				return string.Empty;
			}

			return WeekDayToVeryShort(weekDayName) + items.OrderByDescending(cdri => cdri.DeliveryPriceRule.Water19LCount).Select(cdri => cdri.Title);
		}

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

		private static bool AllDistrictRuleItemsChanged(
			IEnumerable<DistrictRuleItemBase> oldDistrictRuleItems,
			IEnumerable<DistrictRuleItemBase> newDistrictRuleItems) =>
				!oldDistrictRuleItems.All(dri => newDistrictRuleItems
					.Select(x => (x.Price, x.DeliveryPriceRule.Id))
					.Contains((dri.Price, dri.DeliveryPriceRule.Id)))
				|| !newDistrictRuleItems.All(dri => oldDistrictRuleItems
					.Select(x => (x.Price, x.DeliveryPriceRule.Id))
					.Contains((dri.Price, dri.DeliveryPriceRule.Id)));

		private static bool CommonDistrictRuleItemsChanged(
			IEnumerable<CommonDistrictRuleItem> oldDistrictRuleItems,
			IEnumerable<DistrictRuleItemBase> newDistrictRuleItems) =>
				!oldDistrictRuleItems.All(dri => newDistrictRuleItems
					.Select(x => (x.Price, x.DeliveryPriceRule.Id))
					.Contains((dri.Price, dri.DeliveryPriceRule.Id)))
				|| !newDistrictRuleItems.All(dri => oldDistrictRuleItems
					.Select(x => (x.Price, x.DeliveryPriceRule.Id))
					.Contains((dri.Price, dri.DeliveryPriceRule.Id)));
	}
}
