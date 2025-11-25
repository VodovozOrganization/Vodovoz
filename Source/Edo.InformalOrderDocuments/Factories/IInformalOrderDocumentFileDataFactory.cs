using System;
using TaxcomEdo.Contracts.Documents;

namespace Edo.InformalOrderDocuments.Factories
{
	/// <summary>
	/// Фабрика для создания файла неформализованного документа заказа
	/// </summary>
	public interface IInformalOrderDocumentFileDataFactory
	{
		OrderDocumentFileData CreateInformalOrderDocumentFileData(int orderNumber, DateTime documentDate, byte[] data);
	}
}

