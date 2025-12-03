using TaxcomEdo.Contracts.Documents;
using Vodovoz.Core.Domain.Orders;

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
		/// <param name="order"></param>
		/// <param name="fileData"></param>
		/// <returns></returns>
		InfoForCreatingEdoInformalOrderDocument CreateInfoForCreatingEdoInformalOrderDocument(OrderEntity order, OrderDocumentFileData fileData);
	}
}

