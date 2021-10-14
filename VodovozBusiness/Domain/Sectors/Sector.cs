using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Sale;
using Vodovoz.Tools.Orders;

namespace Vodovoz.Domain.Sectors
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "районы",
		Nominative = "район")]
	[EntityPermission]
	[HistoryTrace]
	public class Sector : BusinessObjectBase<Sector>, IDomainObject
	{
		#region Свойства
		public virtual string Title => GetActiveSectorVersionOnDate().SectorName;

		public virtual int Id { get; set; }

		private DateTime dateCreated;
		[Display(Name = "Время создания")]
		public virtual DateTime DateCreated {
			get => dateCreated;
			set => SetField(ref dateCreated, value);
		}

		private IList<SectorVersion> _sectorVersions = new List<SectorVersion>();

		public virtual IList<SectorVersion> SectorVersions
		{
			get => _sectorVersions;
			set => SetField(ref _sectorVersions, value);
		}

		private GenericObservableList<SectorVersion> _observableSectorVersions;

		public virtual GenericObservableList<SectorVersion> ObservableSectorVersions => _observableSectorVersions ??
		                                                                               (_observableSectorVersions = new GenericObservableList<SectorVersion>(SectorVersions));

		private IList<SectorDeliveryRuleVersion> _sectorDeliveryRuleVersions = new List<SectorDeliveryRuleVersion>();

		public virtual IList<SectorDeliveryRuleVersion> SectorDeliveryRuleVersions
		{
			get => _sectorDeliveryRuleVersions;
			set => SetField(ref _sectorDeliveryRuleVersions, value);
		}

		private GenericObservableList<SectorDeliveryRuleVersion> _observableSectorDeliveryRuleVersions;

		public virtual GenericObservableList<SectorDeliveryRuleVersion> ObservableSectorDeliveryRuleVersions =>
			_observableSectorDeliveryRuleVersions ??
			(_observableSectorDeliveryRuleVersions = new GenericObservableList<SectorDeliveryRuleVersion>(SectorDeliveryRuleVersions));

		private IList<SectorWeekDayScheduleVersion> _sectorWeekDaySchedulesVersions = new List<SectorWeekDayScheduleVersion>();

		public virtual IList<SectorWeekDayScheduleVersion> SectorWeekDaySchedulesVersions
		{
			get => _sectorWeekDaySchedulesVersions;
			set => SetField(ref _sectorWeekDaySchedulesVersions, value);
		}

		private GenericObservableList<SectorWeekDayScheduleVersion> _observableSectorWeekDayRulesVersions;

		public virtual GenericObservableList<SectorWeekDayScheduleVersion> ObservableSectorWeekDayScheduleVersions =>
			_observableSectorWeekDayRulesVersions ??
			(_observableSectorWeekDayRulesVersions = new GenericObservableList<SectorWeekDayScheduleVersion>(SectorWeekDaySchedulesVersions));

		private IList<SectorWeekDayDeliveryRuleVersion> _sectorWeekDayDeliveryRuleVersions = new List<SectorWeekDayDeliveryRuleVersion>();

		public virtual IList<SectorWeekDayDeliveryRuleVersion> SectorWeekDayDeliveryRuleVersions
		{
			get => _sectorWeekDayDeliveryRuleVersions;
			set => SetField(ref _sectorWeekDayDeliveryRuleVersions, value);
		}

		private GenericObservableList<SectorWeekDayDeliveryRuleVersion> _observableSectorWeekDayDeliveryRules;

		public virtual GenericObservableList<SectorWeekDayDeliveryRuleVersion> ObservableSectorWeekDayDeliveryRuleVersions =>
			_observableSectorWeekDayDeliveryRules ??
			(_observableSectorWeekDayDeliveryRules = new GenericObservableList<SectorWeekDayDeliveryRuleVersion>(SectorWeekDayDeliveryRuleVersions));

		private IList<DeliveryPointSectorVersion> _deliveryPointSectorVersions = new List<DeliveryPointSectorVersion>();

		public virtual IList<DeliveryPointSectorVersion> DeliveryPointSectorVersions
		{
			get => _deliveryPointSectorVersions;
			set => SetField(ref _deliveryPointSectorVersions, value);
		}

		private GenericObservableList<DeliveryPointSectorVersion> _observableDeliveryPointSectorVersions;

		public virtual GenericObservableList<DeliveryPointSectorVersion> ObservableDeliveryPointSectorVersions =>
			_observableDeliveryPointSectorVersions ??
			(_observableDeliveryPointSectorVersions = new GenericObservableList<DeliveryPointSectorVersion>(DeliveryPointSectorVersions));

		public virtual SectorVersion GetActiveSectorVersionOnDate(DateTime? date = null)
		{
			if(date.HasValue)
			{
				return ObservableSectorVersions.SingleOrDefault(x =>
					(x.Status == SectorsSetStatus.Active || x.Status == SectorsSetStatus.Closed) 
					&& x.StartDate <= date && (x.EndDate == null || x.EndDate <= date));
			}

			return ObservableSectorVersions.SingleOrDefault(x => x.Status == SectorsSetStatus.Active);
		}

		public virtual SectorDeliveryRuleVersion GetActiveDeliveryRuleVersionOnDate(DateTime? date = null)
		{
			if(date.HasValue)
			{
				return ObservableSectorDeliveryRuleVersions.SingleOrDefault(x =>
					(x.Status == SectorsSetStatus.Active || x.Status == SectorsSetStatus.Closed)
					&& x.StartDate <= date && (x.EndDate == null || x.EndDate <= date));
			}

			return ObservableSectorDeliveryRuleVersions.SingleOrDefault(x =>
				x.StartDate <= DateTime.Now.Date && (x.EndDate == null || x.EndDate <= DateTime.Now.Date.AddDays(1)));
		}

		public virtual SectorWeekDayScheduleVersion GetActiveWeekDayScheduleVersionOnDate(DateTime? date = null)
		{
			if(date.HasValue)
			{
				return ObservableSectorWeekDayScheduleVersions.SingleOrDefault(x =>
					(x.Status == SectorsSetStatus.Active || x.Status == SectorsSetStatus.Closed)
					&& x.StartDate <= date && (x.EndDate == null || x.EndDate <= date));
			}

			return ObservableSectorWeekDayScheduleVersions.SingleOrDefault(x =>
				x.StartDate <= DateTime.Now.Date && (x.EndDate == null || x.EndDate <= DateTime.Now.Date.AddDays(1)));
		}

		public virtual SectorWeekDayDeliveryRuleVersion GetActiveWeekDayDeliveryRuleVersionOnDate(DateTime? date = null)
		{
			if(date.HasValue)
			{
				return ObservableSectorWeekDayDeliveryRuleVersions.SingleOrDefault(x =>
					(x.Status == SectorsSetStatus.Active || x.Status == SectorsSetStatus.Closed)
					&& x.StartDate <= date && (x.EndDate == null || x.EndDate <= date));
			}

			return ObservableSectorWeekDayDeliveryRuleVersions.SingleOrDefault(x =>
				x.StartDate <= DateTime.Now.Date && (x.EndDate == null || x.EndDate <= DateTime.Now.Date.AddDays(1)));
		}

		public virtual DeliveryPointSectorVersion GetActiveDeliveryPointVersionOnDate(DateTime? date = null)
		{
			if(date.HasValue)
			{
				return ObservableDeliveryPointSectorVersions.SingleOrDefault(x =>
					(x.Status == SectorsSetStatus.Active || x.Status == SectorsSetStatus.Closed)
					&& x.StartDate <= date && (x.EndDate == null || x.EndDate <= date));
			}

			return ObservableDeliveryPointSectorVersions.SingleOrDefault(x =>
				x.StartDate <= DateTime.Now.Date && (x.EndDate == null || x.EndDate <= DateTime.Now.Date.AddDays(1)));
		}
		#endregion


		/// <summary>
		///	Приоритет от максимального:
		///	1) Правила доставки на сегодня
		/// 2) Правила доставки на текущий день недели
		/// 3) Правила доставки района
		/// </summary>
		public virtual decimal GetDeliveryPrice(OrderStateKey orderStateKey, decimal eShopGoodsSum, DateTime? startDate)
		{
			if(orderStateKey.Order.DeliveryDate.HasValue)
			{
				if(orderStateKey.Order.DeliveryDate.Value.Date == DateTime.Today
				   && (GetActiveWeekDayScheduleVersionOnDate(startDate).SectorSchedules.Any(x=>x.WeekDay == WeekDayName.Today)
				       || GetActiveWeekDayDeliveryRuleVersionOnDate(startDate).WeekDayDistrictRules.Any(y=>y.WeekDay == WeekDayName.Today))) {
					var todayDeliveryRules = GetActiveWeekDayDeliveryRuleVersionOnDate(startDate).WeekDayDistrictRules.Where(x => orderStateKey.CompareWithDeliveryPriceRule(x.DeliveryPriceRule)).ToList();

					if (todayDeliveryRules.Any())
					{
						var todayMinEShopGoodsSum =
							todayDeliveryRules.Max(x => x.DeliveryPriceRule.OrderMinSumEShopGoods);

						if(eShopGoodsSum < todayMinEShopGoodsSum || todayMinEShopGoodsSum == 0)
							return todayDeliveryRules.Max(x => x.Price);
					}
					return 0m;
				}
				var dayOfWeekRules = GetActiveWeekDayDeliveryRuleVersionOnDate(startDate).WeekDayDistrictRules.Where(x=> x.WeekDay == ConvertDayOfWeekToWeekDayName(orderStateKey.Order.DeliveryDate.Value.DayOfWeek));
				if(dayOfWeekRules.Any()) {
					var dayOfWeekDeliveryRules = dayOfWeekRules.Where(x => orderStateKey.CompareWithDeliveryPriceRule(x.DeliveryPriceRule)).ToList();

					if (dayOfWeekDeliveryRules.Any())
					{
						var dayOfWeekEShopGoodsSum =
							dayOfWeekDeliveryRules.Max(x => x.DeliveryPriceRule.OrderMinSumEShopGoods);

						if(eShopGoodsSum < dayOfWeekEShopGoodsSum || dayOfWeekEShopGoodsSum == 0)
							return dayOfWeekDeliveryRules.Max(x => x.Price);
					}

					return 0m;
				}
			}
			var commonDeliveryRules =
				GetActiveDeliveryRuleVersionOnDate(startDate).ObservableCommonDistrictRuleItems.Where(x => orderStateKey.CompareWithDeliveryPriceRule(x.DeliveryPriceRule)).ToList();

			if (commonDeliveryRules.Any())
			{
				var commonMinEShopGoodsSum = commonDeliveryRules.Max(x => x.DeliveryPriceRule.OrderMinSumEShopGoods);

				if(eShopGoodsSum < commonMinEShopGoodsSum || commonMinEShopGoodsSum == 0)
					return commonDeliveryRules.Max(x => x.Price);
			}

			return 0m;
		}

		public static WeekDayName ConvertDayOfWeekToWeekDayName(DayOfWeek dayOfWeek)
		{
			switch (dayOfWeek) {
				case DayOfWeek.Monday: return WeekDayName.Monday;
				case DayOfWeek.Tuesday: return WeekDayName.Tuesday;
				case DayOfWeek.Wednesday: return WeekDayName.Wednesday;
				case DayOfWeek.Thursday: return WeekDayName.Thursday;
				case DayOfWeek.Friday: return WeekDayName.Friday;
				case DayOfWeek.Saturday: return WeekDayName.Saturday;
				case DayOfWeek.Sunday: return WeekDayName.Sunday;
				default: throw new ArgumentOutOfRangeException();
			}
		}

		public virtual bool HaveRestrictions(DateTime? activationTime = null) => GetActiveWeekDayScheduleVersionOnDate(activationTime).ObservableDeliveryScheduleRestriction.Any();

		public virtual string GetSchedulesString(bool withMarkup = false, DateTime? activationTime = null)
		{
			var result = new StringBuilder();
			var observableDeliveryScheduleRestriction =
				GetActiveWeekDayScheduleVersionOnDate(activationTime).ObservableDeliveryScheduleRestriction;
			var observableWeedDayDelivery = GetActiveWeekDayDeliveryRuleVersionOnDate(activationTime)?.ObservableWeekDayDistrictRules;
			foreach (var deliveryScheduleRestriction in observableDeliveryScheduleRestriction.GroupBy(x => x.WeekDay).OrderBy(x => (int)x.Key)) {
				var weekName = deliveryScheduleRestriction.Key.GetEnumTitle();
				result.Append(withMarkup ? $"<u><b>{weekName}</b></u>" : weekName);
				var weekRules = observableWeedDayDelivery?.Where(x => x.WeekDay == deliveryScheduleRestriction.Key);
				var commonDistricts = GetActiveDeliveryRuleVersionOnDate(activationTime).ObservableCommonDistrictRuleItems;
				if(weekRules != null && weekRules.Any())
				{
					result.AppendLine("\nцена: " + weekRules.Select(x => x.Price).Min());
					result.AppendLine("минимум 19л: " + weekRules.Select(x => x.DeliveryPriceRule.Water19LCount).Min());
					result.AppendLine("минимум 6л: " + weekRules.Select(x => x.DeliveryPriceRule.Water6LCount).Min());
					result.AppendLine("минимум 1,5л: " + weekRules.Select(x => x.DeliveryPriceRule.Water1500mlCount).Min());
					result.AppendLine("минимум 0,6л: " + weekRules.Select(x => x.DeliveryPriceRule.Water600mlCount).Min());
					result.AppendLine("минимум 0,5л: " + weekRules.Select(x => x.DeliveryPriceRule.Water500mlCount).Min());
				}
				else if(commonDistricts.Any())
				{
					result.AppendLine("\nцена: " + commonDistricts
						.Select(x => x.Price).Min());
					result.AppendLine("минимум 19л: " + commonDistricts
						.Select(x => x.DeliveryPriceRule.Water19LCount).Min());
					result.AppendLine("минимум 6л: " + commonDistricts
						.Select(x => x.DeliveryPriceRule.Water6LCount).Min());
					result.AppendLine("минимум 1,5л: " + commonDistricts
						.Select(x => x.DeliveryPriceRule.Water1500mlCount).Min());
					result.AppendLine("минимум 0,6л: " + commonDistricts
						.Select(x => x.DeliveryPriceRule.Water600mlCount).Min());
					result.AppendLine("минимум 0,5л: " + commonDistricts
						.Select(x => x.DeliveryPriceRule.Water500mlCount).Min());
				}
				else
					result.AppendLine();

				if(deliveryScheduleRestriction.Key == WeekDayName.Today) {
					var groupedRestrictions = deliveryScheduleRestriction
						.Where(x => x.AcceptBefore != null)
						.GroupBy(x => x.AcceptBefore.Name)
						.OrderBy(x => x.Key);

					foreach (var group in groupedRestrictions) {
						result.Append(withMarkup ? $"<b>до {group.Key}:</b> " : $"до {group.Key}: ");

						int i = 1;
						int maxScheduleCountOnLine = 3;
						var restrictions = group.OrderBy(x => x.DeliverySchedule.From).ThenBy(x => x.DeliverySchedule.To).ToList();
						int lastItemId = restrictions.Last().Id;
						foreach (var restriction in restrictions) {
							result.Append(restriction.DeliverySchedule.Name);
							result.Append(restriction.Id == lastItemId ? ";" : ", ");
							if(i == maxScheduleCountOnLine && restriction.Id != lastItemId) {
								result.AppendLine();
								maxScheduleCountOnLine = 4;
								i = 0;
							}
							i++;
						}
						result.AppendLine();
					}
				}
				else {
					var restrictions = deliveryScheduleRestriction.OrderBy(x => x.DeliverySchedule.From).ThenBy(x => x.DeliverySchedule.To).ToList();
					int maxScheduleCountOnLine = 4;
					int i = 1;
					int lastItemId = restrictions.Last().Id;
					foreach (var restriction in restrictions) {
						result.Append(restriction.DeliverySchedule.Name);
						result.Append(restriction.Id == lastItemId ? ";" : ", ");
						if(i == maxScheduleCountOnLine && restriction.Id != lastItemId) {
							result.AppendLine();
							i = 0;
						}
						i++;
					}
					result.AppendLine();
				}
				result.AppendLine();
			}
			//Удаление лишних переносов строк
			result.Length -= 2;
			return result.ToString();
		}
	}

	public enum SectorsSetStatus
	{
		[Display(Name = "Черновик")]
		Draft,
		[Display(Name = "На активации")]
		OnActivation,
		[Display(Name = "Активна")]
		Active,
		[Display(Name = "Закрыта")]
		Closed
	}
	public class SectorsSetStatusStringType : NHibernate.Type.EnumStringType {
		public SectorsSetStatusStringType() : base(typeof(SectorsSetStatus)) { }
	}
}
