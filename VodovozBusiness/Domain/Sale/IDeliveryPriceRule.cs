namespace Vodovoz.Domain.Sale
{
	public interface IDeliveryPriceRule
	{
		int Water19LCount { get; set; }
		int EqualsCount6LFor19L { get; }
		int EqualsCount600mlFor19L { get; }
	}
}