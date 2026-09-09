namespace Edo.DeviationMonitoring
{
	/// <summary>
	/// Результат одного прохода регистрации отклонений документооборота ЭДО
	/// </summary>
	public class EdoDeviationRegistrationResult
	{
		/// <summary>
		/// Количество проверенных заявок без задач
		/// </summary>
		public int CheckedRequestsCount { get; set; }

		/// <summary>
		/// Количество проверенных незавершенных задач ЭДО
		/// </summary>
		public int CheckedTasksCount { get; set; }

		/// <summary>
		/// Количество проверенных незавершенных задач трансфера
		/// </summary>
		public int CheckedTransferTasksCount { get; set; }

		/// <summary>
		/// Количество задач с завершенным документооборотом,
		/// проверенных на результат обработки кодов в ГИС МТ
		/// </summary>
		public int CheckedFinishedDocflowTasksCount { get; set; }

		/// <summary>
		/// Количество зарегистрированных отклонений
		/// </summary>
		public int RegisteredDeviationsCount { get; set; }
	}
}
