using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrueMark.Contracts.Documents
{
	public class CreatedDocumentInfoDto
	{
		[JsonPropertyName("errors")]
		public IList<string> Errors { get; set; }
		[JsonPropertyName("senderInn")]
		public string SenderInn { get; set; }
	}
}
