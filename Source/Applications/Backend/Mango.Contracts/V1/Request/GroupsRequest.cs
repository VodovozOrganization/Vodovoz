using System.Text.Json.Serialization;

namespace Mango.Contracts.V1.Request
{
	public class GroupsRequest
	{
		[JsonPropertyName("show_users")]
		public int ShowUsers { get; set; } = 1;
	}
}
