using System;
using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.OrdersWithoutShipment;

namespace EdoDocumentsPreparer.Factories
{
	public class InfoForCreatingBillWithoutShipmentEdoFactory : IInfoForCreatingBillWithoutShipmentEdoFactory
	{
		public InfoForCreatingBillWithoutShipmentForDebtEdo CreateInfoForCreatingBillWithoutShipmentEdo(
			OrderWithoutShipmentInfo orderWithoutShipmentInfo, FileData fileData)
		{
			var data = new InfoForCreatingBillWithoutShipmentForDebtEdo
			{
				OrderWithoutShipmentInfo = orderWithoutShipmentInfo,
				FileData = fileData,
				MainDocumentId = Guid.NewGuid()
			};
			
			return data;
		}
		
		/*public InfoForCreatingBillWithoutShipmentEdo CreateInfoForCreatingBillWithoutShipmentEdo(
			OrderWithoutShipmentInfo orderWithoutShipmentInfo, FileData fileData)
		{
			var data = new InfoForCreatingBillWithoutShipmentEdo
			{
				OrderWithoutShipmentInfo = orderWithoutShipmentInfo,
				FileData = fileData,
				MainDocumentId = Guid.NewGuid()
			};
			
			return data;
		}*/
	}
}
