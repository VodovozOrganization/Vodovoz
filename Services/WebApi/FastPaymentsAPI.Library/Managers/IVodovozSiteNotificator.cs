namespace FastPaymentsAPI.Library.Managers
{
	public interface IVodovozSiteNotificator
	{
		void NotifyVodovozSite(int? onlineOrderId, decimal amount, bool paymentSucceeded);
	}
}
