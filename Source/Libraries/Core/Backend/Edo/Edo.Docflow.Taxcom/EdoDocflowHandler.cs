using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Docflow.Dto;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using TaxcomEdo.Client;
using TaxcomEdo.Contracts.Documents;
using Vodovoz.Core.Domain.Documents;

namespace Edo.Docflow.Taxcom
{
	public class EdoDocflowHandler : IEdoDocflowHandler
	{
		private readonly ILogger<EdoDocflowHandler> _logger;
		private readonly ITaxcomApiClient _taxcomApiClient;

		public EdoDocflowHandler(
			ILogger<EdoDocflowHandler> logger,
			ITaxcomApiClient taxcomApiClient)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_taxcomApiClient = taxcomApiClient ?? throw new ArgumentNullException(nameof(taxcomApiClient));
		}

		public async Task CreateTaxcomDocFlowAndSendDocument(
			IUnitOfWork uow,
			int edoOutgoingDocumentId,
			UniversalTransferDocumentInfo updInfo)
		{
			var now = DateTime.Now;
			
			var taxcomDocflow = new TaxcomDocflow
			{
				CreationTime = now,
				MainDocumentId = updInfo.DocumentId.ToString(),
				DocflowId = null,
				EdoDocumentId = edoOutgoingDocumentId
			};
			
			taxcomDocflow.Actions.Add(new TaxcomDocflowAction
			{
				State = EdoDocFlowStatus.NotStarted,
				Time = now
			});
			
			await uow.SaveAsync(taxcomDocflow);
			await uow.CommitAsync();

			var result = await _taxcomApiClient.SendDataForCreateUpdByEdo(updInfo);

			if(!result)
			{
				var newAction = new TaxcomDocflowAction
				{
					State = EdoDocFlowStatus.Error,
					Time = DateTime.Now,
					TaxcomDocflowId = taxcomDocflow.Id,
					ErrorMessage = "Не удалось отправить УПД на сервер Такском"
				};
				
				await uow.SaveAsync(newAction);
				await uow.CommitAsync();
			}
		}

		public async Task UpdateOutgoingTaxcomDocFlow(IUnitOfWork uow, EdoDocFlow edoDocFlow, CancellationToken cancellationToken)
		{
			if(!edoDocFlow.Documents.Any())
			{
				return;
			}
			
			var mainDocument = edoDocFlow.Documents.First();
			
			var taxcomDocflow = uow.Session.Query<TaxcomDocflow>()
				.SingleOrDefault(x => x.MainDocumentId == mainDocument.ExternalIdentifier);

			if(taxcomDocflow is null)
			{
				return;
			}

			taxcomDocflow.DocflowId = edoDocFlow.Id;
			
			var newAction = new TaxcomDocflowAction
			{
				State = (EdoDocFlowStatus)Enum.Parse(typeof(EdoDocFlowStatus), edoDocFlow.Status.ToString()),
				Time = edoDocFlow.StatusChangeDateTime,
				TaxcomDocflowId = taxcomDocflow.Id,
				ErrorMessage = edoDocFlow.ErrorDescription
			};
			
			taxcomDocflow.Actions.Add(newAction);

			if(newAction.State == EdoDocFlowStatus.Succeed)
			{
				//TODO если нужно сохранять файлы по завершению документооборота,
				//нужно модифицировать сервис под новую сущность
				
                /*var containerRawData =
					await _taxcomApiClient.GetDocFlowRawData(edoDocFlow.Id.Value.ToString(), cancellationToken);

				using(var ms = new MemoryStream(containerRawData.ToArray()))
				{
					var result =
						await _edoContainerFileStorageService.UpdateContainerAsync(container, ms, cancellationToken);

					if(result.IsFailure)
					{
						var errors = string.Join(", ", result.Errors.Select(e => e.Message));
						_logger.LogError("Не удалось обновить контейнер, ошибка: {Errors}", errors);
					}
				}*/
			}

			_logger.LogInformation(
				"Сохраняем изменения документооборота {DocflowId} по документу {DocumentId}",
				taxcomDocflow.DocflowId,
				taxcomDocflow.MainDocumentId);
			
			await uow.SaveAsync(taxcomDocflow);
			await uow.CommitAsync();
		}

		public async Task ProcessIngoingTaxcomEdoDocFlow(IUnitOfWork uow, EdoDocFlow edoDocFlow, CancellationToken cancellationToken)
		{
			if(!edoDocFlow.Documents.Any())
			{
				return;
			}
			
			var mainDocument = edoDocFlow.Documents.First();
			
			var taxcomDocflow = uow.Session.Query<TaxcomDocflow>()
				.SingleOrDefault(x => x.MainDocumentId == mainDocument.ExternalIdentifier);

			if(taxcomDocflow is null)
			{
				return;
			}

			var result = await _taxcomApiClient.AcceptIngoingDocflow(edoDocFlow.Id, cancellationToken);

			if(!result)
			{
				_logger.LogError(
					"Не удалось подписать входящий документ {ExternalIdentifier} документооборота {Docflow}",
					taxcomDocflow.MainDocumentId,
					edoDocFlow.Id);
			}
		}
	}
}
