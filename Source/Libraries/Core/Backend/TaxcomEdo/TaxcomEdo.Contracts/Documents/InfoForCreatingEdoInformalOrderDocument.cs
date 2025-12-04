using TaxcomEdo.Contracts.Orders;

namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Необходимая информация для отправки неформализованного документа заказа по ЭДО
	/// </summary>
	public class InfoForCreatingEdoInformalOrderDocument : InfoForCreatingDocumentEdoWithAttachment
	{
		public static readonly string ExchangeAndQueueName = "info-for-create-informal-order-document";

		/// <summary>
		/// Конструктор, нужен для десериализации из Json
		/// </summary>
		public InfoForCreatingEdoInformalOrderDocument() { }
		
		/// <summary>
		/// Информация об организации ЭДО <see cref="EdoParticipantInfo"/>
		/// </summary>
		public EdoParticipantInfo OrganizationInfoForEdo { get; set; }

		/// <summary>
		/// Информация о клиенте ЭДО <see cref="EdoParticipantInfo"/>
		/// </summary>
		public EdoParticipantInfo CounterpartyInfoForEdo { get; set; }

		/// <summary>
		/// Сумма документа
		/// </summary>
		public decimal Sum { get; set; }

		/// <summary>
		/// Информация о прикрепленном файле <see cref="OrderDocumentFileData"/>
		/// </summary>
		public new OrderDocumentFileData FileData { get; set; }
	}
}

