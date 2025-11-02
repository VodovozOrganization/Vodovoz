using Newtonsoft.Json;
using System;

namespace ModulKassa.DTO
{
	/// <summary>
	/// Отраслевые реквизиты.
	/// (Разрешительный режим)
	/// </summary>
	public class IndustryRequisite
	{
		/// <summary>
		/// Идентификатор Федерального органа исполнительной власти (ФОИВ)
		/// </summary>
		[JsonProperty("foivId", Required = Required.Always)]
		public string FoivId { get; set; }

		/// <summary>
		/// Дата регламентирующего документа
		/// </summary>
		[JsonProperty("docDateTime", Required = Required.Always)]
		public string DocDateTime { get; set; }

		/// <summary>
		/// Номер документа, который регламентирует заполнение отраслевых реквизитов.
		/// Постановление Правительства РФ от 21.11.2023 № 1944
		/// </summary>
		[JsonProperty("docNumber", Required = Required.Always)]
		public string DocNumber { get; set; }

		/// <summary>
		/// Данные об идентификаторе и времени запроса проверки КМ. (Получает с помощью терминала сбора данных)
		/// </summary>
		[JsonProperty("docData", Required = Required.Always)]
		public string DocData { get; set; }
	}
}
