using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Responses
{
	/// <summary>
	/// Ответ на запрос списка групп ВАТС
	/// </summary>
	public class GetVpbxGroupsResponse : VpbxResponseBase
	{
		/// <summary>
		/// Группы ВАТС
		/// </summary>
		[JsonPropertyName("groups")]
		public IReadOnlyList<VpbxGroup> Groups { get; set; }
	}
}
