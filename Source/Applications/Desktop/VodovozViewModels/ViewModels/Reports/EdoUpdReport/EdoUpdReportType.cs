using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.ViewModels.Reports.EdoUpdReport
{
	public partial class EdoUpdReportViewModel
	{
		public enum EdoUpdReportType
		{
			[Display(Name = "Не отражённые в ЧЗ")]
			Missing,
			[Display(Name = "Успешно выведенные из оборота")]
			Successfull
		}
	}
}
