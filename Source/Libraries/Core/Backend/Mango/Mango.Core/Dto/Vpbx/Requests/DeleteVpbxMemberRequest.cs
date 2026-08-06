using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Requests
{
	/// <summary>
	/// Запрос удаления сотрудника ВАТС (POST /vpbx/member/delete)
	/// </summary>
	public class DeleteVpbxMemberRequest
	{
		/// <summary>
		/// Id сотрудника ВАТС. Обязательное поле.
		/// Соответствует полю general.user_id в ответе на запрос списка сотрудников
		/// </summary>
		[JsonPropertyName("user_id")]
		public string UserId { get; set; }
	}
}
