using Edo.Contracts.Messages.Events;
using MassTransit;
using QS.DomainModel.UoW;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TaxcomEdo.Contracts.Documents;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Organizations;

namespace Taxcom.Docflow.Utility
{
	public class DocflowStatusesService
    {
		private readonly IUnitOfWork _uow;
		private readonly ILogger<DocflowStatusesService> _logger;
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly ITaxcomApiFactory _taxcomApiFactory;
		private readonly IBus _messageBus;

		public DocflowStatusesService(
			ILogger<DocflowStatusesService> logger,
			IUnitOfWorkFactory uowFactory,
			ITaxcomApiFactory taxcomApiFactory,
			IBus messageBus
			)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_uow = _uowFactory.CreateWithoutRoot();
			_taxcomApiFactory = taxcomApiFactory ?? throw new ArgumentNullException(nameof(taxcomApiFactory));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		public async Task RehandleDocflowStatuses(
			TaxcomSettings taxcomSettings,
			DateTime timeFrom, 
			DateTime timeTo, 
			CancellationToken cancellationToken
			)
		{
			var organization = await _uow.Session.QueryOver<OrganizationEntity>()
				.Where(x => x.TaxcomEdoAccountId == taxcomSettings.EdoAccount)
				.SingleOrDefaultAsync(cancellationToken);

			if(organization is null)
			{
				throw new InvalidOperationException("Не найдена организация");
			}

			var taxcomApiClient = _taxcomApiFactory.Create(taxcomSettings.TaxcomApiOptions);

			EdoDocFlowUpdates docFlowUpdates;
			var lastEventTime = timeFrom;
			do
			{
				var response = await taxcomApiClient.GetDocFlowsUpdates(
					new GetDocFlowsUpdatesParameters
					{
						//смотрим только доки, ожидающих подписи
						DocFlowStatus = "WaitingForSignature",
						LastEventTimeStamp = lastEventTime.ToBinary(),
						DocFlowDirection = "Ingoing",
						DepartmentId = null,
						IncludeTransportInfo = true
					},
					cancellationToken
				);

				if(!response.Ok)
				{
					_logger.LogError("При запросе обновлений ДО произошла ошибка: {ErrorMessage}", response.ErrorMessage);
					return;
				}
				
				docFlowUpdates = response.Result;

				if(docFlowUpdates.Updates is null)
				{
					return;
				}

				var docflows = await _uow.Session.QueryOver<TaxcomDocflow>()
					.WhereRestrictionOn(x => x.DocflowId).IsIn(docFlowUpdates.Updates.Select(d => d.Id).ToArray())
					.ListAsync(cancellationToken);

				foreach(var item in docFlowUpdates.Updates)
				{
					var docflow = docflows.FirstOrDefault(x => x.DocflowId == item.Id);
					if(docflow == null)
					{
						continue;
					}
					var lastStatus = docflow.Actions.Last();
					if(lastStatus.DocFlowState == EdoDocFlowStatus.InProgress)
					{
						await SendAcceptingIngoingTaxcomDocflowWaitingForSignatureEvent(item, organization, cancellationToken);
					}
					lastEventTime = item.StatusChangeDateTime;
				}

			} while(!docFlowUpdates.IsLast && lastEventTime < timeTo);
		}

		private async Task SendAcceptingIngoingTaxcomDocflowWaitingForSignatureEvent(
			EdoDocFlow docflow,
			OrganizationEntity organization,
			CancellationToken cancellationToken
			)
		{
			var @event = new AcceptingIngoingTaxcomDocflowWaitingForSignatureEvent
			{
				DocFlowId = docflow.Id,
				Organization = organization.Name,
				MainDocumentId = docflow.Documents.First().ExternalIdentifier,
				EdoAccount = organization.TaxcomEdoAccountId,
			};

			await _messageBus.Publish(@event, cancellationToken);
		}
	}
}
