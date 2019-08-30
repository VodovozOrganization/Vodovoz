using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.Employees
{
	public enum WageCalculationType
	{
		[Display(Name = "Обычный")]
		normal,
		[Display(Name = "Без оплаты (Разовый водитель)")]
		withoutPayment,
		[Display(Name = "Процент от стоимости")]
		percentage,
		[Display(Name = "Процент от стоимости (СЦ)")]
		percentageForService,
		[Display(Name = "Фиксированная ставка за МЛ")]
		fixedRoute,
		[Display(Name = "Фиксированная ставка за день")]
		fixedDay,
		[Display(Name = "План продаж")]
		salesPlan
	}

	public class WageCalculationTypeStringType : NHibernate.Type.EnumStringType
	{
		public WageCalculationTypeStringType() : base(typeof(WageCalculationType)) { }
	}
}
