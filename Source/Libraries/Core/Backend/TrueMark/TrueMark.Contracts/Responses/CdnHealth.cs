namespace TrueMark.Contracts.Responses
{
	/// <summary>
	/// Состояние CDN-площадки
	/// </summary>
	public class CdnHealth
	{
		/// <summary>
		/// Результат обработки операции
		/// </summary>
		public int Code { get; set; }

		/// <summary>
		/// Текстовое описание результата выполнения метода
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// Среднее время проверки кода маркировки внутри CDN-площадки
		/// </summary>
		public int AvgTimeMs { get; set; }

		public string Host { get; set; }
	}
}
