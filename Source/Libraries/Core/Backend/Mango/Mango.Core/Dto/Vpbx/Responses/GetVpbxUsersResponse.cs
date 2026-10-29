using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Responses
{
	/// <summary>
	/// Ответ на запрос списка сотрудников ВАТС.
	/// Поле result данным методом не возвращается
	/// </summary>
	public class GetVpbxUsersResponse : VpbxResponseBase
	{
		/// <summary>
		/// Сотрудники ВАТС
		/// </summary>
		[JsonPropertyName("users")]
		public IReadOnlyList<VpbxUser> Users { get; set; }
	}
}
