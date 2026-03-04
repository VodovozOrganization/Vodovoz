using Autofac;
using Edo.Transfer.Dispatcher;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Transfer.Routine.Services
{
	public class WaitingTransfersUpdateService
	{
		private readonly ILogger<WaitingTransfersUpdateService> _logger;
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public WaitingTransfersUpdateService(
			ILogger<WaitingTransfersUpdateService> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IServiceScopeFactory serviceScopeFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_unitOfWorkFactory = unitOfWorkFactory ?? throw new ArgumentNullException(nameof(unitOfWorkFactory));
			_serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
		}

		public async Task Update(int tasksCountToProcess, CancellationToken cancellationToken)
		{
			var documentIds = await GetWaitingTransfersDocumentsIds(tasksCountToProcess, cancellationToken);

			_logger.LogInformation("Получено {DocumentIdsCount} документов для обработки", documentIds.Count());

			var errorsCount = 0;

			foreach(var documentId in documentIds)
			{
				try
				{
					_logger.LogInformation("Обработка документа трансфера с Id {DocumentId}", documentId);

					using(var documentScope = _serviceScopeFactory.CreateScope())
					{
						var transferEdoHandler = documentScope.ServiceProvider.GetRequiredService<TransferEdoHandler>();
						await transferEdoHandler.HandleTransferDocumentAcceptance(documentId, cancellationToken);
					}

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

		private async Task<IEnumerable<int>> GetWaitingTransfersDocumentsIds(int count, CancellationToken cancellationToken)
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
						.OrderByDescending(id => id)
						.Take(count)
						.ToListAsync(cancellationToken);

				return documentIds;
			}
		}
	}
}
