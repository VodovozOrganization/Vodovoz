using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Responses
{
	/// <summary>
	/// Общие данные сотрудника ВАТС
	/// </summary>
	public class VpbxUserGeneral
	{
		/// <summary>
		/// Id сотрудника.
		/// Заполняется, только если в запросе указано дополнительное поле user_id
		/// </summary>
		[JsonPropertyName("user_id")]
		public long? UserId { get; set; }

		/// <summary>
		/// ФИО сотрудника
		/// </summary>
		[JsonPropertyName("name")]
		public string Name { get; set; }

		/// <summary>
		/// Адрес электронной почты
		/// </summary>
		[JsonPropertyName("email")]
		public string Email { get; set; }

		/// <summary>
		/// Отдел
		/// </summary>
		[JsonPropertyName("department")]
		public string Department { get; set; }

		/// <summary>
		/// Должность
		/// </summary>
		[JsonPropertyName("position")]
		public string Position { get; set; }

		/// <summary>
		/// Id роли сотрудника.
		/// Заполняется, только если в запросе указано дополнительное поле access_role_id
		/// </summary>
		[JsonPropertyName("access_role_id")]
		public long? AccessRoleId { get; set; }

		/// <summary>
		/// Мобильный телефон.
		/// Заполняется, только если в запросе указано дополнительное поле mobile
		/// </summary>
		[JsonPropertyName("mobile")]
		public string Mobile { get; set; }

		/// <summary>
		/// Логин.
		/// Заполняется, только если в запросе указано дополнительное поле login
		/// </summary>
		[JsonPropertyName("login")]
		public string Login { get; set; }

		/// <summary>
		/// SIP-учётные записи сотрудника.
		/// Заполняется, только если в запросе указано дополнительное поле sips
		/// </summary>
		[JsonPropertyName("sips")]
		public IReadOnlyList<VpbxSipAccount> Sips { get; set; }
	}
}
