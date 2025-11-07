using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.Infrastructure;
using Edo.Contracts.Messages.Events;
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
				EdoDocumentId = @event.EdoOutgoingDocumentId,
			};
			
			taxcomDocflow.Actions.Add(new TaxcomDocflowAction
			{
				DocFlowState = EdoDocFlowStatus.NotStarted,
				Time = now
			});
			
			await _uow.SaveAsync(taxcomDocflow);
			await _uow.CommitAsync();

			var result = await _taxcomApiClient.SendDataForCreateUpdByEdo(@event.UpdInfo);

			if(!result)
			{
				var newAction = new TaxcomDocflowAction
				{
					DocFlowState = EdoDocFlowStatus.Error,
					Time = DateTime.Now,
					TaxcomDocflowId = taxcomDocflow.Id,
					ErrorMessage = "Не удалось отправить УПД на сервер Такском"
				};
				
				await _uow.SaveAsync(newAction);
				await _uow.CommitAsync();
			}
		}

		public async Task SendOfferCancellation(
			TaxcomDocflowRequestCancellationEvent @event, 
			CancellationToken cancellationToken
			)
		{
			var docflow = await _uow.Session.QueryOver<TaxcomDocflow>()
				.Where(x => x.EdoDocumentId == @event.DocumentId)
				.Where(x => x.DocflowId != null)
				.OrderBy(x => x.CreationTime).Desc
				.Take(1)
				.SingleOrDefaultAsync(cancellationToken);

			if(docflow == null)
			{
				_logger.LogWarning("Не найден документооборот для ЭДО документа №{DocumentId}", @event.DocumentId);
				return;
			}

			await _taxcomApiClient.SendOfferCancellationRaw(
				docflow.DocflowId.Value.ToString(),
				@event.CancellationReason,
				cancellationToken
			);
		}

		public async Task AcceptOfferCancellation(
			AcceptingWaitingForCancellationDocflowEvent @event,
			CancellationToken cancellationToken
			)
		{
			await _taxcomApiClient.AcceptOfferCancellation(
				@event.DocFlowId,
				cancellationToken
			);
		}

		public async Task<EdoDocflowUpdatedEvent> UpdateOutgoingTaxcomDocFlow(
			OutgoingTaxcomDocflowUpdatedEvent @event, CancellationToken cancellationToken = default)
		{
			var taxcomDocflow = _uow.Session.Query<TaxcomDocflow>()
				.SingleOrDefault(x => x.MainDocumentId == @event.MainDocumentId);

			if(taxcomDocflow is null)
			{
				_logger.LogWarning("Пришел запрос обновления документооборота {DocflowId} по неизвестному документу {EdoDocument}",
					@event.DocFlowId,
					@event.MainDocumentId);
				return null;
			}

			taxcomDocflow.DocflowId = @event.DocFlowId;
			var lastAction = taxcomDocflow.Actions.LastOrDefault();
			var newStatus = @event.Status.TryParseAsEnum<EdoDocFlowStatus>();
			var newTraceabilityStatus = @event.TrueMarkTraceabilityStatus.TryParseAsEnum<TrueMarkTraceabilityStatus>();

			if(newStatus is null)
			{
				throw new InvalidOperationException($"Неизвестный статус документооборота {@event.Status}");
			}

			EdoDocflowUpdatedEvent edoDocflowUpdatedEvent = null;

			if(lastAction is null
				|| lastAction.DocFlowState != newStatus.Value
				|| lastAction.TrueMarkTraceabilityStatus != newTraceabilityStatus)
			{
				var newAction = new TaxcomDocflowAction
				{
					DocFlowState = newStatus.Value,
					Time = @event.StatusChangeDateTime,
					TaxcomDocflowId = taxcomDocflow.Id,
					ErrorMessage = @event.ErrorDescription,
					TrueMarkTraceabilityStatus = newTraceabilityStatus
				};

				taxcomDocflow.Actions.Add(newAction);
				taxcomDocflow.IsReceived = @event.IsReceived;

				edoDocflowUpdatedEvent = new EdoDocflowUpdatedEvent
				{
					EdoDocumentId = taxcomDocflow.EdoDocumentId,
					DocFlowId = @event.DocFlowId,
					DocFlowStatus = newAction.DocFlowState.ToString(),
					TrueMarkTraceabilityStatus = newAction.TrueMarkTraceabilityStatus?.ToString()
				};

				if(newAction.DocFlowState == EdoDocFlowStatus.Succeed)
				{
					edoDocflowUpdatedEvent.StatusChangeTime = @event.StatusChangeDateTime;
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
			}

			await SaveTaxcomDocflow(taxcomDocflow);
			return edoDocflowUpdatedEvent;
		}

		public async Task AcceptIngoingTaxcomEdoDocFlowWaitingForSignature(
			AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent @event, CancellationToken cancellationToken = default)
		{
			var taxcomDocflow = _uow.Session.Query<TaxcomDocflow>()
				.SingleOrDefault(x => x.MainDocumentId == @event.MainDocumentId);

			if(taxcomDocflow is null)
			{
				_logger.LogWarning("Не нашли отправку с таким документом {ExternalIdentifier}", @event.MainDocumentId);
				return;
			}
			
			if(taxcomDocflow.AcceptingIngoingDocflowTime.HasValue
				&& (DateTime.Now - taxcomDocflow.AcceptingIngoingDocflowTime.Value).TotalHours < 1)
			{
				_logger.LogWarning("По ДО {DocflowId} была уже отправка титула покупателя {SendTime}, ждем час",
					taxcomDocflow.DocflowId,
					taxcomDocflow.AcceptingIngoingDocflowTime.Value.ToShortDateString());
				return;
			}

			_logger.LogInformation(
				"Принимаем документооборот {DocflowId} по документу {DocumentId}",
				taxcomDocflow.DocflowId,
				taxcomDocflow.MainDocumentId);
			var result = await _taxcomApiClient.AcceptIngoingDocflow(@event.DocFlowId, @event.Organization, cancellationToken);

			if(!result)
			{
				_logger.LogError(
					"Не удалось подписать входящий документ {ExternalIdentifier} документооборота {DocflowId}",
					taxcomDocflow.MainDocumentId,
					@event.DocFlowId);
			}
			else
			{
				taxcomDocflow.AcceptingIngoingDocflowTime = DateTime.Now;
				await SaveTaxcomDocflow(taxcomDocflow);
			}
		}
		
		private async Task SaveTaxcomDocflow(TaxcomDocflow taxcomDocflow)
		{
			_logger.LogInformation(
				"Сохраняем изменения документооборота {DocflowId} по документу {DocumentId}",
				taxcomDocflow.DocflowId,
				taxcomDocflow.MainDocumentId);
			
			await _uow.SaveAsync(taxcomDocflow);
			await _uow.CommitAsync();
		}
	}
}
