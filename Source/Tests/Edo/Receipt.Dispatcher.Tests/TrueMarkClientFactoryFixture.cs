using TrueMarkApi.Client;

namespace Receipt.Dispatcher.Tests
{
	public class TrueMarkClientFactoryFixture : ITrueMarkApiClientFactory
	{
		public ITrueMarkApiClient GetClient()
		{
			return new TrueMarkApiClientFixture();
		}
	}
}
