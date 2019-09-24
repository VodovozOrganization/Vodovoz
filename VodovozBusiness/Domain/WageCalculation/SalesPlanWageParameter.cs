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
			NominativePlural = "параметры расчёта зарплаты по плану продаж",
			Nominative = "параметр расчёта зарплаты по плану продаж",
			Accusative = "параметра расчёта зарплаты по плану продаж",
			Genitive = "параметра расчёта зарплаты по плану продаж"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class SalesPlanWageParameter : WageParameter
	{
		[Display(Name = "Тип расчёта ЗП")]
		public override WageParameterTypes WageParameterType {
			get => WageParameterTypes.SalesPlan;
			set { }
		}

		public override string Title => $"{WageParameterType.GetEnumTitle()}, {SalesPlan.Title}";

		SalesPlan salesPlan;
		[Display(Name = "План продаж")]
		public virtual SalesPlan SalesPlan {
			get => salesPlan;
			set => SetField(ref salesPlan, value);
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if(SalesPlan == null)
				yield return new ValidationResult(
					"Не выбран план продаж.",
					new[] { this.GetPropertyName(o => o.SalesPlan) }
				);

			if(SalesPlan != null && SalesPlan.IsArchive)
				yield return new ValidationResult(
					"Выбран архивный план продаж.",
					new[] { this.GetPropertyName(o => o.SalesPlan) }
				);
		}
	}
}