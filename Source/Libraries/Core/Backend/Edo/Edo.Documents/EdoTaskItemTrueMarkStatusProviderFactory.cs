using System;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Documents
{
	public class EdoTaskItemTrueMarkStatusProviderFactory
	{
		private readonly TrueMarkApiClient _trueMarkApiClient;

		public EdoTaskItemTrueMarkStatusProviderFactory(TrueMarkApiClient trueMarkApiClient)
		{
			_trueMarkApiClient = trueMarkApiClient ?? throw new ArgumentNullException(nameof(trueMarkApiClient));
		}

		public EdoTaskItemTrueMarkStatusProvider Create(DocumentEdoTask edoTask)
		{
			return new EdoTaskItemTrueMarkStatusProvider(edoTask, _trueMarkApiClient);
		}
	}
}
