using System;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Common
{
	public class EdoTaskItemTrueMarkStatusProviderFactory
	{
		private readonly ITrueMarkApiClient _trueMarkApiClient;

		public EdoTaskItemTrueMarkStatusProviderFactory(ITrueMarkApiClient trueMarkApiClient)
		{
			_trueMarkApiClient = trueMarkApiClient
				?? throw new ArgumentNullException(nameof(trueMarkApiClient));
		}

		public EdoTaskItemTrueMarkStatusProvider Create(EdoTask edoTask)
		{
			return new EdoTaskItemTrueMarkStatusProvider(edoTask, _trueMarkApiClient);
		}
	}
}
