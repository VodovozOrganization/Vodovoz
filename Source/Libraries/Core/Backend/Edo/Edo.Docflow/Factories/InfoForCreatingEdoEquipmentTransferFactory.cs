using System;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Orders;

namespace Edo.Docflow.Factories
{
	public class InfoForCreatingEdoEquipmentTransferFactory : IInfoForCreatingEdoInformalOrderDocumentFactory
	{
		public InfoForCreatingEdoInformalOrderDocument CreateInfoForCreatingEdoInformalOrderDocument(OrderInfoForEdo orderInfoForEdo, FileData fileData)
		{
			var data = new InfoForCreatingEdoInformalOrderDocument
			{
				OrderInfoForEdo = orderInfoForEdo,
				FileData = fileData,
				MainDocumentId = Guid.NewGuid()
			};
			
			return data;
		}
	}
}

