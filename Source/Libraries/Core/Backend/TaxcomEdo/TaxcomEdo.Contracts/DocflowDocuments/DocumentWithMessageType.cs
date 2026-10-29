namespace TaxcomEdo.Contracts.DocflowDocuments
{
	public enum DocumentWithMessageType
	{
		/// <summary>
		/// Принят с расхождениями
		/// </summary>
		CompletedWithDiscrepancy,

		/// <summary>
		/// Уведомление об уточнении
		/// </summary>
		CorrectionNotice,

		/// <summary>
		/// Предложение об аннулировании
		/// </summary>
		CancellationOffer
	}
}
