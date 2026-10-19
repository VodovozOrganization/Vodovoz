using System.Text.Json.Serialization;

namespace BitrixNotificationsSend.Contracts.Dto
{
	/// <summary>
	/// Данные для закрытия сделки по отсрочке сети в Битрикс24 после оплаты заказа
	/// </summary>
	public class ChainStoreDelayDealCloseDto
	{
		/// <summary>
		/// Id сохранённой в базе данных записи об отправке сделки по отсрочке сети
		/// </summary>
		[JsonIgnore]
		public int ChainStoreDelayBitrixDealId { get; set; }

		/// <summary>
		/// Id закрываемой сделки в Битрикс24
		/// </summary>
		[JsonIgnore]
		public long BitrixDealId { get; set; }

		/// <summary>
		/// Ключ команды закрытия сделки в пакетном запросе, содержит id сохранённой записи об отправке
		/// </summary>
		[JsonIgnore]
		public string DealCommandKey => $"{ChainStoreDelayDealCommandKeys.CloseCommandKeyPrefix}{ChainStoreDelayBitrixDealId}";

		/// <summary>
		/// Стадия сделки в Битрикс24, в которую переводится сделка при оплате заказа
		/// </summary>
		[JsonPropertyName("STAGE_ID")]
		public string StageId { get; set; }
	}
}
