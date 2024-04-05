using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Данные договора
	/// </summary>
	public class UserContractDto
	{
		/// <summary>
		/// ID договора
		/// </summary>
		[JsonPropertyName("id")]
		public string Id { get; set; }

		/// <summary>
		/// Номер договора
		/// </summary>
		[JsonPropertyName("number")]
		public string Number { get; set; }

		/// <summary>
		/// Возможность выпуска МПК
		/// </summary>
		[JsonPropertyName("mpc")]
		public bool IsUserCanReleaseMpc { get; set; }

		/// <summary>
		/// ID шаблона ВК
		/// </summary>
		[JsonPropertyName("template_id")]
		public string TemplateId { get; set; }

		/// <summary>
		/// Количество топливных карт на договоре
		/// </summary>
		[JsonPropertyName("cards_count")]
		public int CardsCount { get; set; }

		/// <summary>
		/// Единая цена
		/// </summary>
		[JsonPropertyName("one_price")]
		public bool IsOnePrice { get; set; }
	}
}
