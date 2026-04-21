using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mango.Contracts.V1.Response
{
	public class MangoGroup
	{
		[JsonPropertyName("id")]
		public long Id { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("description")]
		public string Description { get; set; }

		[JsonPropertyName("extension")]
		public string Extension { get; set; }

		[JsonPropertyName("operators")]
		public List<MangoOperator> Operators { get; set; }
	}
}
