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
			/// Основная секция
			/// </summary>
			Main,
			/// <summary>
			/// Прием автомобиля
			/// </summary>
			CarReception,
			/// <summary>
			/// Передача автомобиля
			/// </summary>
			CarTransfer,
			/// <summary>
			/// Название подтаблицы
			/// </summary>
			SubtableName,
			/// <summary>
			/// Заголовок подтаблицы
			/// </summary>
			SubtableHeader,
			/// <summary>
			/// Итоговая строка
			/// </summary>
			Summary,
			/// <summary>
			/// Пустая строка
			/// </summary>
			Empty
		}
	}
}
