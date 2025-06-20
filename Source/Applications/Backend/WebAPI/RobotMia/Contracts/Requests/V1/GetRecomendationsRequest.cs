using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Vodovoz.RobotMia.Contracts.Requests.V1
{
	/// <summary>
	/// Запрос на получение рекомендаций
	/// </summary>
	public class GetRecomendationsRequest
	{
		/// <summary>
		/// Идентификатор звонка
		/// </summary>
		[JsonPropertyName("call_id"), Required]
		public Guid CallId { get; set; }

		/// <summary>
		/// Идентификатор контрагента
		/// </summary>
		[JsonPropertyName("counterparty_id"), Required]
		public int CounterpartyId { get; set; }

		/// <summary>
		/// Идентификатор точки доставки
		/// </summary>
		[JsonPropertyName("delivery_point_id"), Required]
		public int DeliveryPointId { get; set; }

		/// <summary>
		/// Идентификаторы уже добавленных в корзину товаров
		/// </summary>
		[JsonPropertyName("added_nomenclature_ids")]
		public IEnumerable<int> AddedNomenclatureIds { get; set; }
	}
}
