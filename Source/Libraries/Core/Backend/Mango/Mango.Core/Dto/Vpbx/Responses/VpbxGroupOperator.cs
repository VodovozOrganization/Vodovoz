using System.Text.Json.Serialization;

namespace Mango.Core.Dto.Vpbx.Responses
{
	/// <summary>
	/// Сотрудник в составе группы ВАТС
	/// </summary>
	public class VpbxGroupOperator
	{
		/// <summary>
		/// Id сотрудника. Соответствует полю general.user_id в ответе на запрос списка сотрудников
		/// </summary>
		[JsonPropertyName("id")]
		public long Id { get; set; }

		/// <summary>
		/// Имя сотрудника
		/// </summary>
		[JsonPropertyName("name")]
		public string Name { get; set; }

		/// <summary>
		/// Внутренний номер сотрудника
		/// </summary>
		[JsonPropertyName("extension")]
		public string Extension { get; set; }

		/// <summary>
		/// Приоритет в алгоритмах распределения звонков, использующих приоритет
		/// </summary>
		[JsonPropertyName("priority")]
		public int? Priority { get; set; }

		/// <summary>
		/// Порядок в алгоритмах распределения звонков, использующих порядок.
		/// Присваивается автоматически, зависит от очерёдности добавления сотрудников в группу
		/// </summary>
		[JsonPropertyName("order")]
		public int? Order { get; set; }
	}
}
