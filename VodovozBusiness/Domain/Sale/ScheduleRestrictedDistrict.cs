using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using GeoAPI.Geometries;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using Vodovoz.Tools.Orders;

namespace Vodovoz.Domain.Sale
{
	public class ScheduleRestrictedDistrict : BusinessObjectBase<ScheduleRestrictedDistrict>, IDomainObject, IValidatableObject
	{
		#region Свойства
		public virtual int Id { get; set; }

		string districtName;

		public virtual string DistrictName {
			get { return districtName; }
			set { SetField(ref districtName, value, () => DistrictName); }
		}

		int minBottles;

		public virtual int MinBottles {
			get { return minBottles; }
			set { SetField(ref minBottles, value, () => MinBottles); }
		}

		public virtual bool HaveRestrictions {
			get {
				return
					(ScheduleRestrictionMonday != null && ScheduleRestrictionMonday.Schedules.Any()) ||
					(ScheduleRestrictionTuesday != null && ScheduleRestrictionTuesday.Schedules.Any()) ||
					(ScheduleRestrictionWednesday != null && ScheduleRestrictionWednesday.Schedules.Any()) ||
					(ScheduleRestrictionThursday != null && ScheduleRestrictionThursday.Schedules.Any()) ||
					(ScheduleRestrictionFriday != null && ScheduleRestrictionFriday.Schedules.Any()) ||
					(ScheduleRestrictionSaturday != null && ScheduleRestrictionSaturday.Schedules.Any()) ||
					(ScheduleRestrictionSunday != null && ScheduleRestrictionSunday.Schedules.Any());
			}
		}

		private ScheduleRestriction scheduleRestrictionMonday;

		public virtual ScheduleRestriction ScheduleRestrictionMonday {
			get { return scheduleRestrictionMonday; }
			set { SetField(ref scheduleRestrictionMonday, value, () => ScheduleRestrictionMonday); }
		}

		private ScheduleRestriction scheduleRestrictionTuesday;

		public virtual ScheduleRestriction ScheduleRestrictionTuesday {
			get { return scheduleRestrictionTuesday; }
			set { SetField(ref scheduleRestrictionTuesday, value, () => ScheduleRestrictionTuesday); }
		}

		private ScheduleRestriction scheduleRestrictionWednesday;

		public virtual ScheduleRestriction ScheduleRestrictionWednesday {
			get { return scheduleRestrictionWednesday; }
			set { SetField(ref scheduleRestrictionWednesday, value, () => ScheduleRestrictionWednesday); }
		}

		private ScheduleRestriction scheduleRestrictionThursday;

		public virtual ScheduleRestriction ScheduleRestrictionThursday {
			get { return scheduleRestrictionThursday; }
			set { SetField(ref scheduleRestrictionThursday, value, () => ScheduleRestrictionThursday); }
		}

		private ScheduleRestriction scheduleRestrictionFriday;

		public virtual ScheduleRestriction ScheduleRestrictionFriday {
			get { return scheduleRestrictionFriday; }
			set { SetField(ref scheduleRestrictionFriday, value, () => ScheduleRestrictionFriday); }
		}

		private ScheduleRestriction scheduleRestrictionSaturday;

		public virtual ScheduleRestriction ScheduleRestrictionSaturday {
			get { return scheduleRestrictionSaturday; }
			set { SetField(ref scheduleRestrictionSaturday, value, () => ScheduleRestrictionSaturday); }
		}

		private ScheduleRestriction scheduleRestrictionSunday;

		public virtual ScheduleRestriction ScheduleRestrictionSunday {
			get { return scheduleRestrictionSunday; }
			set { SetField(ref scheduleRestrictionSunday, value, () => ScheduleRestrictionSunday); }
		}

		private IGeometry districtBorder;

		public virtual IGeometry DistrictBorder {
			get { return districtBorder; }
			set { SetField(ref districtBorder, value, () => DistrictBorder); }
		}

		private decimal waterPrice;

		[Display(Name = "Цена на воду")]
		public virtual decimal WaterPrice {
			get { return waterPrice; }
			set { SetField(ref waterPrice, value, () => WaterPrice); }
		}

		private DistrictWaterPrice priceType;

		[Display(Name = "Вид цены")]
		public virtual DistrictWaterPrice PriceType {
			get { return priceType; }
			set {
				SetField(ref priceType, value, () => PriceType);
				if(WaterPrice != 0 && PriceType != DistrictWaterPrice.FixForDistrict)
					WaterPrice = 0;
			}
		}

		IList<ScheduleRestrictedDistrictRuleItem> scheduleRestrictedDistrictRuleItems = new List<ScheduleRestrictedDistrictRuleItem>();
		[Display(Name = "Правила цены доставки")]
		public virtual IList<ScheduleRestrictedDistrictRuleItem> ScheduleRestrictedDistrictRuleItems {
			get { return scheduleRestrictedDistrictRuleItems; }
			set { SetField(ref scheduleRestrictedDistrictRuleItems, value, () => ScheduleRestrictedDistrictRuleItems); }
		}

		GenericObservableList<ScheduleRestrictedDistrictRuleItem> observableScheduleRestrictedDistrictRuleItems;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<ScheduleRestrictedDistrictRuleItem> ObservableScheduleRestrictedDistrictRuleItems {
			get {
				if(observableScheduleRestrictedDistrictRuleItems == null)
					observableScheduleRestrictedDistrictRuleItems = new GenericObservableList<ScheduleRestrictedDistrictRuleItem>(ScheduleRestrictedDistrictRuleItems);
				return observableScheduleRestrictedDistrictRuleItems;
			}
		}

		IList<GeographicGroup> geographicGroups = new List<GeographicGroup>();
		[Display(Name = "Группа района")]
		public virtual IList<GeographicGroup> GeographicGroups {
			get => geographicGroups;
			set => SetField(ref geographicGroups, value, () => GeographicGroups);
		}

		GenericObservableList<GeographicGroup> observableGeographicGroups;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<GeographicGroup> ObservableGeographicGroups {
			get {
				if(observableGeographicGroups == null)
					observableGeographicGroups = new GenericObservableList<GeographicGroup>(GeographicGroups);
				return observableGeographicGroups;
			}
		}
		#endregion

		#region Функции

		public virtual string GetSchedulesString()
		{
			string result = String.Empty;
			if(scheduleRestrictionMonday != null) {
				result += ScheduleRestrictionMonday.WeekDay.GetEnumTitle() + " " + ScheduleRestrictionMonday.SchedulesStr + "; ";
			}
			if(ScheduleRestrictionTuesday != null) {
				result += ScheduleRestrictionTuesday.WeekDay.GetEnumTitle() + " " + ScheduleRestrictionTuesday.SchedulesStr + "; ";
			}
			if(ScheduleRestrictionWednesday != null) {
				result += ScheduleRestrictionWednesday.WeekDay.GetEnumTitle() + " " + ScheduleRestrictionWednesday.SchedulesStr + "; ";
			}
			if(ScheduleRestrictionThursday != null) {
				result += ScheduleRestrictionThursday.WeekDay.GetEnumTitle() + " " + ScheduleRestrictionThursday.SchedulesStr + "; ";
			}
			if(ScheduleRestrictionFriday != null) {
				result += ScheduleRestrictionFriday.WeekDay.GetEnumTitle() + " " + ScheduleRestrictionFriday.SchedulesStr + "; ";
			}
			if(ScheduleRestrictionSaturday != null) {
				result += ScheduleRestrictionSaturday.WeekDay.GetEnumTitle() + " " + ScheduleRestrictionSaturday.SchedulesStr + "; ";
			}
			if(ScheduleRestrictionSunday != null) {
				result += ScheduleRestrictionSunday.WeekDay.GetEnumTitle() + " " + ScheduleRestrictionSunday.SchedulesStr + "; ";
			}
			return result;
		}

		public virtual void Save(IUnitOfWork UoW)
		{
			RemoveUnusedRestriction(UoW);
			UoW.Save(this);
		}

		public virtual void Remove(IUnitOfWork UoW)
		{
			UoW.Delete(this);
		}

		void RemoveUnusedRestriction(IUnitOfWork UoW)
		{
			if(ScheduleRestrictionMonday != null && !ScheduleRestrictionMonday.Schedules.Any()) {
				ScheduleRestrictionMonday.Remove(UoW);
				ScheduleRestrictionMonday = null;
			}
			if(ScheduleRestrictionTuesday != null && !ScheduleRestrictionTuesday.Schedules.Any()) {
				ScheduleRestrictionTuesday.Remove(UoW);
				ScheduleRestrictionTuesday = null;
			}
			if(ScheduleRestrictionWednesday != null && !ScheduleRestrictionWednesday.Schedules.Any()) {
				ScheduleRestrictionWednesday.Remove(UoW);
				ScheduleRestrictionWednesday = null;
			}
			if(ScheduleRestrictionThursday != null && !ScheduleRestrictionThursday.Schedules.Any()) {
				ScheduleRestrictionThursday.Remove(UoW);
				ScheduleRestrictionThursday = null;
			}
			if(ScheduleRestrictionFriday != null && !ScheduleRestrictionFriday.Schedules.Any()) {
				ScheduleRestrictionFriday.Remove(UoW);
				ScheduleRestrictionFriday = null;
			}
			if(ScheduleRestrictionSaturday != null && !ScheduleRestrictionSaturday.Schedules.Any()) {
				ScheduleRestrictionSaturday.Remove(UoW);
				ScheduleRestrictionSaturday = null;
			}
			if(ScheduleRestrictionSunday != null && !ScheduleRestrictionSunday.Schedules.Any()) {
				ScheduleRestrictionSunday.Remove(UoW);
				ScheduleRestrictionSunday = null;
			}
		}

		public virtual void CreateScheduleRestriction(WeekDayName weekday)
		{
			switch(weekday) {
				case WeekDayName.monday:
					if(ScheduleRestrictionMonday == null) {
						ScheduleRestrictionMonday = new ScheduleRestriction();
						ScheduleRestrictionMonday.WeekDay = weekday;
					}
					break;
				case WeekDayName.tuesday:
					if(ScheduleRestrictionTuesday == null) {
						ScheduleRestrictionTuesday = new ScheduleRestriction();
						ScheduleRestrictionTuesday.WeekDay = weekday;
					}
					break;
				case WeekDayName.wednesday:
					if(ScheduleRestrictionWednesday == null) {
						ScheduleRestrictionWednesday = new ScheduleRestriction();
						ScheduleRestrictionWednesday.WeekDay = weekday;
					}
					break;
				case WeekDayName.thursday:
					if(ScheduleRestrictionThursday == null) {
						ScheduleRestrictionThursday = new ScheduleRestriction();
						ScheduleRestrictionThursday.WeekDay = weekday;
					}
					break;
				case WeekDayName.friday:
					if(ScheduleRestrictionFriday == null) {
						ScheduleRestrictionFriday = new ScheduleRestriction();
						ScheduleRestrictionFriday.WeekDay = weekday;
					}
					break;
				case WeekDayName.saturday:
					if(ScheduleRestrictionSaturday == null) {
						ScheduleRestrictionSaturday = new ScheduleRestriction();
						ScheduleRestrictionSaturday.WeekDay = weekday;
					}
					break;
				case WeekDayName.sunday:
					if(ScheduleRestrictionSunday == null) {
						ScheduleRestrictionSunday = new ScheduleRestriction();
						ScheduleRestrictionSunday.WeekDay = weekday;
					}
					break;
			}
		}

		public virtual decimal GetDeliveryPrice(OrderStateKey orderStateKey)
		{
			//будет логичнее, если перенести метод CompareWithDeliveryPriceRule из OrderStateKey в ScheduleRestrictedDistrictRuleItem
			var ruleItems = ScheduleRestrictedDistrictRuleItems.Where(x => orderStateKey.CompareWithDeliveryPriceRule(x.DeliveryPriceRule));
			return ruleItems.Any() ? ruleItems.Max(x => x.DeliveryPrice) : 0m;
		}

		#endregion

		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(ObservableScheduleRestrictedDistrictRuleItems.Any(i => i.DeliveryPrice <= 0))
				yield return new ValidationResult(
					"Для всех правил доставки должны быть указаны цены",
					new[] { this.GetPropertyName(o => o.ScheduleRestrictedDistrictRuleItems) }
				);

			if(!GeographicGroups.Any())
				yield return new ValidationResult(
					string.Format("Для района \"{0}\" необходимо указать часть города, содержащую этот район доставки", DistrictName),
					new[] { this.GetPropertyName(o => o.GeographicGroups) }
				);
		}
		#endregion
	}
	public enum DistrictWaterPrice
	{
		[Display(Name = "По прайсу")]
		Standart,
		[Display(Name = "Специальная цена")]
		FixForDistrict,
		[Display(Name = "По расстоянию")]
		ByDistance,
	}

	public class DistrictWaterPriceStringType : NHibernate.Type.EnumStringType
	{
		public DistrictWaterPriceStringType() : base(typeof(DistrictWaterPrice)) { }
	}
}