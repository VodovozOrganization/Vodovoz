namespace TrueMarkApi.Client
{
	public interface ITrueMarkApiClientFactory
	{
		TrueMarkApiClient GetClient();
	}
}