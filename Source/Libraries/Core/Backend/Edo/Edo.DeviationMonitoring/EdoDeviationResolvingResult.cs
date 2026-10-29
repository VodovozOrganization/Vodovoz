namespace Edo.DeviationMonitoring
{
	/// <summary>
	/// Результат одного прохода снятия отклонений документооборота ЭДО
	/// </summary>
	public class EdoDeviationResolvingResult
	{
		/// <summary>
		/// Количество проверенных незакрытых отклонений
		/// </summary>
		public int CheckedDeviationsCount { get; set; }

		/// <summary>
		/// Количество снятых отклонений
		/// </summary>
		public int ResolvedDeviationsCount { get; set; }
	}
}
