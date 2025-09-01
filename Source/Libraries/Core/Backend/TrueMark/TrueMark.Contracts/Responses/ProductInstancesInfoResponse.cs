using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrueMark.Contracts.Responses
{
	/// <summary>
	/// Информация о статусе регистрации участника
	/// </summary>
	public class ProductInstancesInfoResponse
	{
		/// <summary>
		/// Статус экземпляров товаров
		/// </summary>
		[JsonPropertyName("instanceStatuses")]
		public IEnumerable<ProductInstanceStatus> InstanceStatuses { get; set; }

		/// <summary>
		/// Ошибки
		/// </summary>
		[JsonPropertyName("errorMessage")]
		public string ErrorMessage { get; set; }

		/// <summary>
		/// В ЧЗ нет информации ни по одному из переданных идентификационных кодов
		/// </summary>
		[JsonPropertyName("noCodesFound")]
		public bool NoCodesFound { get; set; }
	}
}
