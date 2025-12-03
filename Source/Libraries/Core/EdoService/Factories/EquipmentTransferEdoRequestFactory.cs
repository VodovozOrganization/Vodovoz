using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Orders;
using Order = Vodovoz.Domain.Orders.Order;

namespace EdoService.Library.Factories
{
	/// <summary>
	/// Фабрика для создания неформализованной заявки ЭДО для акта приёма-передачи оборудования
	/// </summary>
	public class EquipmentTransferEdoRequestFactory : IInformalEdoRequestFactory
	{
		public bool CanCreateFor(OrderDocumentType type) => type == OrderDocumentType.EquipmentTransfer;

		public InformalEdoRequest Create(Order order) => new EquipmentTransferEdoRequest { Order = order };
	}
}
