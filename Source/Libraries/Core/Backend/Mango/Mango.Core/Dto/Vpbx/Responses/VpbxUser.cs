using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Responses
{
	/// <summary>
	/// Сотрудник ВАТС
	/// </summary>
	public class VpbxUser
	{
		/// <summary>
		/// Общие данные сотрудника
		/// </summary>
		[JsonPropertyName("general")]
		public VpbxUserGeneral General { get; set; }

		/// <summary>
		/// Телефония сотрудника
		/// </summary>
		[JsonPropertyName("telephony")]
		public VpbxUserTelephony Telephony { get; set; }

		/// <summary>
		/// Id групп, в которых состоит сотрудник.
		/// Заполняется, только если в запросе указано дополнительное поле groups
		/// </summary>
		[JsonPropertyName("groups")]
		public IReadOnlyList<long> Groups { get; set; }
	}
}
