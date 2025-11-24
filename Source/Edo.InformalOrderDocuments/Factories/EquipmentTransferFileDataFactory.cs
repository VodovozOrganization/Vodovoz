using System;
using TaxcomEdo.Contracts.Documents;

namespace Edo.InformalOrderDocuments.Factories
{
	/// <summary>
	/// Фабрика для создания данных файла акта приёма-передачи оборудования
	/// </summary>
	public class EquipmentTransferFileDataFactory : IEquipmentTransferFileDataFactory
	{
		public OrderDocumentFileData CreateEquipmentTransferFileData(int orderNumber, DateTime documentDate, byte[] data) =>
			new OrderDocumentFileData
			{
				OrderId = orderNumber,
				DocumentDate = documentDate,
				Image = data
			};
	}
}

