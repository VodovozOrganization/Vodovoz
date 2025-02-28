namespace TrueMarkApi.Client
{
	public interface ITrueMarkApiClientFactory
	{
		ITrueMarkApiClient GetClient();
	}
}
