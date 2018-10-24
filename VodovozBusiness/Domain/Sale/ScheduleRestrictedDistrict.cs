using System.ComponentModel.DataAnnotations;
using System.Linq;
using Gamma.Utilities;
using GeoAPI.Geometries;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;

namespace Vodovoz.Domain.Sale
{
	public class ScheduleRestrictedDistrict : PropertyChangedBase, IDomainObject
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

		public virtual bool HaveRestrictions{
			get{
				return 
					(ScheduleRestrictionMonday != null && ScheduleRestrictionMonday.Schedules.Any()) ||
					(scheduleRestrictionTuesday != null && scheduleRestrictionTuesday.Schedules.Any()) ||
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
			set { SetField(ref priceType, value, () => PriceType);
				if(WaterPrice != 0 && PriceType != DistrictWaterPrice.FixForDistrict)
					WaterPrice = 0;
			}
		}

		#endregion

		#region Функции

		public virtual string GetSchedulesString()
		{
			string result = "";
			if(scheduleRestrictionMonday != null) {
				result += ScheduleRestrictionMonday.WeekDay.GetEnumTitle() + " " + ScheduleRestrictionMonday.ShedulesStr + "; ";
			}
			if(ScheduleRestrictionTuesday != null) {
				result += ScheduleRestrictionTuesday.WeekDay.GetEnumTitle() + " " + ScheduleRestrictionTuesday.ShedulesStr + "; ";
			}
			if(ScheduleRestrictionWednesday != null) {
				result += ScheduleRestrictionWednesday.WeekDay.GetEnumTitle() + " " + ScheduleRestrictionWednesday.ShedulesStr + "; ";
			}
			if(ScheduleRestrictionThursday != null) {
				result += ScheduleRestrictionThursday.WeekDay.GetEnumTitle() + " " + ScheduleRestrictionThursday.ShedulesStr + "; ";
			}
			if(ScheduleRestrictionFriday != null) {
				result += ScheduleRestrictionFriday.WeekDay.GetEnumTitle() + " " + ScheduleRestrictionFriday.ShedulesStr + "; ";
			}
			if(ScheduleRestrictionSaturday != null) {
				result += ScheduleRestrictionSaturday.WeekDay.GetEnumTitle() + " " + ScheduleRestrictionSaturday.ShedulesStr + "; ";
			}
			if(ScheduleRestrictionSunday != null) {
				result += ScheduleRestrictionSunday.WeekDay.GetEnumTitle() + " " + ScheduleRestrictionSunday.ShedulesStr + "; ";
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
		public DistrictWaterPriceStringType() : base(typeof(DistrictWaterPrice))
		{

		}
	}
}