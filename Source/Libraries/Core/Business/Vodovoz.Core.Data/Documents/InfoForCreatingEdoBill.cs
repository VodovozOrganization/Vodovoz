using Vodovoz.Core.Data.Orders;

namespace Vodovoz.Core.Data.Documents
{
	public class InfoForCreatingEdoBill : InfoForCreatingDocumentEdo
	{
		protected InfoForCreatingEdoBill(OrderInfoForEdo orderInfoForEdo)
		{
			OrderInfoForEdo = orderInfoForEdo;
		}
		
		public OrderInfoForEdo OrderInfoForEdo { get; }

		public static InfoForCreatingEdoBill Create(OrderInfoForEdo orderInfoForEdo) =>
			new InfoForCreatingEdoBill(orderInfoForEdo);
	}
}
