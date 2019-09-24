using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.WageCalculation
{
	/// <summary>
	/// Типы расчета зарплат
	/// </summary>
	public enum WageParameterTypes
	{
		[Display(Name = "Ручной расчёт")]
		Manual,
		[Display(Name = "Старые ставки")]
		OldRates,
		[Display(Name = "Фиксированная сумма")]
		Fixed,
		[Display(Name = "Процент")]
		Percent,
		[Display(Name = "Уровень ставок")]
		RatesLevel,
		[Display(Name = "План продаж")]
		SalesPlan
	}

	public class WageParameterTypesStringType : NHibernate.Type.EnumStringType
	{
		public WageParameterTypesStringType() : base(typeof(WageParameterTypes)) { }
	}
}