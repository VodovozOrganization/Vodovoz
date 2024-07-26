using Vodovoz.Errors;

namespace Vodovoz.Presentation.ViewModels.Errors
{
	public static class Report
	{
		public static Error CreateAborted =>
			new Error(
				typeof(Report),
				nameof(CreateAborted),
				"Создание отчета отменено");
	}
}
