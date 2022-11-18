using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrueMarkApi.Dto.Documents
{
	public class CreatedDocumentInfoDto
	{
		[JsonPropertyName("errors")]
		public IList<string> Errors { get; set; }
	}
}
