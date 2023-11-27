namespace Fias.Client
{
	public interface IFiasApiClientFactory
	{
		IFiasApiClient CreateClient();
	}
}