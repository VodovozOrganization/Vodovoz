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
			NominativePlural = "параметры расчёта зарплаты без зарплаты",
			Nominative = "параметр расчёта зарплаты без зарплаты",
			Accusative = "параметра расчёта зарплаты без зарплаты",
			Genitive = "параметра расчёта зарплаты без зарплаты"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class ManualWageParameter : WageParameter
	{
		[Display(Name = "Тип расчёта ЗП")]
		public override WageParameterTypes WageParameterType {
			get => WageParameterTypes.Manual;
			set { }
		}

		public override string Title => $"{WageParameterType.GetEnumTitle()}";

		#region IValidatableObject implementation

		public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			yield break;
		}

		#endregion IValidatableObject implementation
	}
}
