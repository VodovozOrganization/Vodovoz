using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Товарный ограничитель
	/// </summary>
	public class FuelCardProductRestrictionDto
	{
		/// <summary>
		/// ID товарного ограничителя
		/// </summary>
		[JsonPropertyName("id")]
		public string Id { get; set; }

		/// <summary>
		/// ID карты
		/// </summary>
		[JsonPropertyName("card_id")]
		public string CardId { get; set; }

		/// <summary>
		/// ID группы карт
		/// </summary>
		[JsonPropertyName("group_id")]
		public string CardGroupId { get; set; }

		/// <summary>
		/// ID договора
		/// </summary>
		[JsonPropertyName("contract_id")]
		public string ContractId { get; set; }

		/// <summary>
		/// ID типа продукта
		/// </summary>
		[JsonPropertyName("productType")]
		public string ProductTypeId { get; set; }

		/// <summary>
		/// ID группы продукта
		/// </summary>
		[JsonPropertyName("productGroup")]
		public string ProductGroupId { get; set; }

		/// <summary>
		/// Название типа продукта
		/// </summary>
		[JsonPropertyName("productTypeName")]
		public string ProductTypeName { get; set; }

		/// <summary>
		/// Название группы продукта
		/// </summary>
		[JsonPropertyName("productGroupName")]
		public string ProductGroupName { get; set; }

		/// <summary>
		/// Дата последнего изменения
		/// </summary>
		[JsonPropertyName("date")]
		public string Date { get; set; }

		/// <summary>
		/// Тип ограничения
		/// 1 – Разрешающий ограничитель,
		/// 2 – Запрещающий ограничитель,
		/// 3 – Тип R
		/// </summary>
		[JsonPropertyName("restriction_type")]
		public uint RestrictionType { get; set; }
	}
}
