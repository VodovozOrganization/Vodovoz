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
	public class SalesPlanWageParameterItem : WageParameterItem
	{
		public override WageParameterItemTypes WageParameterItemType {
			get { return WageParameterItemTypes.SalesPlan; }
			set { }
		}

		public override string Title => $"{WageParameterItemType.GetEnumTitle()}, {SalesPlan.Title}";

		SalesPlan salesPlan;
		[Display(Name = "План продаж")]
		public virtual SalesPlan SalesPlan {
			get => salesPlan;
			set => SetField(ref salesPlan, value);
		}

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			foreach (var validationResult in base.Validate(validationContext)) {
				yield return validationResult;
			}

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