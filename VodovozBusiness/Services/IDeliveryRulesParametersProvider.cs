namespace Vodovoz.Services
{
	public interface IDeliveryRulesParametersProvider
	{
		bool IsStoppedOnlineDeliveriesToday { get; }
		void UpdateOnlineDeliveriesTodayParameter(string value);
	}
}