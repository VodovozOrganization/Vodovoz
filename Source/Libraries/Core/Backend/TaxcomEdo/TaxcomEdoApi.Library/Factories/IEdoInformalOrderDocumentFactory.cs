using Taxcom.Client.Api.Entity;
using TaxcomEdo.Contracts.Documents;

namespace TaxcomEdoApi.Library.Factories
{
	/// <summary>
	/// Интерфейс фабрики неформальных документов заказа для ЭДО
	/// </summary>
	public interface IEdoInformalOrderDocumentFactory
	{
		/// <summary>
		/// Создать неформальный документ заказа для ЭДО
		/// </summary>
		/// <param name="data"></param>
		/// <returns></returns>
		NonformalizedDocument CreateInformalOrderDocument(InfoForCreatingEdoInformalOrderDocument data);
	}
}
