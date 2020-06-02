using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using GeoAPI.Geometries;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.DomainModel.UoW;
using Vodovoz.Domain.WageCalculation;
using Vodovoz.Tools.Orders;

namespace Vodovoz.Domain.Sale
{
	[Appellative(Gender = GrammaticalGender.Masculine,
		NominativePlural = "правила районов доставок",
		Nominative = "правила района доставки")]
	[EntityPermission]
	public class District : BusinessObjectBase<District>, IDomainObject, IValidatableObject
	{
		#region Свойства
		public virtual int Id { get; set; }

		string districtName;

		[Required(ErrorMessage = "Имя района обязательно")]
		public virtual string DistrictName {
			get => districtName;
			set => SetField(ref districtName, value, () => DistrictName);
		}

		int minBottles;

		public virtual int MinBottles {
			get => minBottles;
			set => SetField(ref minBottles, value, () => MinBottles);
		}

		public virtual bool HaveRestrictions => GetAllDeliveryScheduleRestrictions().Any();

			TariffZone tariffZone;
		public virtual TariffZone TariffZone {
			get => tariffZone;
			set => SetField(ref tariffZone, value, () => TariffZone);
		}

		private IGeometry districtBorder;

		public virtual IGeometry DistrictBorder {
			get => districtBorder;
			set => SetField(ref districtBorder, value, () => DistrictBorder);
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


		#region CommonDistrictRuleItems

		private IList<CommonDistrictRuleItem> commonDistrictRuleItems = new List<CommonDistrictRuleItem>();
		[Display(Name = "Правила цены доставки")]
		public virtual IList<CommonDistrictRuleItem> CommonDistrictRuleItems {
			get => commonDistrictRuleItems;
			set => SetField(ref commonDistrictRuleItems, value, () => CommonDistrictRuleItems);
		}

		private GenericObservableList<CommonDistrictRuleItem> observableCommonDistrictRuleItems;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<CommonDistrictRuleItem> ObservableCommonDistrictRuleItems {
			get {
				if(observableCommonDistrictRuleItems == null)
					observableCommonDistrictRuleItems = new GenericObservableList<CommonDistrictRuleItem>(CommonDistrictRuleItems);
				return observableCommonDistrictRuleItems;
			}
		}

		#endregion
		
		#region DeliveryScheduleRestrictions
		
		private IList<DeliveryScheduleRestriction> todayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		public virtual IList<DeliveryScheduleRestriction> TodayDeliveryScheduleRestrictions {
			get => todayDeliveryScheduleRestrictions;
			set => SetField(ref todayDeliveryScheduleRestrictions, value, () => TodayDeliveryScheduleRestrictions);
		}

		private GenericObservableList<DeliveryScheduleRestriction> observableTodayDeliveryScheduleRestrictions;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableTodayDeliveryScheduleRestrictions {
			get {
				if(observableTodayDeliveryScheduleRestrictions == null)
					observableTodayDeliveryScheduleRestrictions = new GenericObservableList<DeliveryScheduleRestriction>(TodayDeliveryScheduleRestrictions);
				return observableTodayDeliveryScheduleRestrictions;
			}
		}
		
		private IList<DeliveryScheduleRestriction> mondayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		public virtual IList<DeliveryScheduleRestriction> MondayDeliveryScheduleRestrictions {
			get => mondayDeliveryScheduleRestrictions;
			set => SetField(ref mondayDeliveryScheduleRestrictions, value, () => MondayDeliveryScheduleRestrictions);
		}

		private GenericObservableList<DeliveryScheduleRestriction> observableMondayDeliveryScheduleRestrictions;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableMondayDeliveryScheduleRestrictions {
			get {
				if(observableMondayDeliveryScheduleRestrictions == null)
					observableMondayDeliveryScheduleRestrictions = new GenericObservableList<DeliveryScheduleRestriction>(MondayDeliveryScheduleRestrictions);
				return observableMondayDeliveryScheduleRestrictions;
			}
		}
		
		private IList<DeliveryScheduleRestriction> tuesdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		public virtual IList<DeliveryScheduleRestriction> TuesdayDeliveryScheduleRestrictions {
			get => tuesdayDeliveryScheduleRestrictions;
			set => SetField(ref tuesdayDeliveryScheduleRestrictions, value, () => TuesdayDeliveryScheduleRestrictions);
		}

		private GenericObservableList<DeliveryScheduleRestriction> observableTuesdayDeliveryScheduleRestrictions;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableTuesdayDeliveryScheduleRestrictions {
			get {
				if(observableTuesdayDeliveryScheduleRestrictions == null)
					observableTuesdayDeliveryScheduleRestrictions = new GenericObservableList<DeliveryScheduleRestriction>(TuesdayDeliveryScheduleRestrictions);
				return observableTuesdayDeliveryScheduleRestrictions;
			}
		}

		private IList<DeliveryScheduleRestriction> wednesdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		public virtual IList<DeliveryScheduleRestriction> WednesdayDeliveryScheduleRestrictions {
			get => wednesdayDeliveryScheduleRestrictions;
			set => SetField(ref wednesdayDeliveryScheduleRestrictions, value, () => WednesdayDeliveryScheduleRestrictions);
		}

		private GenericObservableList<DeliveryScheduleRestriction> observableWednesdayDeliveryScheduleRestrictions;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableWednesdayDeliveryScheduleRestrictions {
			get {
				if(observableWednesdayDeliveryScheduleRestrictions == null)
					observableWednesdayDeliveryScheduleRestrictions = new GenericObservableList<DeliveryScheduleRestriction>(WednesdayDeliveryScheduleRestrictions);
				return observableWednesdayDeliveryScheduleRestrictions;
			}
		}

		private IList<DeliveryScheduleRestriction> thursdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		public virtual IList<DeliveryScheduleRestriction> ThursdayDeliveryScheduleRestrictions {
			get => thursdayDeliveryScheduleRestrictions;
			set => SetField(ref thursdayDeliveryScheduleRestrictions, value, () => ThursdayDeliveryScheduleRestrictions);
		}

		private GenericObservableList<DeliveryScheduleRestriction> observableThursdayDeliveryScheduleRestrictions;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableThursdayDeliveryScheduleRestrictions {
			get {
				if(observableThursdayDeliveryScheduleRestrictions == null)
					observableThursdayDeliveryScheduleRestrictions = new GenericObservableList<DeliveryScheduleRestriction>(ThursdayDeliveryScheduleRestrictions);
				return observableThursdayDeliveryScheduleRestrictions;
			}
		}

		private IList<DeliveryScheduleRestriction> fridayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		public virtual IList<DeliveryScheduleRestriction> FridayDeliveryScheduleRestrictions {
			get => fridayDeliveryScheduleRestrictions;
			set => SetField(ref fridayDeliveryScheduleRestrictions, value, () => FridayDeliveryScheduleRestrictions);
		}

		private GenericObservableList<DeliveryScheduleRestriction> observableFridayDeliveryScheduleRestrictions;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableFridayDeliveryScheduleRestrictions {
			get {
				if(observableFridayDeliveryScheduleRestrictions == null)
					observableFridayDeliveryScheduleRestrictions = new GenericObservableList<DeliveryScheduleRestriction>(FridayDeliveryScheduleRestrictions);
				return observableFridayDeliveryScheduleRestrictions;
			}
		}

		private IList<DeliveryScheduleRestriction> saturdayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		public virtual IList<DeliveryScheduleRestriction> SaturdayDeliveryScheduleRestrictions {
			get => saturdayDeliveryScheduleRestrictions;
			set => SetField(ref saturdayDeliveryScheduleRestrictions, value, () => SaturdayDeliveryScheduleRestrictions);
		}

		private GenericObservableList<DeliveryScheduleRestriction> observableSaturdayDeliveryScheduleRestrictions;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableSaturdayDeliveryScheduleRestrictions {
			get {
				if(observableSaturdayDeliveryScheduleRestrictions == null)
					observableSaturdayDeliveryScheduleRestrictions = new GenericObservableList<DeliveryScheduleRestriction>(SaturdayDeliveryScheduleRestrictions);
				return observableSaturdayDeliveryScheduleRestrictions;
			}
		}

		private IList<DeliveryScheduleRestriction> sundayDeliveryScheduleRestrictions = new List<DeliveryScheduleRestriction>();
		public virtual IList<DeliveryScheduleRestriction> SundayDeliveryScheduleRestrictions {
			get => sundayDeliveryScheduleRestrictions;
			set => SetField(ref sundayDeliveryScheduleRestrictions, value, () => SundayDeliveryScheduleRestrictions);
		}

		private GenericObservableList<DeliveryScheduleRestriction> observableSundayDeliveryScheduleRestrictions;
		//FIXME Костыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<DeliveryScheduleRestriction> ObservableSundayDeliveryScheduleRestrictions {
			get {
				if(observableSundayDeliveryScheduleRestrictions == null)
					observableSundayDeliveryScheduleRestrictions = new GenericObservableList<DeliveryScheduleRestriction>(SundayDeliveryScheduleRestrictions);
				return observableSundayDeliveryScheduleRestrictions;
			}
		}
		
		#endregion

		#region GeographicGroups

		private IList<GeographicGroup> geographicGroups = new List<GeographicGroup>();
		[Display(Name = "Список районов города")]
		public virtual IList<GeographicGroup> GeographicGroups {
			get => geographicGroups;
			set => SetField(ref geographicGroups, value, () => GeographicGroups);
		}

		private GenericObservableList<GeographicGroup> observableGeographicGroups;
		//FIXME Кослыль пока не разберемся как научить hibernate работать с обновляемыми списками.
		public virtual GenericObservableList<GeographicGroup> ObservableGeographicGroups {
			get {
				if(observableGeographicGroups == null)
					observableGeographicGroups = new GenericObservableList<GeographicGroup>(GeographicGroups);
				return observableGeographicGroups;
			}
		}

		#endregion

		WageDistrict wageDistrict;
		[Display(Name = "Группа района для расчёта ЗП")]
		public virtual WageDistrict WageDistrict {
			get => wageDistrict;
			set => SetField(ref wageDistrict, value);
		}

		#endregion

		#region Функции

		public virtual string Title => DistrictName;

		public virtual string GetSchedulesString()
		{
			string result = String.Empty;
			foreach (var deliveryScheduleRestriction in GetAllDeliveryScheduleRestrictions()
			                                            .GroupBy(x => x.WeekDay)
			                                            .OrderBy(x => (int)x.Key)) 
			{
				result += deliveryScheduleRestriction.Key.GetEnumTitle() + " ";
				result += String.Join(", ",deliveryScheduleRestriction
														.OrderBy(x => x.DeliverySchedule.From)
														.ThenBy(x => x.DeliverySchedule.To)
														.Select(x => x.DeliverySchedule.Name));
				result += ";\n";
			}
			result.TrimEnd('\n');
			return result;
		}

		public virtual void Save(IUnitOfWork UoW)
		{
			UoW.Save(this);
		}

		public virtual void Remove(IUnitOfWork UoW)
		{
			UoW.Delete(this);
		}

		public virtual IEnumerable<DeliveryScheduleRestriction> GetAllDeliveryScheduleRestrictions()
		{
			return TodayDeliveryScheduleRestrictions
			       .Union(MondayDeliveryScheduleRestrictions)
			       .Union(TuesdayDeliveryScheduleRestrictions)
			       .Union(WednesdayDeliveryScheduleRestrictions)
			       .Union(ThursdayDeliveryScheduleRestrictions)
			       .Union(FridayDeliveryScheduleRestrictions)
			       .Union(SaturdayDeliveryScheduleRestrictions)
			       .Union(SundayDeliveryScheduleRestrictions);
		}
		
		public virtual decimal GetDeliveryPrice(OrderStateKey orderStateKey)
		{
			var ruleItems = CommonDistrictRuleItems.Where(x => orderStateKey.CompareWithDeliveryPriceRule(x.DeliveryPriceRule));
			return ruleItems.Any() ? ruleItems.Max(x => x.DeliveryPrice) : 0m;
		}

		#endregion

		#region IValidatableObject implementation
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(ObservableCommonDistrictRuleItems.Any(i => i.DeliveryPrice <= 0))
				yield return new ValidationResult(
					"Для всех правил доставки должны быть указаны цены",
					new[] { this.GetPropertyName(o => o.CommonDistrictRuleItems) }
				);

			if(!GeographicGroups.Any())
				yield return new ValidationResult(
					string.Format("Для района \"{0}\" необходимо указать часть города, содержащую этот район доставки", DistrictName),
					new[] { this.GetPropertyName(o => o.GeographicGroups) }
				);

			if(DistrictBorder == null)
				yield return new ValidationResult(
					string.Format("Для района \"{0}\" необходимо нарисовать границы на карте", DistrictName),
					new[] { this.GetPropertyName(o => o.DistrictBorder) }
				);
			if(WageDistrict == null)
				yield return new ValidationResult(
					string.Format("Для района \"{0}\" необходимо выбрать зарплатную группу", DistrictName),
					new[] { this.GetPropertyName(o => o.WageDistrict) }
				);
		}
		#endregion
	}

}