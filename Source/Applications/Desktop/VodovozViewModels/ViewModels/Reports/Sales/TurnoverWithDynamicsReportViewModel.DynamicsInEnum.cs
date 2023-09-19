using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.Reports.Sales
{
	public partial class TurnoverWithDynamicsReportViewModel
	{
		public enum DynamicsInEnum
		{
			[Display(Name = "Проценты")]
			Percents,
			[Display(Name = "Единицы измерения")]
			MeasurementUnit
		}
	}
}
