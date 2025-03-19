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
		private readonly IUnitOfWork _uow;
		private readonly TransferEdoHandler _transferEdoHandler;

		public WaitingTransfersUpdateService(
			ILogger<WaitingTransfersUpdateService> logger,
			IUnitOfWork uow,
			TransferEdoHandler transferEdoHandler)
		{
			_logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_transferEdoHandler = transferEdoHandler ?? throw new System.ArgumentNullException(nameof(transferEdoHandler));
		}

		public async Task Update(CancellationToken cancellationToken)
		{
			var documentIds = await GetWaitingTransfersDocumentsIds();

			_logger.LogInformation("Получено {DocumentIdsCount} документов для обработки", documentIds.Count());

			var errorsCount = 0;

			foreach(var documentId in documentIds)
			{
				try
				{
					_logger.LogInformation("Обработка документа трансфера с Id {DocumentId}", documentId);

					await _transferEdoHandler.HandleTransferDocumentAcceptance(documentId, cancellationToken);

					_logger.LogInformation("Документ трансфера с Id {DocumentId} успешно обработан", documentId);
				}
				catch(Exception ex)
				{
					_logger.LogError(ex, "Ошибка при обработке документа трансфера с Id {DocumentId}", documentId);

					errorsCount++;
				}
			}

			_logger.LogInformation(
				"Обработка завершена. Обработано {DocumentIdsCount} документов. Ошибок: {ErrorsCount}",
				documentIds.Count(),
				errorsCount);
		}

		private async Task<IEnumerable<int>> GetWaitingTransfersDocumentsIds()
		{
			var documentIds =
					await (from task in _uow.Session.Query<TransferEdoTask>()
						   join document in _uow.Session.Query<TransferEdoDocument>() on task.Id equals document.TransferTaskId
						   where
						   task.Status == EdoTaskStatus.Waiting
						   && document.Status == EdoDocumentStatus.Succeed
						   select document.Id)
					.ToListAsync();

			return documentIds;
		}
	}
}
