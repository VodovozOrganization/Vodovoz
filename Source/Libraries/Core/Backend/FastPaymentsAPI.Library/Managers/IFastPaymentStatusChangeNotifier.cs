namespace FastPaymentsAPI.Library.Managers
{
	public interface IFastPaymentStatusChangeNotifier
	{
		void NotifyVodovozSite(int? onlineOrderId, int paymentFrom, decimal amount, bool paymentSucceeded);
		void NotifyMobileApp(int? onlineOrderId, int paymentFrom, decimal amount, bool paymentSucceeded, string callbackUrl);
	}
}
