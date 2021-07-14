namespace Vodovoz.Services
{
	public interface IDeliveryRulesParametersProvider
	{
		bool IsStoppedOnlineDeliveriesToday { get; }
	}
}