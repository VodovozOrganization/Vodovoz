using System.ComponentModel.DataAnnotations;

namespace Vodovoz.Domain.WageCalculation
{
    public enum WageParameterItemTypes
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
}
