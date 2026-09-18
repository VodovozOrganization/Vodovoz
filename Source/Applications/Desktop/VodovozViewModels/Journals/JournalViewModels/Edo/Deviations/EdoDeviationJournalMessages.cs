namespace Vodovoz.ViewModels.Journals.JournalViewModels.Edo.Deviations
{
	/// <summary>
	/// Постоянные тексты для подстановки в журнал отклонений документооборота ЭДО
	/// </summary>
	public static class EdoDeviationJournalMessages
	{
		/// <summary>
		/// Результат строки задачи, оставшейся в проблемном статусе без единой записи проблемы
		/// </summary>
		public const string UnknownProblemResult = "Проблема без зафиксированной причины";

		/// <summary>
		/// Результат строки задачи, оставшейся в проблемном статусе с решенными проблемами
		/// </summary>
		public const string SolvedProblemResult = "Расхождение статуса задачи и ее проблем";

		/// <summary>
		/// Описание строки задачи, оставшейся в проблемном статусе без единой записи проблемы
		/// </summary>
		public const string UnknownProblemDescription =
			"Задача переведена в проблемный статус, но запись о проблеме по ней не заведена:"
			+ " причина не зафиксирована";

		/// <summary>
		/// Описание строки задачи, оставшейся в проблемном статусе с решенными проблемами
		/// </summary>
		public const string SolvedProblemDescription =
			"Все проблемы по задаче помечены решенными, но сама задача осталась"
			+ " в проблемном статусе: статус задачи разошелся с состоянием ее проблем";

		/// <summary>
		/// Рекомендация по строке задачи, оставшейся в проблемном статусе без действующей
		/// записи проблемы
		/// </summary>
		public const string UnknownProblemRecommendation = "Обратитесь в отдел разработки";
	}
}
