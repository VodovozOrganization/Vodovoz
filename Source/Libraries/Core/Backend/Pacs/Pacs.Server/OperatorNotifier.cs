using MassTransit;
using System;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Pacs;

namespace Pacs.Server
{
	public class OperatorNotifier : IOperatorNotifier
	{
		private readonly IBus _messageBus;

		public OperatorNotifier(IBus messageBus)
		{
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task OperatorChanged(OperatorState operatorState)
		{

			await _messageBus.Publish<OperatorState>(operatorState);
		}
	}
}
