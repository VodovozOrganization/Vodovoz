namespace Vodovoz.Presentation.ViewModels.Logistic.Reports
{
	public partial class CarIsNotAtLineReport
	{
		/// <summary>
		/// Тип строки отчета в UI
		/// </summary>
		public enum UiRowType
		{
			/// <summary>
			/// Строка отчета - основная секция
			/// </summary>
			Row,
			/// <summary>
			/// Строка отчета - прием автомобиля
			/// </summary>
			CarReceptionRow,
			/// <summary>
			/// Строка отчета - передача автомобиля
			/// </summary>
			CarTransferRow,
			/// <summary>
			/// Строка отчета - название подтаблицы
			/// </summary>
			SubtableName,
			/// <summary>
			/// Строка отчета - заголовок подтаблицы
			/// </summary>
			SubtableHeader,
			/// <summary>
			/// Строка отчета - итоговая строка
			/// </summary>
			SummaryRow,
			/// <summary>
			/// Строка отчета - пустая строка
			/// </summary>
			EmptyRow
		}
	}
}
