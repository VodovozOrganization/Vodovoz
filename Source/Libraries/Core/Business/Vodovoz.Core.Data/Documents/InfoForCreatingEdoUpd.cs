using System;
using Vodovoz.Core.Data.Orders;

namespace Vodovoz.Core.Data.Documents
{
	public class InfoForCreatingEdoUpd : InfoForCreatingDocumentEdo
	{
		protected InfoForCreatingEdoUpd(OrderInfoForEdo orderInfoForEdo)
		{
			OrderInfoForEdo = orderInfoForEdo;
		}
		
		public OrderInfoForEdo OrderInfoForEdo { get; }

		public static InfoForCreatingEdoUpd Create(OrderInfoForEdo orderInfoForEdo) =>
			new InfoForCreatingEdoUpd(orderInfoForEdo);
	}
}
