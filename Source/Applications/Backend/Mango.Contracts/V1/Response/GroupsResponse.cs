using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mango.Contracts.V1.Response
{
	public class GroupsResponse
	{
		[JsonPropertyName("result")]
		public int Result { get; set; }

		[JsonPropertyName("groups")]
		public List<MangoGroup> Groups { get; set; }
	}
}
