using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace FuelControl.Contracts.Dto
{
	/// <summary>
	/// Данные пользователя
	/// </summary>
	public class UserDataDto
	{
		/// <summary>
		/// ID клиента
		/// </summary>
		[JsonPropertyName("client_id")]
		public string ClientId { get; set; }

		/// <summary>
		/// Статус пользователя
		/// </summary>
		[JsonPropertyName("client_status")]
		public string ClientStatus { get; set; }

		/// <summary>
		/// Название организации
		/// </summary>
		[JsonPropertyName("org_name")]
		public string OrganizationName { get; set; }

		/// <summary>
		/// ID текущей сессии пользователя
		/// </summary>
		[JsonPropertyName("session_id")]
		public string SessionId { get; set; }

		/// <summary>
		/// ID пользователя
		/// </summary>
		[JsonPropertyName("user_id")]
		public string UserId { get; set; }

		/// <summary>
		/// Данные договоров доступных пользователю
		/// </summary>
		[JsonPropertyName("contracts")]
		public IEnumerable<UserContractDto> UserContracts { get; set; }

		/// <summary>
		/// ID роли пользователя
		/// </summary>
		[JsonPropertyName("role_id")]
		public string RoleId { get; set; }

		/// <summary>
		/// Название роли пользователя
		/// </summary>
		[JsonPropertyName("role_name")]
		public string RolName { get; set; }

		/// <summary>
		/// Режим чтения
		/// </summary>
		[JsonPropertyName("read_only")]
		public bool IsReadOnly { get; set; }

		/// <summary>
		/// Имя пользователя
		/// </summary>
		[JsonPropertyName("user_name")]
		public string UserName { get; set; }

		/// <summary>
		/// Отчество пользователя
		/// </summary>
		[JsonPropertyName("user_patronymic")]
		public string UuserPatronymic { get; set; }

		/// <summary>
		/// Фамилия пользователя
		/// </summary>
		[JsonPropertyName("user_surname")]
		public string UserSurname { get; set; }

		/// <summary>
		/// Последний используемый договор
		/// </summary>
		[JsonPropertyName("last_contract")]
		public string LastContract { get; set; }

		/// <summary>
		/// Доступ в ЛК/МП/API
		/// </summary>
		[JsonPropertyName("access")]
		public UserAccessDto UserAccess { get; set; }

		/// <summary>
		/// Email
		/// </summary>
		[JsonPropertyName("email")]
		public string Email { get; set; }

		/// <summary>
		/// Телефон
		/// </summary>
		[JsonPropertyName("phone")]
		public string Phone { get; set; }
	}
}
