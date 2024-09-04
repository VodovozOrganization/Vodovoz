using TaxcomEdo.Contracts.Orders;

namespace TaxcomEdo.Contracts.Documents
{
	/// <summary>
	/// Необходимая информация для отправки счета по ЭДО
	/// </summary>
	public class InfoForCreatingEdoBill : InfoForCreatingDocumentEdoWithAttachment
	{
		protected InfoForCreatingEdoBill(OrderInfoForEdo orderInfoForEdo, FileData fileData) : base(fileData)
		{
			OrderInfoForEdo = orderInfoForEdo;
		}
		
		/// <summary>
		/// Информация о заказе для ЭДО <see cref="OrderInfoForEdo"/>
		/// </summary>
		public OrderInfoForEdo OrderInfoForEdo { get; }

		public static InfoForCreatingEdoBill Create(OrderInfoForEdo orderInfoForEdo, FileData fileData) =>
			new InfoForCreatingEdoBill(orderInfoForEdo, fileData);
	}
}
