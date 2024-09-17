using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.OrdersWithoutShipment;

namespace EdoDocumentsPreparer.Factories
{
	public interface IInfoForCreatingBillWithoutShipmentEdoFactory
	{
		InfoForCreatingBillWithoutShipmentForDebtEdo CreateInfoForCreatingBillWithoutShipmentEdo(
			OrderWithoutShipmentInfo orderWithoutShipmentInfo, FileData fileData);
	}
}
