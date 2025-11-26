using System;
using TaxcomEdo.Contracts.Documents;

namespace Edo.InformalOrderDocuments.Factories
{
	/// <summary>
	/// Фабрика для создания данных файла акта приёма-передачи оборудования
	/// </summary>
	public class InformalOrderDocumentFileDataFactory : IInformalOrderDocumentFileDataFactory
	{
		public OrderDocumentFileData CreateInformalOrderDocumentFileData(int orderNumber, DateTime documentDate, byte[] data) =>
			new OrderDocumentFileData
			{
				OrderId = orderNumber,
				DocumentDate = documentDate,
				Image = data
			};
	}
}

