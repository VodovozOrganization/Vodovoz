namespace Vodovoz.Settings.FastPayments
{
	public interface IFastPaymentSettings
	{
		int GetQRLifetime { get; }
		int GetPayUrlLifetime { get; }
		int GetOnlinePayByQRLifetime { get; }
		int GetDefaultShopId { get; }
		string GetFastPaymentBackUrl { get; }
		string GetFastPaymentApiBaseUrl { get; }
		string GetAvangardFastPayBaseUrl { get; }
		string GetVodovozFastPayBaseUrl { get; }
	}
}
