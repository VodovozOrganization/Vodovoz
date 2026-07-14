using MassTransit;
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
		private readonly IEdoRepository _edoRepository;
		private readonly ITaxcomApiClient _taxcomApiClient;
		private readonly IReminderToAcceptUpdEmailPreparer _reminderToAcceptUpdEmailPreparer;
		private readonly IBus _bus;

		public ReminderToAcceptUpdService(
			IEdoRepository edoRepository,
			ITaxcomApiClient taxcomApiClient,
			IReminderToAcceptUpdEmailPreparer reminderToAcceptUpdEmailPreparer,
			IBus bus)
		{
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
			_taxcomApiClient = taxcomApiClient ?? throw new ArgumentNullException(nameof(taxcomApiClient));
			_reminderToAcceptUpdEmailPreparer = reminderToAcceptUpdEmailPreparer ?? throw new ArgumentNullException(nameof(reminderToAcceptUpdEmailPreparer));
			_bus = bus ?? throw new ArgumentNullException(nameof(bus));
		}

		public async Task RemindToAcceptUpd(IUnitOfWork unitOfWork, int timeoutDays, CancellationToken cancellationToken)
		{
			var timedOutDocFlowNodes = (await _edoRepository.GetTimedOutDocFlows(unitOfWork, timeoutDays, cancellationToken))
				.ToList();

			var checkedTimedOutDocFlowNodes = await CheckDocFlowStatusInTaxcom(timedOutDocFlowNodes);

			var reminderEmails = await _reminderToAcceptUpdEmailPreparer.PrepareReminderToAcceptUpdEmails(unitOfWork, checkedTimedOutDocFlowNodes, cancellationToken);

			foreach(var sendEmailMessage in reminderEmails)
			{
				await _bus.Publish(sendEmailMessage, cancellationToken);
			}

			await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
		}

		private async Task<List<TimedOutDocFlowGrouppedNode>> CheckDocFlowStatusInTaxcom(List<TimedOutDocFlowGrouppedNode> timedOutDocFlowNodes)
		{
			foreach(var node in timedOutDocFlowNodes)
			{
				foreach(var document in node.Documents.ToList())
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
			}

			timedOutDocFlowNodes.RemoveAll(x => x.Documents.Count == 0);

			return timedOutDocFlowNodes;
		}
	}
}


