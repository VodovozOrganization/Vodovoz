using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ReportsParameters.Production
{
	public partial class ProducedProductionReport
	{
		public enum ProducedProductionReportMode
		{
			[Display(Name ="Месяц")]
			Month,
			[Display(Name = "Год")]
			Year
		}
	}
}
