using TaxcomEdo.Contracts.Documents;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Contracts.Messages.Events
{
	/// <summary>
	/// Событие для создания неформализованного документа (PDF) по ЭДО
	/// </summary>
	public class TaxcomDocflowEquipmentTransferSendEvent
	{
		public const string Event = "taxcom-docflow-equipment-transfer-send";
		/// <summary>
		/// Код кабинета в ЭДО, от имени которого отправляется документ
		/// </summary>
		public string EdoAccount { get; set; }
		/// <summary>
		/// Id исходящего документа по ЭДО
		/// </summary>
		public int EdoOutgoingDocumentId { get; set; }
		/// <summary>
		/// Тип неформализованного документа
		/// </summary>
		public EdoDocumentType DocumentType { get; set; }
		/// <summary>
		/// Информация для создания неформализованного документа по ЭДО
		/// </summary>
		public InfoForCreatingEdoEquipmentTransfer DocumentInfo { get; set; }
	}
}

