using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Requests
{
	/// <summary>
	/// Запрос списка групп ВАТС (POST /vpbx/groups)
	/// </summary>
	public class GetVpbxGroupsRequest
	{
		/// <summary>
		/// Id группы. Если не заполнен, возвращаются все группы (поле в запрос не попадает)
		/// </summary>
		[JsonPropertyName("group_id")]
		public string GroupId { get; set; }

		/// <summary>
		/// Выводить ли в ответе сотрудников в группах: 0 - нет, 1 - да
		/// </summary>
		[JsonPropertyName("show_users")]
		public int ShowUsers { get; set; } = 1;
	}
}
