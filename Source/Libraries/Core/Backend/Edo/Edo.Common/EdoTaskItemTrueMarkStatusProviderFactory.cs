using System;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Common
{
	public class EdoTaskItemTrueMarkStatusProviderFactory
	{
		private readonly ITrueMarkApiClient _trueMarkApiClient;

		public EdoTaskItemTrueMarkStatusProviderFactory(ITrueMarkApiClientFactory trueMarkApiClientFactory)
		{
			if(trueMarkApiClientFactory is null)
			{
				throw new ArgumentNullException(nameof(trueMarkApiClientFactory));
			}

			_trueMarkApiClient = trueMarkApiClientFactory.GetClient();
		}

		public EdoTaskItemTrueMarkStatusProvider Create(EdoTask edoTask)
		{
			return new EdoTaskItemTrueMarkStatusProvider(edoTask, _trueMarkApiClient);
		}
	}
}
