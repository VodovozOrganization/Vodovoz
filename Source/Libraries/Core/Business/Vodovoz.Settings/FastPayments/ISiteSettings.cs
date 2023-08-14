namespace Vodovoz.Settings.FastPayments
{
	public interface ISiteSettings
	{
		string BaseUrl { get; }
		string NotifyOfFastPaymentStatusChangedUri { get; }
	}
}
