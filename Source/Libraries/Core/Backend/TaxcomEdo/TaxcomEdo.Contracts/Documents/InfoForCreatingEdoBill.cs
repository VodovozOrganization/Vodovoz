using TaxcomEdo.Contracts.Orders;

namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Необходимая информация для отправки счета по ЭДО
	/// </summary>
	public class InfoForCreatingEdoBill : InfoForCreatingDocumentEdoWithAttachment
	{
		public static readonly string ExchangeAndQueueName = "info-for-create-bills";

		/// <summary>
		/// Конструктор, нужен для десериализации из Json
		/// </summary>
		public InfoForCreatingEdoBill() { }
		
		/// <summary>
		/// Информация о заказе для ЭДО <see cref="OrderInfoForEdo"/>
		/// </summary>
		public OrderInfoForEdo OrderInfoForEdo { get; set; }
		
		public BillFileData BillFileData
		{
			get => FileData as BillFileData;
			set => FileData = value;
		}
	}
}
