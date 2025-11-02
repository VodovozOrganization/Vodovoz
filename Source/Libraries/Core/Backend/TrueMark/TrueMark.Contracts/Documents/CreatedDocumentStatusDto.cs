using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrueMark.Contracts.Documents
{
	/// <summary>
	/// Результат создания документа
	/// </summary>
	public class CreatedDocumentInfoDto
	{
		[JsonPropertyName("errors")]
		public IList<string> Errors { get; set; }
	}
}
