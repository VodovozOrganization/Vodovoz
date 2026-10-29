namespace Vodovoz.Core.Domain.Edo
{
	/// <summary>
	/// Наборы статусов задачи ЭДО
	/// </summary>
	public static class EdoTaskStatuses
	{
		/// <summary>
		/// Статусы, в которых задача ЭДО считается завершенной
		/// </summary>
		public static readonly EdoTaskStatus[] Finished =
		{
			EdoTaskStatus.Completed,
			EdoTaskStatus.Cancelled
		};
	}
}
