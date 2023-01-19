using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class TurnoverWithDynamicsReportViewModel
	{
		public enum MeasurementUnitEnum
		{
			[Display(Name = "Штуки")]
			Amount,
			[Display(Name = "Рубли")]
			Price
		}
	}
}
