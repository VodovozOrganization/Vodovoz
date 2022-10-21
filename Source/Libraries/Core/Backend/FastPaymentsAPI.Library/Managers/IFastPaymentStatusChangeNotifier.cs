namespace FastPaymentsAPI.Library.Managers
{
	public interface IFastPaymentStatusChangeNotifier
	{
		void NotifyVodovozSite(int? onlineOrderId, int paymentFrom, decimal amount, bool paymentSucceeded);
	}
}
