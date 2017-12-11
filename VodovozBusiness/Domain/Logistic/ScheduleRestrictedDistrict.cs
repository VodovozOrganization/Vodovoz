using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using GeoAPI.Geometries;
using QSOrmProject;

namespace Vodovoz.Domain.Logistic
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

		IList<ScheduleRestriction> scheduleRestrictions = new List<ScheduleRestriction>();

		public virtual IList<ScheduleRestriction> ScheduleRestrictions {
			get { return scheduleRestrictions; }
			set { SetField(ref scheduleRestrictions, value, () => ScheduleRestrictions); }
		}

		GenericObservableList<ScheduleRestriction> observableScheduleRestrictions;

		public virtual GenericObservableList<ScheduleRestriction> ObservableScheduleRestrictions {
			get {
				if(observableScheduleRestrictions == null) {
					observableScheduleRestrictions = new GenericObservableList<ScheduleRestriction>(ScheduleRestrictions);
				}
				return observableScheduleRestrictions;
			}
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

		public virtual void AddSchedule(IUnitOfWork UoW)
		{
			var schedule = new ScheduleRestriction() {
				District = this
			};
			observableScheduleRestrictions.Add(schedule);
		}

		public virtual void Save(IUnitOfWork UoW)
		{
			UoW.Save(this);
			foreach(ScheduleRestriction restriction in ObservableScheduleRestrictions) {
				restriction.Save(UoW);
			}
		}

		public virtual void Remove(IUnitOfWork UoW)
		{
			foreach(ScheduleRestriction restriction in ObservableScheduleRestrictions) {
				restriction.Remove(UoW);
			}
			UoW.Delete(this);
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