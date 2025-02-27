using System;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Documents
{
	public class EdoTaskItemTrueMarkStatusProviderFactory
	{

		public EdoTaskItemTrueMarkStatusProviderFactory()
		{
		}

		public EdoTaskItemTrueMarkStatusProvider Create(DocumentEdoTask edoTask, TrueMarkApiClient trueMarkApiClient)
		{
			return new EdoTaskItemTrueMarkStatusProvider(edoTask, trueMarkApiClient);
		}
	}
}
