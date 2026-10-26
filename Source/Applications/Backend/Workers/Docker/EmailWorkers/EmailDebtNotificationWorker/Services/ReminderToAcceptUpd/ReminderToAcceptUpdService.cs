using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TaxcomEdo.Client;
using Vodovoz.Core.Data.Repositories;

namespace EmailDebtNotificationWorker.Services.ReminderToAcceptUpd
{
	public class ReminderToAcceptUpdService : IReminderToAcceptUpdService
	{
		// UoW и IPublishEndpoint из одного DI-scope (прохода воркера): письма сохраняются в outbox при коммите этого UoW
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEdoRepository _edoRepository;
		private readonly ITaxcomApiClient _taxcomApiClient;
		private readonly IReminderToAcceptUpdEmailPreparer _reminderToAcceptUpdEmailPreparer;
		private readonly ILogger<ReminderToAcceptUpdService> _logger;
		private readonly IPublishEndpoint _publishEndpoint;

		public ReminderToAcceptUpdService(
			IUnitOfWork unitOfWork,
			IEdoRepository edoRepository,
			ITaxcomApiClient taxcomApiClient,
			IReminderToAcceptUpdEmailPreparer reminderToAcceptUpdEmailPreparer,
			ILogger<ReminderToAcceptUpdService> logger,
			IPublishEndpoint publishEndpoint)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_taxcomApiClient = taxcomApiClient ?? throw new ArgumentNullException(nameof(taxcomApiClient));
			_reminderToAcceptUpdEmailPreparer = reminderToAcceptUpdEmailPreparer ?? throw new ArgumentNullException(nameof(reminderToAcceptUpdEmailPreparer));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
		}

		public async Task RemindToAcceptUpd(int timeoutDays, CancellationToken cancellationToken)
		{
			var timedOutDocFlowNodes = (await _edoRepository.GetTimedOutDocFlows(_unitOfWork, timeoutDays, cancellationToken))
				.ToList();

			var checkedTimedOutDocFlowNodes = await CheckDocFlowStatusInTaxcom(timedOutDocFlowNodes);

			var reminderEmails = await _reminderToAcceptUpdEmailPreparer.PrepareReminderToAcceptUpdEmails(_unitOfWork, checkedTimedOutDocFlowNodes, cancellationToken);

			foreach(var sendEmailMessage in reminderEmails)
			{
				await _publishEndpoint.Publish(sendEmailMessage, cancellationToken);
			}

			await _unitOfWork.CommitAsync(cancellationToken: cancellationToken);
		}

		private async Task<List<TimedOutDocFlowGrouppedNode>> CheckDocFlowStatusInTaxcom(List<TimedOutDocFlowGrouppedNode> timedOutDocFlowNodes)
		{
			foreach(var node in timedOutDocFlowNodes)
			{
				foreach(var document in node.Documents.ToList())
				{
					try
					{
						var description = await _taxcomApiClient.GetDocflowStatus(document.TaxcomDocflow.DocflowId.ToString(), document.OurEdoAccount);

						var mainDocument = description.DocFlow.Documents.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Definition.Identifiers.ExternalIdentifier));

						if(mainDocument is null)
						{
							node.Documents.Remove(document);
							continue;
						}

						if(description.DocFlow.Status != "Sent")
						{
							node.Documents.Remove(document);
							continue;
						}
					}
					catch(Exception ex)
					{
						node.Documents.Remove(document);
						_logger.LogError(ex, "Ошибка при проверке статуса документооборота в Такскоме для документа {DocflowId}: {Exception}." , document.TaxcomDocflow.DocflowId, ex);
						continue;
					}
				}
			}

			timedOutDocFlowNodes.RemoveAll(x => x.Documents.Count == 0);

			return timedOutDocFlowNodes;
		}
	}
}


