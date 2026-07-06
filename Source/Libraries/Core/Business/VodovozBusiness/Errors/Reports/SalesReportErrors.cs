using Vodovoz.Core.Domain.Results;

namespace VodovozBusiness.Errors.Reports
{
	public static partial class SalesReportErrors
	{
		public static Error NoDataForExport =>
			new Error(
				typeof(SalesReportErrors),
				nameof(NoDataForExport),
				"Нет данных для экспорта отчета");

		public static Error InvalidFilePath =>
			new Error(
				typeof(SalesReportErrors),
				nameof(InvalidFilePath),
				"Путь к файлу не может быть пустым");
	}
}
