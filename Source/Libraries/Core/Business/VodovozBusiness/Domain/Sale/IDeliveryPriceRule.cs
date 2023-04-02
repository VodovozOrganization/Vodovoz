namespace Vodovoz.Domain.Sale
{
	public interface IDeliveryPriceRule
	{
		int Water19LCount { get; set; }
		int Water6LCount { get; set; }
		int Water1500mlCount { get; set; }
		int Water600mlCount { get; set; }
		int Water500mlCount { get; set; }
	}
}
