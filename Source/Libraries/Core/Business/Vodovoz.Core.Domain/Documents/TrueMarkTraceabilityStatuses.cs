namespace Vodovoz.Core.Domain.Documents
{
	/// <summary>
	/// Наборы статусов прослеживаемости в ЧЗ
	/// </summary>
	public static class TrueMarkTraceabilityStatuses
	{
		/// <summary>
		/// Статусы, которыми ГИС МТ отказала в обработке кодов документооборота
		/// </summary>
		public static readonly TrueMarkTraceabilityStatus[] Rejected =
		{
			TrueMarkTraceabilityStatus.Rejected,
			TrueMarkTraceabilityStatus.CancellationRejected
		};
	}
}
