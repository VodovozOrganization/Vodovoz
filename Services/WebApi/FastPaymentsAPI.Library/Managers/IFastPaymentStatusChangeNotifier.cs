namespace FastPaymentsAPI.Library.Managers
{
	public interface IFastPaymentStatusChangeNotifier
	{
		void NotifyVodovozSite(int? onlineOrderId, decimal amount, bool paymentSucceeded);
	}
}
