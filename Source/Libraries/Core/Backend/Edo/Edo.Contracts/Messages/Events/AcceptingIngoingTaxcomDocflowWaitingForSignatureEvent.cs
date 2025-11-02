using System;

namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие для подписания входящего документооборота
	/// </summary>
	public class AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent
	{
		public const string Event = "accepting-ingoing-taxcom-Docflow-waiting-for-signature";
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
		/// <summary>
		/// Название организации
		/// </summary>
		public string Organization { get; set; }
	}
}
