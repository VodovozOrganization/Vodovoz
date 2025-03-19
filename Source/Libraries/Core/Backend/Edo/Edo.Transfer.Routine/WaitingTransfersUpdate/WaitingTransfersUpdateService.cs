using Edo.Transfer.Dispatcher;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Transfer.Routine.WaitingTransfersUpdate
{
	public class WaitingTransfersUpdateService
	{
		private readonly ILogger<WaitingTransfersUpdateService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly TransferEdoHandler _transferEdoHandler;

		public WaitingTransfersUpdateService(
			ILogger<WaitingTransfersUpdateService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			TransferEdoHandler transferEdoHandler)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new System.ArgumentNullException(nameof(unitOfWorkFactory));
			_transferEdoHandler = transferEdoHandler ?? throw new System.ArgumentNullException(nameof(transferEdoHandler));
		}

		public async Task Update(CancellationToken cancellationToken)
		{
			var documentIds = await GetWaitingTransfersDocumentsIds();

			foreach(var documentId in documentIds)
			{
				try
				{
					await _transferEdoHandler.HandleTransferDocumentAcceptance(documentId, cancellationToken);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка при обработке документа трансфера с Id {DocumentId}", documentId);
				}
			}
		}

		private async Task<IEnumerable<int>> GetWaitingTransfersDocumentsIds()
		{
			using(var uow = _unitOfWorkFactory.CreateWithoutRoot())
			{
				var documentIds =
					await (from task in uow.Session.Query<TransferEdoTask>()
						   join document in uow.Session.Query<TransferEdoDocument>() on task.Id equals document.TransferTaskId
						   where
						   task.Status == EdoTaskStatus.Waiting
						   && document.Status == EdoDocumentStatus.Succeed
						   select document.Id)
					.ToListAsync();

				return documentIds;
			}
		}
	}
