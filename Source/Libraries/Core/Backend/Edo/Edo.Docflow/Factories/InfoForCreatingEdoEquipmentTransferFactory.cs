using System;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Orders;

namespace Edo.Docflow.Factories
{
	public class InfoForCreatingEdoEquipmentTransferFactory : IInfoForCreatingEdoEquipmentTransferFactory
	{
		public InfoForCreatingEdoEquipmentTransfer CreateInfoForCreatingEdoEquipmentTransfer(OrderInfoForEdo orderInfoForEdo, FileData fileData)
		{
			var data = new InfoForCreatingEdoEquipmentTransfer
			{
				OrderInfoForEdo = orderInfoForEdo,
				FileData = fileData,
				MainDocumentId = Guid.NewGuid()
			};
			
			return data;
		}
	}
}

