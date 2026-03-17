using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mango.Contracts.V1.Response
{
	public class GroupsResponse
	{
		[JsonPropertyName("result")]
		public int Result { get; set; }

		[JsonPropertyName("groups")]
		public List<MangoGroup>? Groups { get; set; }
	}
	
	public sealed class MangoGroup
	{
		[JsonPropertyName("id")]
		public long Id { get; set; }

		[JsonPropertyName("name")]
		public string? Name { get; set; }

		[JsonPropertyName("description")]
		public string? Description { get; set; }

		[JsonPropertyName("extension")]
		public string? Extension { get; set; }

		[JsonPropertyName("operators")]
		public List<MangoOperator>? Operators { get; set; }
	}

	public sealed class MangoOperator
	{
		[JsonPropertyName("id")]
		public long Id { get; set; }

		[JsonPropertyName("name")]
		public string? Name { get; set; }

		[JsonPropertyName("extension")]
		public string? Extension { get; set; }

		[JsonPropertyName("priority")]
		public int? Priority { get; set; }

		[JsonPropertyName("order")]
		public int? Order { get; set; }
	}
}
