using MassTransit;
using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Edo.Contracts.Messages.Events;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Transfer.Sender
{
	public class TransferSendHandler : IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly ILogger<TransferSendHandler> _logger;
		private readonly IBus _messageBus;

		public TransferSendHandler(
			ILogger<TransferSendHandler> logger,
			IUnitOfWork uow, 
			IBus messageBus)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));

		}

		public async Task HandleReadyToSend(int transferTaskId, CancellationToken cancellationToken)
		{
			_uow.OpenTransaction();

			var transferDocument = await _uow.Session.QueryOver<TransferEdoDocument>()
				.Where(x => x.TransferTaskId == transferTaskId)
				.SingleOrDefaultAsync(cancellationToken);

			if(transferDocument != null)
			{
				_logger.LogError("При отправке документа на трансфер обнаружен уже существующий документ на трансфер.");
				return;
			}

			transferDocument = new TransferEdoDocument
			{
				TransferTaskId = transferTaskId,
				DocumentType = EdoDocumentType.UPD,
				EdoType = EdoType.Taxcom,
				SendTime = DateTime.Now,
				Status = EdoDocumentStatus.NotStarted
			};

			var transferTask = await _uow.Session.GetAsync<TransferEdoTask>(transferTaskId);
			transferTask.TransferStatus = TransferEdoTaskStatus.InProgress;
			transferTask.TransferStartTime = DateTime.Now;

			await _uow.SaveAsync(transferDocument, cancellationToken: cancellationToken);
			await _uow.SaveAsync(transferTask, cancellationToken: cancellationToken);

			await _uow.CommitAsync();

			var message = new TransferDocumentSendEvent
			{
				Id = transferDocument.Id
			};
			await _messageBus.Publish(message, cancellationToken);
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
