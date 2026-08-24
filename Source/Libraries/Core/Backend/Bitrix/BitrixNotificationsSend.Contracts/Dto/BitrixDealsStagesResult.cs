using System.Collections.Generic;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Результат чтения текущих стадий сделок из Битрикс24
	/// </summary>
	public class BitrixDealsStagesResult
	{
		/// <summary>
		/// Стадии найденных сделок: id сделки - идентификатор стадии
		/// </summary>
		public IDictionary<long, string> StagesByDealIds { get; set; } = new Dictionary<long, string>();

		/// <summary>
		/// Id сделок, которые не найдены в Битрикс24 (удалены)
		/// </summary>
		public IList<long> NotFoundDealIds { get; set; } = new List<long>();

		/// <summary>
		/// Прочие ошибки чтения сделок.
		/// По таким сделкам стадия не известна, обработку нужно повторить позднее
		/// </summary>
		public IList<BitrixBatchItemError> Errors { get; set; } = new List<BitrixBatchItemError>();
	}
}
