namespace Vodovoz.Settings.Counterparty
{
	public interface ICounterpartySettings
	{
		int CounterpartyFromTenderId { get; }
		int GetMobileAppCounterpartyCameFromId { get; }
		int GetWebSiteCounterpartyCameFromId { get; }
		/// <summary>
		/// Id откуда клиент ИИ Бот
		/// </summary>
		int GetAiBotCounterpartyCameFromId { get; }
		string RevenueServiceClientAccessToken { get; }
		int ReferFriendPromotionCameFromId { get; }
	}
}
