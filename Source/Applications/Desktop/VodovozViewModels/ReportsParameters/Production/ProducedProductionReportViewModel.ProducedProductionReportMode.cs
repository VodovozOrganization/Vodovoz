using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.ReportsParameters.Production
{
	public partial class ProducedProductionReportViewModel
	{
		public enum ProducedProductionReportMode
		{
			[Display(Name = "Месяц")]
			Month,
			[Display(Name = "Год")]
			Year
		}
	}
}
