using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Warehouses;

namespace Vodovoz.EntityRepositories.Equipments
{
	public class EquipmentLocation
	{
		public LocationType Type { get; set; }
		public OperationBase Operation { get; set; }
		public Warehouse Warehouse { get; set; }
		public CounterpartyEntity Counterparty { get; set; }
		public DeliveryPointEntity DeliveryPoint { get; set; }

		public string Title
		{
			get
			{
				switch(Type)
				{
					case LocationType.NoMovements:
						return "Нет движений в БД";
					case LocationType.Warehouse:
						return $"На складе: {Warehouse.Name}";
					case LocationType.Couterparty:
						return $"У {Counterparty.Name}{(DeliveryPoint != null ? " на адресе " + DeliveryPoint.Title : string.Empty)}";
					case LocationType.Superposition:
						return "В состоянии суперпозиции (как кот Шрёдингера)";
				}
				return null;
			}
		}
	}
}
