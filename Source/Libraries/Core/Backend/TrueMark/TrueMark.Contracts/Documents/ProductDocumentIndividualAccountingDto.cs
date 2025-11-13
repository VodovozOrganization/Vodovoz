using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrueMark.Contracts.Documents
{
	/// <summary>
	/// Документ для отправки в ЧЗ с товарами подлежащими индивидуальному учету
	/// </summary>
	public class ProductDocumentIndividualAccountingDto
	{
		/// <summary>
		/// Дата вывода из оборота
		/// </summary>
		[JsonIgnore]
		public DateTime ActionDate { get; set; }

		/// <summary>
		/// Дата первичногодокумента
		/// </summary>
		[JsonIgnore]
		public DateTime DocumentDate { get; set; }

		/// <summary>
		/// ИНН участника оборота товаров
		/// </summary>
		[JsonPropertyName("inn")]
		public string Inn { get; set; }

		/// <summary>
		/// ИНН покупателя
		/// </summary>
		[JsonPropertyName("buyer_inn")]
		public string BuyerInn { get; set; }

		/// <summary>
		/// Причина выбытия
		/// </summary>
		[JsonPropertyName("action")]
		public string Action { get; set; }

		/// <summary>
		/// Тип первчиного документа
		/// </summary>
		[JsonPropertyName("document_type")]
		public string DocumentType { get; set; }

		/// <summary>
		/// Массив, содержащий список КИ / КиЗ
		/// </summary>
		[JsonPropertyName("products")]
		public IEnumerable<ProductIndividualAccountingDto> Products { get; set; }

		/// <summary>
		/// Дата вывода из оборота
		/// </summary>
		[JsonPropertyName("action_date")]
		public string ActionDateString => ActionDate.ToString("yyyy-MM-dd");

		/// <summary>
		/// Дата первичногодокумента
		/// </summary>
		[JsonPropertyName("document_date")]
		public string DocumentDateString => DocumentDate.ToString("yyyy-MM-dd");

		/// <summary>
		/// Номер первичного документа
		/// </summary>
		[JsonPropertyName("document_number")]
		public string DocumentNumber { get; set; }

		/// <summary>
		/// "Наименование первичного документа"
		/// </summary>
		[JsonPropertyName("primary_document_custom_name")]
		public string PrimaryDocumentCustomName { get; set; }
	}
}
