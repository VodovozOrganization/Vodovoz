using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TrueMark.Contracts;
using TrueMarkApi.Client;
using Vodovoz.Core.Domain.Orders;
using Vodovoz.Core.Domain.Repositories;

namespace Edo.Withdrawal.Routine.Services
{
	/// <summary>
	/// Сервис обновления статусов документов ЧЗ
	/// </summary>
	public class TrueMarkDocumentsStatusUpdateService
	{
		private readonly ILogger<TrueMarkDocumentsStatusUpdateService> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly IGenericRepository<TrueMarkDocument> _trueMarkDocumentRepository;
		private readonly ITrueMarkApiClient _trueMarkApiClient;

		public TrueMarkDocumentsStatusUpdateService(
			ILogger<TrueMarkDocumentsStatusUpdateService> logger,
			IUnitOfWorkFactory uowFactory,
			IGenericRepository<TrueMarkDocument> trueMarkDocumentRepository,
			ITrueMarkApiClient trueMarkApiClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_trueMarkDocumentRepository = trueMarkDocumentRepository ?? throw new ArgumentNullException(nameof(trueMarkDocumentRepository));
			_trueMarkApiClient = trueMarkApiClient ?? throw new ArgumentNullException(nameof(trueMarkApiClient));
		}

		/// <summary>
		/// Обновляет статусы документов ЧЗ, которые еще не были обновлены и имеют Guid.
		/// Статусы обновляются путем запроса к API ЧЗ
		/// </summary>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns></returns>
		public async Task UpdateTrueMarkDocuments(CancellationToken cancellationToken)
		{
			using(var uow = _uowFactory.CreateWithoutRoot(nameof(TrueMarkDocumentsStatusUpdateService)))
			{
				var documentsToUpdateStatus = await GetTrueMarkDocumentsToUpdate(uow, cancellationToken);

				foreach(var trueMarkDocument in documentsToUpdateStatus)
				{
					if(trueMarkDocument.Guid == null)
					{
						_logger.LogWarning("Документ ЧЗ с Id {Id} имеет пустой Guid, пропускаем", trueMarkDocument.Id);
						continue;
					}

					await UpdateTrueMarkDocumentStatus(uow, trueMarkDocument, trueMarkDocument.Organization.INN, cancellationToken);
				}

				await uow.CommitAsync(cancellationToken);
			}
		}

		private async Task UpdateTrueMarkDocumentStatus(
			IUnitOfWork uow,
			TrueMarkDocument trueMarkDocument,
			string organizationInn,
			CancellationToken cancellationToken)
		{
			var documentGuid = trueMarkDocument.Guid.Value;

			try
			{
				var documentInfo = await _trueMarkApiClient.GetDocumentInfo(documentGuid, organizationInn, cancellationToken);

				switch(documentInfo.Status)
				{
					case TrueMarkDocumentStatus.Ok:
						trueMarkDocument.IsSuccess = true;
						trueMarkDocument.ErrorMessage = null;
						break;
					case TrueMarkDocumentStatus.Error:
						trueMarkDocument.IsSuccess = false;
						trueMarkDocument.ErrorMessage = documentInfo.ErrorMessage.Substring(0, 255);
						break;
					case TrueMarkDocumentStatus.NotFound:
						_logger.LogWarning(
							"Документ ЧЗ с Guid {Guid} не найден в системе ЧЗ, пропускаем. Возможно, создание документа еще не завершено",
							documentGuid);
						break;
				}

				await uow.SaveAsync(trueMarkDocument, cancellationToken: cancellationToken);
			}
			catch(Exception ex)
			{
				_logger.LogError(ex, "Ошибка при обновлении статуса документа ЧЗ с Guid {Guid}", documentGuid);
			}
		}

		private async Task<IEnumerable<TrueMarkDocument>> GetTrueMarkDocumentsToUpdate(IUnitOfWork uow, CancellationToken cancellationToken)
		{
			_logger.LogInformation("Получение документов ЧЗ для обновления статуса");

			var trueMarkDocuments = (await _trueMarkDocumentRepository
				.GetAsync(
					uow,
					x => !x.IsSuccess && x.ErrorMessage == null && x.Guid != null,
					cancellationToken: cancellationToken))
				.Value ?? Enumerable.Empty<TrueMarkDocument>();

			_logger.LogInformation("Найдено {Count} документов для обновления статуса", trueMarkDocuments.Count());

			return trueMarkDocuments;
		}
	}
}
