using System;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Orders;

namespace EdoDocumentsPreparer.Factories
{
	public class InfoForCreatingEdoBillFactory : IInfoForCreatingEdoBillFactory
	{
		public InfoForCreatingEdoBill CreateInfoForCreatingEdoBill(OrderInfoForEdo orderInfoForEdo, FileData fileData)
		{
			var data = new InfoForCreatingEdoBill
			{
				OrderInfoForEdo = orderInfoForEdo,
				FileData = fileData,
				MainDocumentId = Guid.NewGuid()
			};
			
			return data;
		}
	}
}
