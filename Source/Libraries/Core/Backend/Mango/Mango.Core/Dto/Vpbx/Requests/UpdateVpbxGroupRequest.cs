using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Requests
{
	/// <summary>
	/// Запрос редактирования группы ВАТС (POST /vpbx/group/update).
	/// Изменяются только переданные поля, остальные настройки группы сохраняются
	/// </summary>
	public class UpdateVpbxGroupRequest
	{
		/// <summary>
		/// Id редактируемой группы. Обязательное поле
		/// </summary>
		[JsonPropertyName("group_id")]
		public string GroupId { get; set; }

		/// <summary>
		/// Изменяемые данные группы
		/// </summary>
		[JsonPropertyName("group")]
		public VpbxGroupUpdate Group { get; set; }
	}
}
