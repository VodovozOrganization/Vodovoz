using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ViewModels.QualityControl.Reports
{
	public partial class NumberOfComplaintsAgainstDriversReportViewModel
	{
		/// <summary>
		/// Порядок сортировки в отчёте по кол-ву рекламаций на водителей
		/// </summary>
		public enum ReportSortOrder
		{
			[Display(Name = "По кол-ву рекламаций")]
			ComplaintsCount,

			[Display(Name = "По ФИО")]
			DriverName
		}
	}
}
