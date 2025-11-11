using TaxcomEdo.Contracts.Orders;

namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Необходимая информация для отправки акта приёма-передачи оборудования по ЭДО
	/// </summary>
	public class InfoForCreatingEdoEquipmentTransfer : InfoForCreatingDocumentEdoWithAttachment
	{
		public static readonly string ExchangeAndQueueName = "info-for-create-equipment-transfer";

		/// <summary>
		/// Конструктор, нужен для десериализации из Json
		/// </summary>
		public InfoForCreatingEdoEquipmentTransfer() { }
		
		/// <summary>
		/// Информация о заказе для ЭДО <see cref="OrderInfoForEdo"/>
		/// </summary>
		public OrderInfoForEdo OrderInfoForEdo { get; set; }
		
		/// <summary>
		/// Информация о прикрепленном файле <see cref="EquipmentTransferFileData"/>
		/// </summary>
		public EquipmentTransferFileData EquipmentTransferFileData
		{
			get => FileData as EquipmentTransferFileData;
			set => FileData = value;
		}
	}
}

