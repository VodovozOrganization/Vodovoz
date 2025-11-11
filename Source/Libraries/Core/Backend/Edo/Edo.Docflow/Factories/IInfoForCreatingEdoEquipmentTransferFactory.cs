using TaxcomEdo.Contracts.Documents;
using TaxcomEdo.Contracts.Orders;

namespace Edo.Docflow.Factories
{
	/// <summary>
	/// Фабрика для создания информации об акте приёма-передачи оборудования для ЭДО
	/// </summary>
	public interface IInfoForCreatingEdoEquipmentTransferFactory
	{
		InfoForCreatingEdoEquipmentTransfer CreateInfoForCreatingEdoEquipmentTransfer(OrderInfoForEdo orderInfoForEdo, FileData fileData);
	}
}

