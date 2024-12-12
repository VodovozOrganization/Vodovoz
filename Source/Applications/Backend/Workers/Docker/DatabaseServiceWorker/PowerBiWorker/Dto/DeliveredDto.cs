using System;

namespace DatabaseServiceWorker.PowerBiWorker.Dto
{
	internal sealed class DeliveredDto
	{
		internal DateTime Date { get; set; }
		internal int ShipmentDayPlan { get; set; }
		internal int ShipmentDayFact { get; set; }
		internal int DeliveryPlan { get; set; }
		internal int DeliveryFact { get; set; }
	}
}
