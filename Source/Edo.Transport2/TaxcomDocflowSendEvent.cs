using Edo.Docflow.Dto;

namespace Edo.Transport2
{
	/// <summary>
	/// Событие для создания УПД по ЭДО
	/// </summary>
	public class TaxcomDocflowSendEvent
	{
		public const string Event = "TaxcomDocflowSend";
		/// <summary>
		/// Код кабинета в ЭДО
		/// </summary>
		public string EdoAccount { get; set; }
		/// <summary>
		/// Id исходящего документа по ЭДО
		/// </summary>
		public int EdoOutgoingDocumentId { get; set; }
		/// <summary>
		/// Информация для создания УПД по ЭДО
		/// </summary>
		public UniversalTransferDocumentInfo UpdInfo { get; set; }
	}
}
