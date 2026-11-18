using Newtonsoft.Json;

namespace ModulKassa.DTO
{
	/// <summary>
	/// Данные для чека коррекции.
	/// Заполняется при docType SALE_CORRECTION / SALE_RETURN_CORRECTION / BUY_CORRECTION / BUY_RETURN_CORRECTION.
	/// </summary>
	public class CorrectionInfo
	{
		/// <summary>
		/// INDEPENDENT — самостоятельная коррекция; BY_ORDER — по предписанию ФНС.
		/// </summary>
		[JsonProperty("reason", Required = Required.Always)]
		public string Reason { get; set; }

		/// <summary>
		/// Дата совершения корректируемого расчёта (исходный чек).
		/// </summary>
		[JsonProperty("documentDate", Required = Required.Always)]
		public string DocumentDate { get; set; }

		/// <summary>
		/// Номер предписания / служебной записки. Обязателен при reason = BY_ORDER.
		/// </summary>
		[JsonProperty("documentNum")]
		public string DocumentNum { get; set; }

		/// <summary>
		/// Фискальный признак некорректного чека.
		/// </summary>
		[JsonProperty("fiscalSign")]
		public string FiscalSign { get; set; }
	}
}
