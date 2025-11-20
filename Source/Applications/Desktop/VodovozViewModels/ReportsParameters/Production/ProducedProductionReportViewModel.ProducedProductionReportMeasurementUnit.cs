using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.ReportsParameters.Production
{
	public partial class ProducedProductionReportViewModel
	{
		public enum MeasurementUnit
		{
			[Display(Name = "Штуки")]
			Item,
			[Display(Name = "Литры")]
			Liters
		}
	}
}
