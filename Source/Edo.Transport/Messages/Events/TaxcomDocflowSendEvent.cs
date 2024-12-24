using Edo.Transport.Messages.Dto;

namespace Edo.Transport.Messages.Events
{
	/// <summary>
	/// Событие для создания УПД по ЭДО
	/// </summary>
	public class TaxcomDocflowSendEvent
	{
		public const string Event = "TaxcomDocflowSend";
		/// <summary>
		/// Код кабинета в ЭДО, от имени которого отправляется документ
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
