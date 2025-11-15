using System;
using TaxcomEdo.Contracts.Documents;

namespace Edo.Docflow.Factories
{
	/// <summary>
	/// Фабрика для создания данных файла акта приёма-передачи оборудования
	/// </summary>
	public interface IEquipmentTransferFileDataFactory
	{
		EquipmentTransferFileData CreateEquipmentTransferFileData(string orderNumber, DateTime documentDate, byte[] data);
	}
}

