using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Responses
{
	/// <summary>
	/// Ответ на запрос создания сотрудника ВАТС
	/// </summary>
	public class CreateVpbxMemberResponse : VpbxResponseBase
	{
		/// <summary>
		/// Id созданного сотрудника
		/// </summary>
		[JsonPropertyName("user_id")]
		public long? UserId { get; set; }
	}
}
