using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TrueMark.Contracts.Documents
{
	public class ProductDocumentDto
	{
		[JsonIgnore]
		public DateTime ActionDate { get; set; }

		[JsonIgnore]
		public DateTime DocumentDate { get; set; }

		[JsonPropertyName("inn")]
		public string Inn { get; set; }

		[JsonPropertyName("buyer_inn")]
		public string BuyerInn { get; set; }

		[JsonPropertyName("action")]
		public string Action { get; set; }

		[JsonPropertyName("document_type")]
		public string DocumentType { get; set; }

		[JsonPropertyName("products")]
		public IList<ProductDto> Products { get; set; }

		[JsonPropertyName("action_date")]
		public string ActionDateString => ActionDate.ToString("yyyy-MM-dd");

		[JsonPropertyName("document_date")]
		public string DocumentDateString => DocumentDate.ToString("yyyy-MM-dd");

		[JsonPropertyName("document_number")]
		public string DocumentNumber { get; set; }

		[JsonPropertyName("primary_document_custom_name")]
		public string PrimaryDocumentCustomName { get; set; }

	}
}
