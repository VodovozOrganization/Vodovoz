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
		/// Информация о заказе для ЭДО <see cref="OrderInfoForEdo"/>
		/// </summary>
		public OrderInfoForEdo OrderInfoForEdo { get; set; }
		
		/// <summary>
		/// Информация о прикрепленном файле <see cref="EquipmentTransferFileData"/>
		/// </summary>
		public OrderDocumentFileData EquipmentTransferFileData
		{
			get => FileData as OrderDocumentFileData;
			set => FileData = value;
		}
	}
}

