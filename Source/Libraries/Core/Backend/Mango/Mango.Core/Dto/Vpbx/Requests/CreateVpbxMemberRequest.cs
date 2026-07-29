using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Requests
{
	/// <summary>
	/// Запрос создания сотрудника ВАТС (POST /vpbx/member/create)
	/// </summary>
	public class CreateVpbxMemberRequest
	{
		/// <summary>
		/// ФИО сотрудника. Обязательное поле
		/// </summary>
		[JsonPropertyName("name")]
		public string Name { get; set; }

		/// <summary>
		/// Внутренний номер сотрудника. Обязательное поле
		/// </summary>
		[JsonPropertyName("extension")]
		public string Extension { get; set; }

		/// <summary>
		/// Id роли сотрудника. Обязательное поле.
		/// Получить список ролей можно запросом POST /vpbx/config/roles
		/// </summary>
		[JsonPropertyName("access_role_id")]
		public string AccessRoleId { get; set; }

		/// <summary>
		/// Id исходящей линии.
		/// Получить список линий можно запросом POST /vpbx/config/incominglines
		/// </summary>
		[JsonPropertyName("line_id")]
		public string LineId { get; set; }

		/// <summary>
		/// Алгоритм дозвона, допустимые значения 0..2
		/// </summary>
		[JsonPropertyName("dial_alg")]
		public int DialAlg { get; set; } = 1;

		/// <summary>
		/// Адрес электронной почты
		/// </summary>
		[JsonPropertyName("email")]
		public string Email { get; set; }

		/// <summary>
		/// Мобильный телефон
		/// </summary>
		[JsonPropertyName("mobile")]
		public string Mobile { get; set; }

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
		/// Настройки средств дозвона. Порядок элементов определяет порядок использования
		/// </summary>
		[JsonPropertyName("numbers")]
		public IEnumerable<VpbxMemberNumber> Numbers { get; set; }
	}
}
