using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrueMark.Contracts.Documents
{
	/// <summary>
	/// Результат создания документа
	/// </summary>
	public class CreatedDocumentInfoDto
	{
		/// <summary>
		/// Идентификатор документа в Честном знаке
		/// </summary>
		[JsonPropertyName("number")]
		public string Number {  get; set; }

		/// <summary>
		/// Статус обработки документа в Честном знаке
		/// </summary>
		[JsonPropertyName("status")]
		public string Status { get; set; }

		/// <summary>
		/// Ошибки обработки документа. Финальный результат определяется по <see cref="Status"/>.
		/// </summary>
		[JsonPropertyName("errors")]
		public IList<string> Errors { get; set; }

		/// <summary>
		/// В документе есть ошибки
		/// </summary>
		[JsonIgnore]
		public bool HasErrors => Errors != null && Errors.Count > 0;
	}
}
