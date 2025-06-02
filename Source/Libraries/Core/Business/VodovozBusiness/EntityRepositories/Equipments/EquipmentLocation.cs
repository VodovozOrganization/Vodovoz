using System;
using Vodovoz.Core.Domain.Operations;
using Vodovoz.Core.Domain.Warehouses;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Operations;

namespace Vodovoz.EntityRepositories.Equipments
{
	public class EquipmentLocation
	{
		public LocationType Type { get; set; }
		public OperationBase Operation { get; set; }
		public Warehouse Warehouse { get; set; }
		public Counterparty Counterparty { get; set; }
		public DeliveryPoint DeliveryPoint { get; set; }

		public string Title {
			get {
				switch(Type) {
					case LocationType.NoMovements:
						return "Нет движений в БД";
					case LocationType.Warehouse:
						return $"На складе: { Warehouse.Name }";
					case LocationType.Couterparty:
						return $"У { Counterparty.Name }{ (DeliveryPoint != null ? " на адресе " + DeliveryPoint.Title : string.Empty) }";
					case LocationType.Superposition:
						return "В состоянии суперпозиции (как кот Шрёдингера)";
				}
				return null;
			}
		}
	}
}