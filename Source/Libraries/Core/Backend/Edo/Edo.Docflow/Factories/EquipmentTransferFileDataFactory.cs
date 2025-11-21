using System;
using TaxcomEdo.Contracts.Documents;

namespace Edo.Docflow.Factories
{
	/// <summary>
	/// Фабрика для создания данных файла акта приёма-передачи оборудования
	/// </summary>
	public class EquipmentTransferFileDataFactory : IEquipmentTransferFileDataFactory
	{
		public EquipmentTransferFileData CreateEquipmentTransferFileData(string orderNumber, DateTime documentDate, byte[] data) =>
			new EquipmentTransferFileData
			{
				OrderNumber = orderNumber,
				DocumentDate = documentDate,
				Image = data
			};
	}
}

