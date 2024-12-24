using System;

namespace Edo.Transport.Messages.Events
{
	/// <summary>
	/// Событие для подписания входящего документооборота
	/// </summary>
	public class AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent
	{
		public const string Event = "AcceptingIngoingTaxcomDocflowWaitingForSignature";
		/// <summary>
		/// Код кабинета в ЭДО
		/// </summary>
		public string EdoAccount { get; set; }
		/// <summary>
		/// Id главного документа
		/// </summary>
		public string MainDocumentId { get; set; }
		/// <summary>
		/// Id документооборота
		/// </summary>
		public Guid? DocFlowId { get; set; }
	}
}
