using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Orders;

namespace Edo.Docflow.Factories
{
	/// <summary>
	/// Фабрика для создания информации об документе заказа для ЭДО
	/// </summary>
	public interface IInfoForCreatingEdoInformalOrderDocumentFactory
	{
		/// <summary>
		/// Создать информацию об документе заказа для ЭДО
		/// </summary>
		/// <param name="orderInfoForEdo"></param>
		/// <param name="fileData"></param>
		/// <returns></returns>
		InfoForCreatingEdoInformalOrderDocument CreateInfoForCreatingEdoInformalOrderDocument(OrderInfoForEdo orderInfoForEdo, FileData fileData);
	}
}

