using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ReportsParameters
{
    public enum OrderStatisticsByWeekReportType
    {
        [Display(Name = "План")]
        Plan,
        [Display(Name = "Факт")]
        Fact,
    }
}
