using Newtonsoft.Json;
using System;

namespace Vodovoz.Models.CashReceipts.DTO
{
	/// <summary>
	/// Разрешительный режим
	/// </summary>
	public class IndustryRequisite
	{
		/// <summary>
		/// Идентификатор Федерального органа исполнительной власти (ФОИВ)
		/// </summary>
		[JsonProperty("foivId", Required = Required.Always)]
		public string FoivId => "030";

		/// <summary>
		/// Дата регламентирующего документа
		/// </summary>
		[JsonProperty("docDateTime", Required = Required.Always)]
		public string DocDateTime => "2023-11-21T00:00:00+00:00";

		/// <summary>
		/// Номер документа, который регламентирует заполнение отраслевых реквизитов.
		/// Постановление Правительства РФ от 21.11.2023 № 1944
		/// </summary>
		[JsonProperty("docNumber", Required = Required.Always)]
		public string DocNumber => "1944";

		/// <summary>
		/// Данные об идентификаторе и времени запроса проверки КМ. (Получает с помощью терминала сбора данных)
		/// </summary>
		[JsonProperty("docData", Required = Required.Always)]
		public string DocData { get; set; }
	}
}
