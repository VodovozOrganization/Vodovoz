namespace Vodovoz.Settings.Delivery
{
	public interface IDeliveryScheduleSettings
	{
		int ClosingDocumentDeliveryScheduleId { get; }
		int DefaultDeliveryDayScheduleId { get; }
	}
}
