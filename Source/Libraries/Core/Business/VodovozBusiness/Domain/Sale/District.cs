using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using System.Text;
using Gamma.Utilities;
using NetTopologySuite.Geometries;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Tools.Orders;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "районы",
		Nominative = "район")]
	[EntityPermission]
	[HistoryTrace]
	public class District : BusinessObjectBase<District>, IDomainObject, IValidatableObject, ICloneable
	{
		#region Свойства
		public virtual int Id { get; set; }

		string districtName;
		[Display(Name = "Название района")]
		public virtual string DistrictName {
			get => districtName;
			set => SetField(ref districtName, value, () => DistrictName);
		}

		private TariffZone tariffZone;
		[Display(Name = "Тарифная зоны")]
		public virtual TariffZone TariffZone {
			get => tariffZone;
			set => SetField(ref tariffZone, value, () => TariffZone);
		}

		private Geometry districtBorder;
		[Display(Name = "Граница")]
		public virtual Geometry DistrictBorder {
			get => districtBorder;
			set => SetField(ref districtBorder, value, () => DistrictBorder);
		}
		
		int minBottles;
		[Display(Name = "Минимальное количество бутылей")]
		public virtual int MinBottles {
			get => minBottles;
			set => SetField(ref minBottles, value, () => MinBottles);
		}

		private decimal waterPrice;
		[Display(Name = "Цена на воду")]
		public virtual decimal WaterPrice {
			get => waterPrice;
			set => SetField(ref waterPrice, value, () => WaterPrice);
		}

		private DistrictWaterPrice priceType;
		[Display(Name = "Вид цены")]
		public virtual DistrictWaterPrice PriceType {
			get => priceType;
			set {
				SetField(ref priceType, value, () => PriceType);
				if(WaterPrice != 0 && PriceType != DistrictWaterPrice.FixForDistrict)
					WaterPrice = 0;
			}
		}

		private WageDistrict wageDistrict;
		[Display(Name = "Группа района для расчёта ЗП")]
		public virtual WageDistrict WageDistrict {
			get => wageDistrict;
			set => SetField(ref wageDistrict, value);
		}
		
		private GeoGroup geographicGroup;
		[Display(Name = "Часть города")]
		public virtual GeoGroup GeographicGroup {
			get => geographicGroup;
			set => SetField(ref geographicGroup, value, () => GeographicGroup);
		}
		
		private DistrictsSet districtsSet;
		[Display(Name = "Версия районов")]
		public virtual DistrictsSet DistrictsSet {
			get => districtsSet;
			set => SetField(ref districtsSet, value, () => DistrictsSet);
		}
		
		private District copyOf;
		[Display(Name = "Копия района")]
		public virtual District CopyOf {
			get => copyOf;
			set => SetField(ref copyOf, value, () => CopyOf);
		}
		
		private District copiedTo;
		[Display(Name = "Район, скопированный из этого района")]
		public virtual District CopiedTo {
			get => copiedTo;
			set => SetField(ref copiedTo, value);
		}
		
		#endregion

		#region CommonDistrictRuleItems

		private IList<CommonDistrictRuleItem> commonDistrictRuleItems = new List<CommonDistrictRuleItem>();
		[Display(Name = "Правила и цены доставки района")]
		public virtual IList<CommonDistrictRuleItem> CommonDistrictRuleItems {
			get => commonDistrictRuleItems;
			set => SetField(ref commonDistrictRuleItems, value, () => CommonDistrictRuleItems);
		}

		private GenericObservableList<CommonDistrictRuleItem> observableCommonDistrictRuleItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<CommonDistrictRuleItem> ObservableCommonDistrictRuleItems =>
			observableCommonDistrictRuleItems ?? (observableCommonDistrictRuleItems =
				new GenericObservableList<CommonDistrictRuleItem>(CommonDistrictRuleItems));

		#endregion

		#region WeekDayDistricRuleItems
		
		private IList<WeekDayDistrictRuleItem> todayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
		public virtual IList<WeekDayDistrictRuleItem> TodayDistrictRuleItems {
			get => todayDistrictRuleItems;
			set => SetField(ref todayDistrictRuleItems, value, () => TodayDistrictRuleItems);
		}

		private GenericObservableList<WeekDayDistrictRuleItem> observableTodayDistrictRuleItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WeekDayDistrictRuleItem> ObservableTodayDistrictRuleItems =>
			observableTodayDistrictRuleItems ?? (observableTodayDistrictRuleItems =
				new GenericObservableList<WeekDayDistrictRuleItem>(TodayDistrictRuleItems));

		private IList<WeekDayDistrictRuleItem> mondayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
		public virtual IList<WeekDayDistrictRuleItem> MondayDistrictRuleItems {
			get => mondayDistrictRuleItems;
			set => SetField(ref mondayDistrictRuleItems, value, () => MondayDistrictRuleItems);
		}

		private GenericObservableList<WeekDayDistrictRuleItem> observableMondayDistrictRuleItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WeekDayDistrictRuleItem> ObservableMondayDistrictRuleItems =>
			observableMondayDistrictRuleItems ?? (observableMondayDistrictRuleItems =
				new GenericObservableList<WeekDayDistrictRuleItem>(MondayDistrictRuleItems));

		private IList<WeekDayDistrictRuleItem> tuesdayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
		public virtual IList<WeekDayDistrictRuleItem> TuesdayDistrictRuleItems {
			get => tuesdayDistrictRuleItems;
			set => SetField(ref tuesdayDistrictRuleItems, value, () => TuesdayDistrictRuleItems);
		}

		private GenericObservableList<WeekDayDistrictRuleItem> observableTuesdayDistrictRuleItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WeekDayDistrictRuleItem> ObservableTuesdayDistrictRuleItems =>
			observableTuesdayDistrictRuleItems ?? (observableTuesdayDistrictRuleItems =
				new GenericObservableList<WeekDayDistrictRuleItem>(TuesdayDistrictRuleItems));

		private IList<WeekDayDistrictRuleItem> wednesdayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
		public virtual IList<WeekDayDistrictRuleItem> WednesdayDistrictRuleItems {
			get => wednesdayDistrictRuleItems;
			set => SetField(ref wednesdayDistrictRuleItems, value, () => WednesdayDistrictRuleItems);
		}

		private GenericObservableList<WeekDayDistrictRuleItem> observableWednesdayDistrictRuleItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WeekDayDistrictRuleItem> ObservableWednesdayDistrictRuleItems =>
			observableWednesdayDistrictRuleItems ?? (observableWednesdayDistrictRuleItems =
				new GenericObservableList<WeekDayDistrictRuleItem>(WednesdayDistrictRuleItems));

		private IList<WeekDayDistrictRuleItem> thursdayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
		public virtual IList<WeekDayDistrictRuleItem> ThursdayDistrictRuleItems {
			get => thursdayDistrictRuleItems;
			set => SetField(ref thursdayDistrictRuleItems, value, () => ThursdayDistrictRuleItems);
		}

		private GenericObservableList<WeekDayDistrictRuleItem> observableThursdayDistrictRuleItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WeekDayDistrictRuleItem> ObservableThursdayDistrictRuleItems =>
			observableThursdayDistrictRuleItems ?? (observableThursdayDistrictRuleItems =
				new GenericObservableList<WeekDayDistrictRuleItem>(ThursdayDistrictRuleItems));

		private IList<WeekDayDistrictRuleItem> fridayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
		public virtual IList<WeekDayDistrictRuleItem> FridayDistrictRuleItems {
			get => fridayDistrictRuleItems;
			set => SetField(ref fridayDistrictRuleItems, value, () => FridayDistrictRuleItems);
		}

		private GenericObservableList<WeekDayDistrictRuleItem> observableFridayDistrictRuleItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WeekDayDistrictRuleItem> ObservableFridayDistrictRuleItems =>
			observableFridayDistrictRuleItems ?? (observableFridayDistrictRuleItems =
				new GenericObservableList<WeekDayDistrictRuleItem>(FridayDistrictRuleItems));

		private IList<WeekDayDistrictRuleItem> saturdayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
		public virtual IList<WeekDayDistrictRuleItem> SaturdayDistrictRuleItems {
			get => saturdayDistrictRuleItems;
			set => SetField(ref saturdayDistrictRuleItems, value, () => SaturdayDistrictRuleItems);
		}

		private GenericObservableList<WeekDayDistrictRuleItem> observableSaturdayDistrictRuleItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WeekDayDistrictRuleItem> ObservableSaturdayDistrictRuleItems =>
			observableSaturdayDistrictRuleItems ?? (observableSaturdayDistrictRuleItems =
				new GenericObservableList<WeekDayDistrictRuleItem>(SaturdayDistrictRuleItems));

		private IList<WeekDayDistrictRuleItem> sundayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
		public virtual IList<WeekDayDistrictRuleItem> SundayDistrictRuleItems {
			get => sundayDistrictRuleItems;
			set => SetField(ref sundayDistrictRuleItems, value, () => SundayDistrictRuleItems);
		}

		private GenericObservableList<WeekDayDistrictRuleItem> observableSundayDistrictRuleItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<WeekDayDistrictRuleItem> ObservableSundayDistrictRuleItems =>
			observableSundayDistrictRuleItems ?? (observableSundayDistrictRuleItems =
				new GenericObservableList<WeekDayDistrictRuleItem>(SundayDistrictRuleItems));

		#endregion
		
		#region DeliveryScheduleRestrictions
		
		private IList<DeliveryScheduleRestriction> todayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		public virtual IList<DeliveryScheduleRestriction> TodayDeliveryScheduleRestrictions {
			get => todayDeliveryScheduleRestrictions;
			set => SetField(ref todayDeliveryScheduleRestrictions, value, () => TodayDeliveryScheduleRestrictions);
		}

		private GenericObservableList<DeliveryScheduleRestriction> observableTodayDeliveryScheduleRestrictions;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableTodayDeliveryScheduleRestrictions =>
			observableTodayDeliveryScheduleRestrictions ?? (observableTodayDeliveryScheduleRestrictions =
				new GenericObservableList<DeliveryScheduleRestriction>(TodayDeliveryScheduleRestrictions));

		private IList<DeliveryScheduleRestriction> mondayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		public virtual IList<DeliveryScheduleRestriction> MondayDeliveryScheduleRestrictions {
			get => mondayDeliveryScheduleRestrictions;
			set => SetField(ref mondayDeliveryScheduleRestrictions, value, () => MondayDeliveryScheduleRestrictions);
		}

		private GenericObservableList<DeliveryScheduleRestriction> observableMondayDeliveryScheduleRestrictions;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableMondayDeliveryScheduleRestrictions =>
			observableMondayDeliveryScheduleRestrictions ?? (observableMondayDeliveryScheduleRestrictions =
				new GenericObservableList<DeliveryScheduleRestriction>(MondayDeliveryScheduleRestrictions));

		private IList<DeliveryScheduleRestriction> tuesdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		public virtual IList<DeliveryScheduleRestriction> TuesdayDeliveryScheduleRestrictions {
			get => tuesdayDeliveryScheduleRestrictions;
			set => SetField(ref tuesdayDeliveryScheduleRestrictions, value, () => TuesdayDeliveryScheduleRestrictions);
		}

		private GenericObservableList<DeliveryScheduleRestriction> observableTuesdayDeliveryScheduleRestrictions;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableTuesdayDeliveryScheduleRestrictions =>
			observableTuesdayDeliveryScheduleRestrictions ?? (observableTuesdayDeliveryScheduleRestrictions =
				new GenericObservableList<DeliveryScheduleRestriction>(TuesdayDeliveryScheduleRestrictions));

		private IList<DeliveryScheduleRestriction> wednesdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		public virtual IList<DeliveryScheduleRestriction> WednesdayDeliveryScheduleRestrictions {
			get => wednesdayDeliveryScheduleRestrictions;
			set => SetField(ref wednesdayDeliveryScheduleRestrictions, value, () => WednesdayDeliveryScheduleRestrictions);
		}

		private GenericObservableList<DeliveryScheduleRestriction> observableWednesdayDeliveryScheduleRestrictions;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableWednesdayDeliveryScheduleRestrictions =>
			observableWednesdayDeliveryScheduleRestrictions ?? (observableWednesdayDeliveryScheduleRestrictions =
				new GenericObservableList<DeliveryScheduleRestriction>(WednesdayDeliveryScheduleRestrictions));

		private IList<DeliveryScheduleRestriction> thursdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		public virtual IList<DeliveryScheduleRestriction> ThursdayDeliveryScheduleRestrictions {
			get => thursdayDeliveryScheduleRestrictions;
			set => SetField(ref thursdayDeliveryScheduleRestrictions, value, () => ThursdayDeliveryScheduleRestrictions);
		}

		private GenericObservableList<DeliveryScheduleRestriction> observableThursdayDeliveryScheduleRestrictions;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableThursdayDeliveryScheduleRestrictions =>
			observableThursdayDeliveryScheduleRestrictions ?? (observableThursdayDeliveryScheduleRestrictions =
				new GenericObservableList<DeliveryScheduleRestriction>(ThursdayDeliveryScheduleRestrictions));

		private IList<DeliveryScheduleRestriction> fridayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		public virtual IList<DeliveryScheduleRestriction> FridayDeliveryScheduleRestrictions {
			get => fridayDeliveryScheduleRestrictions;
			set => SetField(ref fridayDeliveryScheduleRestrictions, value, () => FridayDeliveryScheduleRestrictions);
		}

		private GenericObservableList<DeliveryScheduleRestriction> observableFridayDeliveryScheduleRestrictions;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableFridayDeliveryScheduleRestrictions =>
			observableFridayDeliveryScheduleRestrictions ?? (observableFridayDeliveryScheduleRestrictions =
				new GenericObservableList<DeliveryScheduleRestriction>(FridayDeliveryScheduleRestrictions));

		private IList<DeliveryScheduleRestriction> saturdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		public virtual IList<DeliveryScheduleRestriction> SaturdayDeliveryScheduleRestrictions {
			get => saturdayDeliveryScheduleRestrictions;
			set => SetField(ref saturdayDeliveryScheduleRestrictions, value, () => SaturdayDeliveryScheduleRestrictions);
		}

		private GenericObservableList<DeliveryScheduleRestriction> observableSaturdayDeliveryScheduleRestrictions;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableSaturdayDeliveryScheduleRestrictions =>
			observableSaturdayDeliveryScheduleRestrictions ?? (observableSaturdayDeliveryScheduleRestrictions =
				new GenericObservableList<DeliveryScheduleRestriction>(SaturdayDeliveryScheduleRestrictions));

		private IList<DeliveryScheduleRestriction> sundayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		public virtual IList<DeliveryScheduleRestriction> SundayDeliveryScheduleRestrictions {
			get => sundayDeliveryScheduleRestrictions;
			set => SetField(ref sundayDeliveryScheduleRestrictions, value, () => SundayDeliveryScheduleRestrictions);
		}

		private GenericObservableList<DeliveryScheduleRestriction> observableSundayDeliveryScheduleRestrictions;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableSundayDeliveryScheduleRestrictions =>
			observableSundayDeliveryScheduleRestrictions ?? (observableSundayDeliveryScheduleRestrictions =
				new GenericObservableList<DeliveryScheduleRestriction>(SundayDeliveryScheduleRestrictions));

		#endregion

		#region Функции

		public virtual string Title => DistrictName;
		
		public virtual bool HaveRestrictions => GetAllDeliveryScheduleRestrictions().Any();

		public virtual string GetSchedulesString(bool withMarkup = false)
		{
			var result = new StringBuilder();
			foreach (var deliveryScheduleRestriction in GetAllDeliveryScheduleRestrictions().GroupBy(x => x.WeekDay).OrderBy(x => (int)x.Key)) {
				var weekName = deliveryScheduleRestriction.Key.GetEnumTitle();
				result.Append(withMarkup ? $"<u><b>{weekName}</b></u>" : weekName);
				var weekRules = GetWeekDayRuleItemCollectionByWeekDayName(deliveryScheduleRestriction.Key);
				if(weekRules.Any())
				{
					result.AppendLine("\nцена: " + weekRules.Select(x => x.Price).Min());
					result.AppendLine("минимум 19л: " + weekRules.Select(x => x.DeliveryPriceRule.Water19LCount).Min());
					result.AppendLine("минимум 6л: " + weekRules.Select(x => x.DeliveryPriceRule.Water6LCount).Min());
					result.AppendLine("минимум 1,5л: " + weekRules.Select(x => x.DeliveryPriceRule.Water1500mlCount).Min());
					result.AppendLine("минимум 0,5л: " + weekRules.Select(x => x.DeliveryPriceRule.Water500mlCount).Min());
				}
				else if(ObservableCommonDistrictRuleItems.Any())
				{
					result.AppendLine("\nцена: " + ObservableCommonDistrictRuleItems
						.Select(x => x.Price).Min());
					result.AppendLine("минимум 19л: " + ObservableCommonDistrictRuleItems
						.Select(x => x.DeliveryPriceRule.Water19LCount).Min());
					result.AppendLine("минимум 6л: " + ObservableCommonDistrictRuleItems
						.Select(x => x.DeliveryPriceRule.Water6LCount).Min());
					result.AppendLine("минимум 1,5л: " + ObservableCommonDistrictRuleItems
						.Select(x => x.DeliveryPriceRule.Water1500mlCount).Min());
					result.AppendLine("минимум 0,5л: " + ObservableCommonDistrictRuleItems
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

		public virtual IEnumerable<DeliveryScheduleRestriction> GetAllDeliveryScheduleRestrictions()
		{
			return TodayDeliveryScheduleRestrictions
					.Concat(MondayDeliveryScheduleRestrictions)
					.Concat(TuesdayDeliveryScheduleRestrictions)
					.Concat(WednesdayDeliveryScheduleRestrictions)
					.Concat(ThursdayDeliveryScheduleRestrictions)
					.Concat(FridayDeliveryScheduleRestrictions)
					.Concat(SaturdayDeliveryScheduleRestrictions)
					.Concat(SundayDeliveryScheduleRestrictions);
		}
		
		public virtual IEnumerable<WeekDayDistrictRuleItem> GetAllWeekDayDistrictRuleItems()
		{
			return TodayDistrictRuleItems
					.Concat(MondayDistrictRuleItems)
					.Concat(TuesdayDistrictRuleItems)
					.Concat(WednesdayDistrictRuleItems)
					.Concat(ThursdayDistrictRuleItems)
					.Concat(FridayDistrictRuleItems)
					.Concat(SaturdayDistrictRuleItems)
					.Concat(SundayDistrictRuleItems);
		}

		public virtual GenericObservableList<WeekDayDistrictRuleItem> GetWeekDayRuleItemCollectionByWeekDayName(WeekDayName weekDayName)
		{
			switch (weekDayName) {
				case WeekDayName.Today: return ObservableTodayDistrictRuleItems;
				case WeekDayName.Monday: return ObservableMondayDistrictRuleItems;
				case WeekDayName.Tuesday: return ObservableTuesdayDistrictRuleItems;
				case WeekDayName.Wednesday: return ObservableWednesdayDistrictRuleItems;
				case WeekDayName.Thursday: return ObservableThursdayDistrictRuleItems;
				case WeekDayName.Friday: return ObservableFridayDistrictRuleItems;
				case WeekDayName.Saturday: return ObservableSaturdayDistrictRuleItems;
				case WeekDayName.Sunday: return ObservableSundayDistrictRuleItems;
				default: throw new ArgumentOutOfRangeException();
			}
		}
		
		public virtual GenericObservableList<DeliveryScheduleRestriction> GetScheduleRestrictionCollectionByWeekDayName(WeekDayName weekDayName)
		{
			switch (weekDayName) {
				case WeekDayName.Today: return ObservableTodayDeliveryScheduleRestrictions;
				case WeekDayName.Monday: return ObservableMondayDeliveryScheduleRestrictions;
				case WeekDayName.Tuesday: return ObservableTuesdayDeliveryScheduleRestrictions;
				case WeekDayName.Wednesday: return ObservableWednesdayDeliveryScheduleRestrictions;
				case WeekDayName.Thursday: return ObservableThursdayDeliveryScheduleRestrictions;
				case WeekDayName.Friday: return ObservableFridayDeliveryScheduleRestrictions;
				case WeekDayName.Saturday: return ObservableSaturdayDeliveryScheduleRestrictions;
				case WeekDayName.Sunday: return ObservableSundayDeliveryScheduleRestrictions;
				default: throw new ArgumentOutOfRangeException();
			}
		}

		private void ClearAllDeliveryScheduleRestrictions()
		{
			TodayDeliveryScheduleRestrictions.Clear();
			MondayDeliveryScheduleRestrictions.Clear();
			TuesdayDeliveryScheduleRestrictions.Clear();
			WednesdayDeliveryScheduleRestrictions.Clear();
			ThursdayDeliveryScheduleRestrictions.Clear();
			FridayDeliveryScheduleRestrictions.Clear();
			SaturdayDeliveryScheduleRestrictions.Clear();
			SundayDeliveryScheduleRestrictions.Clear();
		}

		public virtual void ReplaceDistrictDeliveryScheduleRestrictions(IEnumerable<DeliveryScheduleRestriction> deliveryScheduleRestrictions)
		{
			if(deliveryScheduleRestrictions == null)
			{
				throw new ArgumentException(
					 "Отсутствуют данные новых графиков доставки");
			}

			if(deliveryScheduleRestrictions.Any(s => s.District.Id != Id))
			{
				throw new ArgumentException(
					 "Id района в который добавляется график доставки должен совпадать с Id района в новом графике доставки");
			}

			ClearAllDeliveryScheduleRestrictions();

			foreach(var schedule in  deliveryScheduleRestrictions)
			{
				switch(schedule.WeekDay)
				{
					case WeekDayName.Today:
						ObservableTodayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Monday:
						ObservableMondayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Tuesday:
						ObservableTuesdayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Wednesday:
						ObservableWednesdayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Thursday:
						ObservableThursdayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Friday:
						ObservableFridayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Saturday:
						ObservableSaturdayDeliveryScheduleRestrictions.Add(schedule);
						break;
					case WeekDayName.Sunday:
						ObservableSundayDeliveryScheduleRestrictions.Add(schedule);
						break;
					default: throw new ArgumentOutOfRangeException();
				}
			}
		}

		/// <summary>
		///	Приоритет от максимального:
		///	1) Правила доставки на сегодня
		/// 2) Правила доставки на текущий день недели
		/// 3) Правила доставки района
		/// </summary>
		public virtual decimal GetDeliveryPrice(OrderStateKey orderStateKey, decimal eShopGoodsSum)
		{
			if(orderStateKey.Order.DeliveryDate.HasValue) {
				if(orderStateKey.Order.DeliveryDate.Value.Date == DateTime.Today && ObservableTodayDistrictRuleItems.Any()) {
					var todayDeliveryRules = ObservableTodayDistrictRuleItems.Where(x => orderStateKey.CompareWithDeliveryPriceRule(x.DeliveryPriceRule)).ToList();

					if (todayDeliveryRules.Any())
					{
						var todayMinEShopGoodsSum =
							todayDeliveryRules.Max(x => x.DeliveryPriceRule.OrderMinSumEShopGoods);
						
						if(eShopGoodsSum < todayMinEShopGoodsSum || todayMinEShopGoodsSum == 0)
							return todayDeliveryRules.Max(x => x.Price);
					}
					return 0m;
				}
				var dayOfWeekRules = GetWeekDayRuleItemCollectionByWeekDayName(ConvertDayOfWeekToWeekDayName(orderStateKey.Order.DeliveryDate.Value.DayOfWeek));
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
				CommonDistrictRuleItems.Where(x => orderStateKey.CompareWithDeliveryPriceRule(x.DeliveryPriceRule)).ToList();
			
			if (commonDeliveryRules.Any())
			{
				var commonMinEShopGoodsSum = commonDeliveryRules.Max(x => x.DeliveryPriceRule.OrderMinSumEShopGoods);
				
				if(eShopGoodsSum < commonMinEShopGoodsSum || commonMinEShopGoodsSum == 0)
					return commonDeliveryRules.Max(x => x.Price);
			}
					
			return 0m;
		}

		private void InitializeAllCollections()
		{
			CommonDistrictRuleItems = new List<CommonDistrictRuleItem>();
			
			TodayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
			MondayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
			TuesdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
			WednesdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
			ThursdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
			FridayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
			SaturdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
			SundayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
			
			TodayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
			MondayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
			TuesdayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
			WednesdayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
			ThursdayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
			FridayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
			SaturdayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
			SundayDistrictRuleItems = new List<WeekDayDistrictRuleItem>();
		}

		/// <summary>
		/// Активная ли версия районов, связанная с этим районом
		/// </summary>
		public virtual bool IsActive => DistrictsSet?.Status == DistrictsSetStatus.Active;

		#endregion

		#region IValidatableObject implementation
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(String.IsNullOrWhiteSpace(DistrictName)) {
				yield return new ValidationResult(
					"Необходимо заполнить имя района",
					new[] { this.GetPropertyName(o => o.DistrictName) }
				);
			}
			if(GeographicGroup == null) {
				yield return new ValidationResult(
					$"Для района \"{DistrictName}\" необходимо указать часть города, содержащую этот район доставки",
					new[] { this.GetPropertyName(o => o.GeographicGroup) }
				);
			}
			if(DistrictBorder == null) {
				yield return new ValidationResult(
					$"Для района \"{DistrictName}\" необходимо нарисовать границы на карте",
					new[] { this.GetPropertyName(o => o.DistrictBorder) }
				);
			}
			if(WageDistrict == null) {
				yield return new ValidationResult(
					$"Для района \"{DistrictName}\" необходимо выбрать зарплатную группу",
					new[] { this.GetPropertyName(o => o.WageDistrict) }
				);
			}
			if(ObservableCommonDistrictRuleItems.Any(i => i.Price <= 0)) {
				yield return new ValidationResult(
					$"Для всех правил доставки для района \"{DistrictName}\" должны быть указаны цены",
					new[] { this.GetPropertyName(o => o.CommonDistrictRuleItems) }
				);
			}
			if(GetAllWeekDayDistrictRuleItems().Any(i => i.Price <= 0)) {
				yield return new ValidationResult(
					$"Для всех особых правил доставки для района \"{DistrictName}\" должны быть указаны цены"
				);
			}
			if(ObservableTodayDeliveryScheduleRestrictions.Any(i => i.AcceptBefore == null)) {
				yield return new ValidationResult(
					$"Для графиков доставки \"день в день\" для района \"{DistrictName}\" должно быть указано время приема до"
				);
			}
		}
		
		#endregion

		#region ICloneable implementation

		public virtual object Clone()
		{
			var newDistrict = new District {
				DistrictName = DistrictName,
				DistrictBorder = DistrictBorder?.Copy(),
				WageDistrict = WageDistrict,
				GeographicGroup = GeographicGroup,
				PriceType = PriceType,
				MinBottles = MinBottles,
				TariffZone = TariffZone,
				WaterPrice = WaterPrice
			};
			newDistrict.InitializeAllCollections();

			foreach (var commonRuleItem in CommonDistrictRuleItems) {
				var newCommonRuleItem = (CommonDistrictRuleItem)commonRuleItem.Clone();
				newCommonRuleItem.District = newDistrict;
				newDistrict.CommonDistrictRuleItems
					.Add(newCommonRuleItem);
			}
			foreach (var scheduleRestriction in GetAllDeliveryScheduleRestrictions()) {
				var newScheduleRestriction = (DeliveryScheduleRestriction)scheduleRestriction.Clone();
				newScheduleRestriction.District = newDistrict;
				newDistrict.GetScheduleRestrictionCollectionByWeekDayName(scheduleRestriction.WeekDay)
					.Add(newScheduleRestriction);
			}
			foreach (var weekDayRule in GetAllWeekDayDistrictRuleItems()) {
				var newWeekDayRule = (WeekDayDistrictRuleItem)weekDayRule.Clone();
				newWeekDayRule.District = newDistrict;
				newDistrict.GetWeekDayRuleItemCollectionByWeekDayName(weekDayRule.WeekDay)
					.Add(newWeekDayRule);
			}
			
			return newDistrict;
		}

		#endregion
		
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

		public static District GetDistrictFromActiveDistrictsSetOrNull(District district)
		{
			while(true) {
				if(district?.DistrictsSet == null) {
					return null;
				}
				if(district.DistrictsSet.Status == DistrictsSetStatus.Active) {
					return district;
				}
				district = district.CopiedTo;
			}
		}
	}
}
