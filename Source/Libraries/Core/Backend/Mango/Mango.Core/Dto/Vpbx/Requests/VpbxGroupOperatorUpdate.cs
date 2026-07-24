using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Requests
{
	/// <summary>
	/// Сотрудник в составе группы ВАТС при её редактировании
	/// </summary>
	public class VpbxGroupOperatorUpdate
	{
		/// <summary>
		/// Id сотрудника. Соответствует полю user_id в ответе на запрос списка сотрудников
		/// </summary>
		[JsonPropertyName("id")]
		public string Id { get; set; }
	}
}
