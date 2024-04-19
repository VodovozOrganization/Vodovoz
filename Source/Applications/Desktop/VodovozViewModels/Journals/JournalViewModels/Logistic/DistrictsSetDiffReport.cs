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
							? ConvertDiffDeliveryRulesGeneralOld(district.CopyOf.CommonDistrictRuleItems, district.CommonDistrictRuleItems)
							: "",

						DelikveryRulesGeneralNew = 
							delikveryRulesGeneralChanged
							? ConvertDiffDeliveryRulesGeneralNew(district.CopyOf.CommonDistrictRuleItems, district.CommonDistrictRuleItems)
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

						DeliveryRulesSpecialOld = ConvertDiffDeliveryRulesSpecialOld(
							district.CopyOf.TodayDistrictRuleItems,
							district.TodayDistrictRuleItems,
							district.CopyOf.MondayDistrictRuleItems,
							district.MondayDistrictRuleItems,
							district.CopyOf.TuesdayDistrictRuleItems,
							district.TuesdayDistrictRuleItems,
							district.CopyOf.WednesdayDistrictRuleItems,
							district.WednesdayDistrictRuleItems,
							district.CopyOf.ThursdayDistrictRuleItems,
							district.ThursdayDistrictRuleItems,
							district.CopyOf.FridayDistrictRuleItems,
							district.FridayDistrictRuleItems,
							district.CopyOf.SaturdayDistrictRuleItems,
							district.SaturdayDistrictRuleItems,
							district.CopyOf.SundayDistrictRuleItems,
							district.SundayDistrictRuleItems),

						DeliveryRulesSpecialNew = ConvertDiffDeliveryRulesSpecialNew(
							district.CopyOf.TodayDistrictRuleItems,
							district.TodayDistrictRuleItems,
							district.CopyOf.MondayDistrictRuleItems,
							district.MondayDistrictRuleItems,
							district.CopyOf.TuesdayDistrictRuleItems,
							district.TuesdayDistrictRuleItems,
							district.CopyOf.WednesdayDistrictRuleItems,
							district.WednesdayDistrictRuleItems,
							district.CopyOf.ThursdayDistrictRuleItems,
							district.ThursdayDistrictRuleItems,
							district.CopyOf.FridayDistrictRuleItems,
							district.FridayDistrictRuleItems,
							district.CopyOf.SaturdayDistrictRuleItems,
							district.SaturdayDistrictRuleItems,
							district.CopyOf.SundayDistrictRuleItems,
							district.SundayDistrictRuleItems),
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

					DeliveryRulesSpecial = ConvertDeliveryRulesSpecialToString(district),
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

					DeliveryRulesSpecial = ConvertDeliveryRulesSpecialToString(district),
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

		private static string ConvertDiffRestrictionsNewToString(IEnumerable<DeliveryScheduleRestriction> deliveryScheduleRestrictionsOld, IEnumerable<DeliveryScheduleRestriction> deliveryScheduleRestrictionsNew)
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

		private static string ConvertDiffRestrictionsOldToString(IEnumerable<DeliveryScheduleRestriction> deliveryScheduleRestrictionsOld, IEnumerable<DeliveryScheduleRestriction> deliveryScheduleRestrictionsNew)
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
