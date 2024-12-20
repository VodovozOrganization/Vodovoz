using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Edo.Transport2;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using TaxcomEdo.Client;
using Vodovoz.Core.Domain.Documents;

namespace Edo.Docflow.Taxcom
{
	public class EdoDocflowHandler : IEdoDocflowHandler
	{
		private readonly IUnitOfWork _uow;
		private readonly ILogger<EdoDocflowHandler> _logger;
		private readonly ITaxcomApiClient _taxcomApiClient;

		public EdoDocflowHandler(
			IUnitOfWork uow,
			ILogger<EdoDocflowHandler> logger,
			ITaxcomApiClient taxcomApiClient)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_taxcomApiClient = taxcomApiClient ?? throw new ArgumentNullException(nameof(taxcomApiClient));
		}

		public async Task CreateTaxcomDocFlowAndSendDocument(TaxcomDocflowSendEvent @event)
		{
			var now = DateTime.Now;
			
			var taxcomDocflow = new TaxcomDocflow
			{
				CreationTime = now,
				MainDocumentId = @event.UpdInfo.DocumentId.ToString(),
				DocflowId = null,
				EdoDocumentId = @event.EdoOutgoingDocumentId
			};
			
			taxcomDocflow.Actions.Add(new TaxcomDocflowAction
			{
				State = EdoDocFlowStatus.NotStarted,
				Time = now
			});
			
			await _uow.SaveAsync(taxcomDocflow);
			await _uow.CommitAsync();

			var result = await _taxcomApiClient.SendDataForCreateUpdByEdo(@event.UpdInfo);

			if(!result)
			{
				var newAction = new TaxcomDocflowAction
				{
					State = EdoDocFlowStatus.Error,
					Time = DateTime.Now,
					TaxcomDocflowId = taxcomDocflow.Id,
					ErrorMessage = "Не удалось отправить УПД на сервер Такском"
				};
				
				await _uow.SaveAsync(newAction);
				await _uow.CommitAsync();
			}
		}

		public async Task UpdateOutgoingTaxcomDocFlow(
			OutgoingTaxcomDocflowUpdatedEvent @event, CancellationToken cancellationToken = default)
		{
			var taxcomDocflow = _uow.Session.Query<TaxcomDocflow>()
				.SingleOrDefault(x => x.MainDocumentId == @event.MainDocumentId);

			if(taxcomDocflow is null)
			{
				return;
			}

			taxcomDocflow.DocflowId = @event.DocFlowId;
			
			var newAction = new TaxcomDocflowAction
			{
				State = (EdoDocFlowStatus)Enum.Parse(typeof(EdoDocFlowStatus), @event.Status.ToString()),
				Time = @event.StatusChangeDateTime,
				TaxcomDocflowId = taxcomDocflow.Id,
				ErrorMessage = @event.ErrorDescription
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
			
			await _uow.SaveAsync(taxcomDocflow);
			await _uow.CommitAsync();
		}

		public async Task AcceptIngoingTaxcomEdoDocFlowWaitingForSignature(
			AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent @event, CancellationToken cancellationToken = default)
		{
			var taxcomDocflow = _uow.Session.Query<TaxcomDocflow>()
				.SingleOrDefault(x => x.MainDocumentId == @event.MainDocumentId);

			if(taxcomDocflow is null)
			{
				return;
			}

			var result = await _taxcomApiClient.AcceptIngoingDocflow(@event.DocFlowId, cancellationToken);

			if(!result)
			{
				_logger.LogError(
					"Не удалось подписать входящий документ {ExternalIdentifier} документооборота {Docflow}",
					taxcomDocflow.MainDocumentId,
					@event.DocFlowId);
			}
		}
	}
}
