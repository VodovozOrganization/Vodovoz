using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Orders;

namespace EdoDocumentsPreparer.Factories
{
	public interface IInfoForCreatingEdoBillFactory
	{
		InfoForCreatingEdoBill CreateInfoForCreatingEdoBill(OrderInfoForEdo orderInfoForEdo, FileData fileData);
	}
}
