using QS.DomainModel.UoW;
using System;
using Vodovoz.Core.Data.Repositories;

namespace Pacs.MangoCalls.Services
{
	internal class CallEventHandlerFactory
	{
		private readonly IPacsRepository _pacsRepository;

		public CallEventHandlerFactory(IPacsRepository pacsRepository)
		{
			_pacsRepository = pacsRepository ?? throw new ArgumentNullException(nameof(pacsRepository));
		}

		public CallEventHandler CreateCallEventHandler(string entryId, IUnitOfWork uow)
		{
			return new CallEventHandler(entryId, uow, _pacsRepository);
		}
	}
}
