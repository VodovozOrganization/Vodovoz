namespace Vodovoz.ViewModels.ViewModels.Reports.Logistics.CompletedAddressesReport
{
	/// <summary>
	/// Строка отчета по выполненным адресам и районам (один водитель)
	/// </summary>
	public class CompletedAddressesReportRow
	{
		/// <summary>
		/// Идентификатор водителя
		/// </summary>
		public int DriverId { get; set; }

		/// <summary>
		/// ФИО водителя
		/// </summary>
		public string DriverFullName { get; set; }

		/// <summary>
		/// Количество выполненных адресов по районам (индекс совпадает с индексом колонки района)
		/// </summary>
		public int[] DistrictCounts { get; set; }

		/// <summary>
		/// Всего выполненных адресов по всем районам
		/// </summary>
		public int Total { get; set; }
	}
}
