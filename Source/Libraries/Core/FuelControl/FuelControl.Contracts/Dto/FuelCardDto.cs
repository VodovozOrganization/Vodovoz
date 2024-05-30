using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Топливная карта
	/// </summary>
	public class FuelCardDto
	{
		/// <summary>
		/// ID топливной карты
		/// </summary>
		[JsonPropertyName("id")]
		public string CardId { get; set; }

		/// <summary>
		/// ID группы карт, к которой принадлежит карта
		/// </summary>
		[JsonPropertyName("group_id")]
		public string GroupId { get; set; }

		/// <summary>
		/// Название группы карт, к которой принадлежит карта
		/// </summary>
		[JsonPropertyName("group_name")]
		public string GroupName { get; set; }

		/// <summary>
		/// ID договора
		/// </summary>
		[JsonPropertyName("contract_id")]
		public string ContractId { get; set; }

		/// <summary>
		/// Наименование договора
		/// </summary>
		[JsonPropertyName("contract_name")]
		public string ContractName { get; set; }

		/// <summary>
		/// Номер карты
		/// </summary>
		[JsonPropertyName("number")]
		public string Number { get; set; }

		/// <summary>
		/// ID статуса карты
		/// </summary>
		[JsonPropertyName("status")]
		public string Status { get; set; }

		/// <summary>
		/// Название статуса карты
		/// </summary>
		[JsonPropertyName("status_name")]
		public string StatusName { get; set; }

		/// <summary>
		/// Комментарий карты
		/// </summary>
		[JsonPropertyName("comment")]
		public string Comment { get; set; }

		/// <summary>
		/// ID типа продукта
		/// </summary>
		[JsonPropertyName("product")]
		public string Product { get; set; }

		/// <summary>
		/// Название типа продукта
		/// </summary>
		[JsonPropertyName("product_name")]
		public string ProductName { get; set; }

		/// <summary>
		/// Тип карты
		/// </summary>
		[JsonPropertyName("carrier")]
		public string Carrier { get; set; }

		/// <summary>
		/// Название типа карты
		/// </summary>
		[JsonPropertyName("carrier_name")]
		public string CarrierName { get; set; }

		/// <summary>
		/// Возможность использования услуги Платон
		/// </summary>
		[JsonPropertyName("platon")]
		public bool IsCanUsePlaton { get; set; }

		/// <summary>
		/// Возможность использования услуги Автодор
		/// </summary>
		[JsonPropertyName("avtodor")]
		public bool IsCanUseAvtodor { get; set; }

		/// <summary>
		/// Статус синхронизации группы карт
		/// </summary>
		[JsonPropertyName("sync_group_state")]
		public string SyncGroupState { get; set; }

		/// <summary>
		/// Список ID пользователей к которым привязана карта
		/// </summary>
		[JsonPropertyName("users")]
		public IEnumerable<string> Users { get; set; }

		/// <summary>
		/// Существует ли МПК на карте
		/// </summary>
		[JsonPropertyName("mpc")]
		public string Mpc { get; set; }
	}
}
