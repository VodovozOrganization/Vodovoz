using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.ReportsParameters.Logistic
{
	public partial class DeliveryTimeReportViewModel
	{
		public enum ReportType
		{
			[Display(Name = "Группировка по водителям, без нумерации")]
			Grouped,
			[Display(Name = "Без группировки, с нумерацией")]
			Numbering
		}
	}
}
