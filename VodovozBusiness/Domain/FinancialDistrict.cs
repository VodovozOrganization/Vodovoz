using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GeoAPI.Geometries;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using Vodovoz.Domain.Sale;

namespace Vodovoz.Domain
{
    [Appellative(Gender = GrammaticalGender.Masculine,
        NominativePlural = "финансовые районы",
        Nominative = "финансовый район")]
    [EntityPermission]
    [HistoryTrace]
    public class FinancialDistrict: PropertyChangedBase, IDomainObject, IValidatableObject, ICloneable
    {
        #region Свойства
		public virtual int Id { get; set; }

		string name;
		[Display(Name = "Название района")]
		public virtual string Name {
			get => name;
			set => SetField(ref name, value);
		}

		private IGeometry border;
		[Display(Name = "Граница")]
		public virtual IGeometry Border {
			get => border;
			set => SetField(ref border, value);
		}
		
		private GeographicGroup geographicGroup;
		[Display(Name = "Часть города")]
		public virtual GeographicGroup GeographicGroup {
			get => geographicGroup;
			set => SetField(ref geographicGroup, value);
		}
		
		private FinancialDistrictsSet financialDistrictsesSet;
		[Display(Name = "Версия финансовых районов")]
		public virtual FinancialDistrictsSet FinancialDistrictsSet {
			get => financialDistrictsesSet;
			set => SetField(ref financialDistrictsesSet, value);
		}
		
		private FinancialDistrict copyOf;
		[Display(Name = "Копия финансового района")]
		public virtual FinancialDistrict CopyOf {
			get => copyOf;
			set => SetField(ref copyOf, value);
		}
		
		#endregion
		
		#region IValidatableObject implementation
		
		public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(String.IsNullOrWhiteSpace(Name)) {
				yield return new ValidationResult(
					"Необходимо заполнить имя района",
					new[] { nameof(this.Name) }
				);
			}
			if(GeographicGroup == null) {
				yield return new ValidationResult(
					$"Для района \"{Name}\"Необходимо указать часть города, содержащую этот финансовый район",
					new[] { nameof(this.GeographicGroup) }
				);
			}
			if(Border == null) {
				yield return new ValidationResult(
					$"Для района \"{Name}\"Необходимо нарисовать границы на карте",
					new[] { nameof(this.Border) }
				);
			}
		}
		
		#endregion
		
		#region ICloneable implementation

		public virtual object Clone()
		{
			var newDistrict = new FinancialDistrict {
				Name = Name,
				Border = Border?.Copy(),
				GeographicGroup = GeographicGroup,
			};
			/*newDistrict.InitializeAllCollections();

			foreach (var commonRuleItem in CommonDistrictRuleItems) {
				var newCommonRuleItem = commonRuleItem.Clone() as CommonDistrictRuleItem;
				newCommonRuleItem.District = newDistrict;
				newDistrict.CommonDistrictRuleItems
					.Add(newCommonRuleItem);
			}
			foreach (var scheduleRestriction in GetAllDeliveryScheduleRestrictions()) {
				var newScheduleRestriction = scheduleRestriction.Clone() as DeliveryScheduleRestriction;
				newScheduleRestriction.District = newDistrict;
				newDistrict.GetScheduleRestrictionCollectionByWeekDayName(scheduleRestriction.WeekDay)
					.Add(newScheduleRestriction);
			}
			foreach (var weekDayRule in GetAllWeekDayDistrictRuleItems()) {
				var newWeekDayRule = weekDayRule.Clone() as WeekDayDistrictRuleItem;
				newWeekDayRule.District = newDistrict;
				newDistrict.GetWeekDayRuleItemCollectionByWeekDayName(weekDayRule.WeekDay)
					.Add(newWeekDayRule);
			}*/
			
			return newDistrict;
		}

		#endregion
    }
}