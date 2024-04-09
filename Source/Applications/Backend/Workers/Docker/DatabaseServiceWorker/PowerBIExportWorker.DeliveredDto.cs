namespace DatabaseServiceWorker
{
	internal partial class PowerBIExportWorker
	{
		private class DeliveredDto
		{
			internal long ShipmentDayPlan { get; set; }
			internal long ShipmentDayFact { get; set; }
			internal long DeliveryPlan { get; set; }
			internal long DeliveryFact { get; set; }
		}
	}
}
