using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.OrdersWithoutShipment;

namespace EdoDocumentsPreparer.Factories
{
	public class InfoForCreatingBillWithoutShipmentEdoFactory : IInfoForCreatingBillWithoutShipmentEdoFactory
	{
		public InfoForCreatingBillWithoutShipmentEdo CreateInfoForCreatingBillWithoutShipmentEdo(
			OrderWithoutShipmentInfo orderWithoutShipmentInfo, FileData fileData)
		{
			InfoForCreatingBillWithoutShipmentEdo data = null;

			switch(orderWithoutShipmentInfo)
			{
				case OrderWithoutShipmentForDebtInfo:
					data = new InfoForCreatingBillWithoutShipmentForDebtEdo();
					break;
				case OrderWithoutShipmentForPaymentInfo:
					data = new InfoForCreatingBillWithoutShipmentForPaymentEdo();
					break;
				case OrderWithoutShipmentForAdvancePaymentInfo:
					data = new InfoForCreatingBillWithoutShipmentForAdvancePaymentEdo();
					break;
			}

			data.Initialize(orderWithoutShipmentInfo, fileData);

			return data;
		}
	}
}
