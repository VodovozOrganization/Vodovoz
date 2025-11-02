using Vodovoz.Core.Domain.Results;

namespace Vodovoz.Presentation.ViewModels.Errors
{
	public static class Report
	{
		public static Error CreateAborted =>
			new Error(
				typeof(Report),
				nameof(CreateAborted),
				"Создание отчета отменено");

		public static Error NoData =>
			new Error(
				typeof(Report),
				nameof(NoData),
				"Для указанных параметров формирования отчета отсутствуют данные");
	}
}
