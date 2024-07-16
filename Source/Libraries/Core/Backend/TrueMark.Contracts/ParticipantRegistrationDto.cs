using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace TrueMark.Contracts
{
	/// <summary>
	/// Статус регистрации участника
	/// </summary>
	public class ParticipantRegistrationDto
	{
		/// <summary>
		/// ИНН
		/// </summary>
		[JsonPropertyName("inn")]
		public string Inn { get; set; }

		/// <summary>
		/// Зарегистрирован?
		/// </summary>
		[JsonPropertyName("is_registered")]
		public bool IsRegistered { get; set; }

		/// <summary>
		/// Статус
		/// </summary>
		[JsonPropertyName("status")]
		public string Status { get; set; }

		/// <summary>
		/// Ошибка
		/// </summary>
		[JsonPropertyName("error_message")]
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Группы продуктов, для которых зарегистрирован участник
		/// </summary>
		[JsonPropertyName("productGroups")]
		public IEnumerable<string> ProductGroups { get; set; }

		/// <summary>
		/// Зарегистрирован для воды?
		/// </summary>
		[JsonIgnore]
		public bool IsRegisteredForWater => ProductGroups != null && ProductGroups.Any(pg => pg == "water") && IsRegistered;
	}
}
