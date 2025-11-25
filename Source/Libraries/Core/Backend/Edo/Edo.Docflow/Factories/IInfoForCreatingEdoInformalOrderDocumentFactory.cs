using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Orders;

namespace Edo.Docflow.Factories
{
	/// <summary>
	/// Фабрика для создания информации об документе заказа для ЭДО
	/// </summary>
	public interface IInfoForCreatingEdoInformalOrderDocumentFactory
	{
		InfoForCreatingEdoInformalOrderDocument CreateInfoForCreatingEdoInformalOrderDocument(OrderInfoForEdo orderInfoForEdo, FileData fileData);
	}
}

