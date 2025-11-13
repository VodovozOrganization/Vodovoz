using Edo.Contracts.Messages.Dto;

namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие для создания УПД по ЭДО
	/// </summary>
	public class TaxcomDocflowSendEvent
	{
		public const string Event = "taxcom-docflow-send";
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
