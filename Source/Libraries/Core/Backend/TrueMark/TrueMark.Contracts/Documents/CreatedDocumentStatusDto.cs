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
		/// Ошибки при создании документа. Если список пустой, значит документ был успешно создан
		/// </summary>
		[JsonPropertyName("errors")]
		public IList<string> Errors { get; set; }
	}
}
