namespace DatabaseServiceWorker
{
	internal partial class PowerBiExportWorker
	{
		internal sealed class DeliveredDto
		{
			internal long ShipmentDayPlan { get; set; }
			internal long ShipmentDayFact { get; set; }
			internal long DeliveryPlan { get; set; }
			internal long DeliveryFact { get; set; }
		}
	}
}
