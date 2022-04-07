namespace Vodovoz.Parameters
{
	public interface IFastPaymentParametersProvider
	{
		int GetQRLifetime { get; }
		int GetPayUrlLifetime { get; }
		string GetFastPaymentBackUrl { get; }
	}
}
