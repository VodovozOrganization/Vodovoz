using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;
using QS.Utilities;

namespace Vodovoz.Domain.WageCalculation
{
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "параметры расчёта зарплаты с фиксированной суммой",
			Nominative = "параметр расчёта зарплаты с фиксированной суммой",
			Accusative = "параметра расчёта зарплаты с фиксированной суммой",
			Genitive = "параметра расчёта зарплаты с фиксированной суммой"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class FixedWageParameterItem : WageParameterItem
	{
		public override WageParameterItemTypes WageParameterItemType {
			get { return WageParameterItemTypes.Fixed; }
			set { }
		}

		public override string Title => $"{WageParameterItemType.GetEnumTitle()}, {FixedWageType.GetEnumTitle()} - {RouteListFixedWage.ToShortCurrencyString()}";

		private FixedWageTypes fixedWageType;
		[Display(Name = "Тип расчета по фиксе")]
		public virtual FixedWageTypes FixedWageType {
			get => fixedWageType;
			set => SetField(ref fixedWageType, value, () => FixedWageType);
		}

		private decimal routeListFixedWage;
		[Display(Name = "Фикса за МЛ")]
		public virtual decimal RouteListFixedWage {
			get => routeListFixedWage;
			set => SetField(ref routeListFixedWage, value, () => RouteListFixedWage);
		}

		#region IValidatableObject implementation

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			foreach(var item in base.Validate(validationContext)) {
				yield return item;
			}
			switch(FixedWageType) {
				case FixedWageTypes.RouteList:
					if(RouteListFixedWage <= 0) {
						yield return new ValidationResult("Должна быть указана фиксированная зарплата за маршрутный лист");
					}
					break;
				default:
					break;
			}
		}

		#endregion IValidatableObject implementation
	}
}