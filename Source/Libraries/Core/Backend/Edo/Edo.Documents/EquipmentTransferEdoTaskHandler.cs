using Edo.Contracts.Messages.Events;
using MassTransit;
using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Documents
{
	/// <summary>
	/// Обработчик задач ЭДО по документу акт приёмапередачи оборудования
	/// </summary>
	public class EquipmentTransferEdoTaskHandler : IDisposable
	{
		private readonly IUnitOfWork _uow;
		private readonly IBus _messageBus;

		public EquipmentTransferEdoTaskHandler(
			IUnitOfWork uow,
			IBus messageBus
			)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
		}

		/// <summary>
		/// Отправка документа передачи оборудования через ЭДО
		/// </summary>
		/// <param name="equipmentTransferEdoTaskId"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		public async Task SendTransferDocument(int equipmentTransferEdoTaskId, CancellationToken cancellationToken)
		{
			var customerDocument = await SendEquipmentTransferDocument(equipmentTransferEdoTaskId, cancellationToken);
			var edoTask = await _uow.Session.GetAsync<EquipmentTransferEdoTask>(equipmentTransferEdoTaskId, cancellationToken);
			var message = new EquipmentTransferDocumentSendEvent { EquipmentTransferDocumentId = customerDocument.Id };

			edoTask.Status = EdoTaskStatus.InProgress;

			await _uow.SaveAsync(edoTask, cancellationToken: cancellationToken);
			await _uow.CommitAsync(cancellationToken);

			await _messageBus.Publish(message, cancellationToken);
		}

		private async Task<EquipmentTransferEdoDocument> SendEquipmentTransferDocument(int edoTaskId, CancellationToken cancellationToken)
		{
			var customerEdoDocument = new EquipmentTransferEdoDocument
			{
				DocumentTaskId = edoTaskId,
				Status = EdoDocumentStatus.NotStarted,
				EdoType = EdoType.Taxcom,
				DocumentType = EdoDocumentType.EquipmentTransfer,
				Type = OutgoingEdoDocumentType.EquipmentTransfer
			};

			await _uow.SaveAsync(customerEdoDocument, cancellationToken: cancellationToken);
			return customerEdoDocument;
		}

		public void Dispose()
		{
			_uow.Dispose();
		}
	}
}
