namespace Vodovoz.Settings.Counterparty
{
	public interface ICounterpartySettings
	{
		int CounterpartyFromTenderId { get; }
		int GetMobileAppCounterpartyCameFromId { get; }
		int GetWebSiteCounterpartyCameFromId { get; }
		string RevenueServiceClientAccessToken { get; }
		int ReferFriendPromotionCameFromId { get; }
	}
}
