using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Receipt.Dispatcher
{
	public class ReceiptEdoTaskHandler : IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly ForOwnNeedsReceiptEdoTaskHandler _forOwnNeedsReceiptEdoTaskHandler;
		private readonly ResaleReceiptEdoTaskHandler _resaleReceiptEdoTaskHandler;

		public ReceiptEdoTaskHandler(
			IUnitOfWork uow,
			ForOwnNeedsReceiptEdoTaskHandler forOwnNeedsReceiptEdoTaskHandler,
			ResaleReceiptEdoTaskHandler resaleReceiptEdoTaskHandler
			)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_forOwnNeedsReceiptEdoTaskHandler = forOwnNeedsReceiptEdoTaskHandler ?? throw new ArgumentNullException(nameof(forOwnNeedsReceiptEdoTaskHandler));
			_resaleReceiptEdoTaskHandler = resaleReceiptEdoTaskHandler ?? throw new ArgumentNullException(nameof(resaleReceiptEdoTaskHandler));
		}

		public async Task HandleNew(int receiptEdoTaskId, CancellationToken cancellationToken)
		{
			var edoTask = await _uow.Session.GetAsync<ReceiptEdoTask>(receiptEdoTaskId, cancellationToken);

			if(edoTask.OrderEdoRequest.Order.Client.ReasonForLeaving == ReasonForLeaving.Resale)
			{
				await _resaleReceiptEdoTaskHandler.HandleResaleReceipt(edoTask, cancellationToken);
			}
			else
			{
				await _forOwnNeedsReceiptEdoTaskHandler.HandleForOwnNeedsReceipt(edoTask, cancellationToken);
			}

			// надо ловить исключения о проблемах и сохранять их вне основного UoW
		}

		public async Task HandleTransfered(int receiptEdoTaskId, CancellationToken cancellationToken)
		{
			
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
