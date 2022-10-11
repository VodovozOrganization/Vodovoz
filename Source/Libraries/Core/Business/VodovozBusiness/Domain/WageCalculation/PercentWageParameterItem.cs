using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.WageCalculation
{
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "параметры расчёта зарплаты по проценту",
			Nominative = "параметр расчёта зарплаты по проценту",
			Accusative = "параметра расчёта зарплаты по проценту",
			Genitive = "параметра расчёта зарплаты по проценту"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class PercentWageParameterItem : WageParameterItem
	{
		public override WageParameterItemTypes WageParameterItemType {
			get { return WageParameterItemTypes.Percent; }
			set { }
		}

		public override string Title => $"{WageParameterItemType.GetEnumTitle()}, {GetPercentTitle()}";

		private PercentWageTypes percentWageType;
		[Display(Name = "Тип расчета по процентам")]
		public virtual PercentWageTypes PercentWageType {
			get => percentWageType;
			set => SetField(ref percentWageType, value, () => PercentWageType);
		}

		private decimal routeListPercent;
		[Display(Name = "Процент со стоимости МЛ")]
		public virtual decimal RouteListPercent {
			get => routeListPercent;
			set => SetField(ref routeListPercent, value, () => RouteListPercent);
		}

		private string GetPercentTitle()
		{
			string result = PercentWageType.GetEnumTitle();
			switch(PercentWageType) {
				case PercentWageTypes.RouteList:
					result += $" - {RouteListPercent}%";
					break;
				case PercentWageTypes.Service:
					break;
				default:
					break;
			}

			return result;
		}

		#region IValidatableObject implementation

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			foreach (var validationResult in base.Validate(validationContext)) {
				yield return validationResult;
			}
			
			switch(PercentWageType) {
				case PercentWageTypes.RouteList:
					if(RouteListPercent <= 0) {
						yield return new ValidationResult("Должен быть указан процент за маршрутный лист");
					}
					break;
				case PercentWageTypes.Service:
					break;
				default:
					break;
			}
		}

		#endregion IValidatableObject implementation
	}
}
