using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Requests
{
	/// <summary>
	/// Запрос списка сотрудников ВАТС (POST /vpbx/config/users/request)
	/// </summary>
	public class GetVpbxUsersRequest
	{
		/// <summary>
		/// Внутренний номер сотрудника, настройки которого запрашиваются.
		/// Если не заполнен, возвращается полный список сотрудников
		/// </summary>
		[JsonPropertyName("extension")]
		public string Extension { get; set; }

		/// <summary>
		/// Список дополнительных полей, которые нужно вернуть в ответе
		/// </summary>
		[JsonPropertyName("ext_fields")]
		public IEnumerable<string> ExtFields { get; set; } = new[]
		{
			"general.user_id",
			"groups",
			"telephony.line_id",
			"telephony.outgoingline"
		};
	}
}
