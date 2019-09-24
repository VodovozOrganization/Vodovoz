using System.ComponentModel.DataAnnotations;
using Gamma.Utilities;
using QS.DomainModel.Entity;
using QS.DomainModel.Entity.EntityPermissions;
using QS.HistoryLog;

namespace Vodovoz.Domain.WageCalculation
{
	[Appellative(
			Gender = GrammaticalGender.Masculine,
			NominativePlural = "параметры расчёта зарплаты по уровню ставок",
			Nominative = "параметр расчёта зарплаты по уровню ставок",
			Accusative = "параметра расчёта зарплаты по уровню ставок",
			Genitive = "параметра расчёта зарплаты по уровню ставок"
	)]
	[HistoryTrace]
	[EntityPermission]
	public class RatesLevelWageParameter : WageParameter
	{
		[Display(Name = "Тип расчёта ЗП")]
		public override WageParameterTypes WageParameterType {
			get => WageParameterTypes.RatesLevel;
			set { }
		}


		public override string Title => $"{WageParameterType.GetEnumTitle()}, {WageDistrictLevelRates.Name}";

		WageDistrictLevelRates wageDistrictLevelRates;
		[Display(Name = "Уровневая ставка по районам")]
		public virtual WageDistrictLevelRates WageDistrictLevelRates {
			get => wageDistrictLevelRates;
			set => SetField(ref wageDistrictLevelRates, value);
		}
	}
}