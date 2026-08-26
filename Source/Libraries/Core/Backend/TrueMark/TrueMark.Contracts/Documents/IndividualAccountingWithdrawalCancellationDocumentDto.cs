using System.Text.Json.Serialization;

namespace TrueMark.Contracts.Documents
{
	/// <summary>
	/// Документ отмены вывода из оборота индивидуального учета.
	/// </summary>
	public class IndividualAccountingWithdrawalCancellationDocumentDto
	{
		/// <summary>
		/// ИНН участника оборота товаров.
		/// </summary>
		[JsonPropertyName("inn")]
		public string Inn { get; set; }

		/// <summary>
		/// Идентификатор отменяемого документа вывода из оборота.
		/// </summary>
		[JsonPropertyName("lk_receipt_id")]
		public string LkReceiptId { get; set; }
	}
}
