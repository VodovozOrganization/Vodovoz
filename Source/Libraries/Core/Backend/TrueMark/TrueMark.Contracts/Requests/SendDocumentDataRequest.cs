using System.Text.Json.Serialization;

namespace TrueMark.Contracts.Requests
{
	/// <summary>
	/// Отправка документа в ЧЗ
	/// </summary>
	public class SendDocumentDataRequest
	{
		/// <summary>
		/// Документ
		/// </summary>
		public string Document { get; set; }

		/// <summary>
		/// Инн организации
		/// </summary>
		public string Inn { get; set; }

		/// <summary>
		/// Товарная группа маркировки: в JSON уходит как water/petfood,
		/// значение для параметра pg= даёт <see cref="TrueMarkProductGroupExtensions.ToApiValue"/>.
		/// </summary>
		[JsonConverter(typeof(TrueMarkProductGroupJsonConverter))]
		public TrueMarkProductGroup? ProductGroup { get; set; }
	}
}
