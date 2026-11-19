using System;
using System.Threading;
using System.Threading.Tasks;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Repositories.Document;
using Vodovoz.Core.Domain.Documents;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Core.Domain.Results;

namespace Edo.Transfer.Sender
{
	/// <summary>
	/// Подготавливает шапку заказа трансфера по минимальной дате доставки связанных заказов.
	/// </summary>
	public class TransferOrderHeaderPreparer : ITransferOrderHeaderPreparer
	{
		private readonly IUnitOfWork _uow;
		private readonly ITransferTaskRepository _transferTaskRepository;
		private readonly IDocumentOrganizationCounterRepository _documentOrganizationCounterRepository;

		/// <summary>
		/// Конструктор.
		/// </summary>
		/// <param name="uow">Контекст работы с данными.</param>
		/// <param name="transferTaskRepository">Репозиторий задач трансфера.</param>
		/// <param name="documentOrganizationCounterRepository">Репозиторий счетчиков документов организаций.</param>
		public TransferOrderHeaderPreparer(
			IUnitOfWork uow,
			ITransferTaskRepository transferTaskRepository,
			IDocumentOrganizationCounterRepository documentOrganizationCounterRepository)
		{
			_uow = uow ?? throw new ArgumentNullException(nameof(uow));
			_transferTaskRepository = transferTaskRepository ?? throw new ArgumentNullException(nameof(transferTaskRepository));
			_documentOrganizationCounterRepository = documentOrganizationCounterRepository ?? throw new ArgumentNullException(nameof(documentOrganizationCounterRepository));
		}

		/// <inheritdoc/>
		public async Task<Result<TransferOrder>> PrepareAsync(
			TransferEdoTask transferEdoTask,
			CancellationToken cancellationToken)
		{
			var transferDate = await _transferTaskRepository.GetMinOrderDeliveryDateForTransferTaskAsync(
				_uow,
				transferEdoTask.Id,
				cancellationToken);
			if(!transferDate.HasValue)
			{
				throw new InvalidOperationException("Невозможно определить дату УПД трансфера: у связанных заказов отсутствует дата доставки.");
			}

			var seller = _uow.GetById<OrganizationEntity>(transferEdoTask.FromOrganizationId);
			var customer = _uow.GetById<OrganizationEntity>(transferEdoTask.ToOrganizationId);
			var transferDocumentCounter = await CreateDocumentOrganizationCounterAsync(transferDate.Value, seller, cancellationToken);

			return TransferOrder.Create(transferDate.Value, seller, customer, transferDocumentCounter);
		}

		private async Task<DocumentOrganizationCounter> CreateDocumentOrganizationCounterAsync(
			DateTime transferDate,
			OrganizationEntity seller,
			CancellationToken cancellationToken)
		{
			var lastDocument = await _documentOrganizationCounterRepository
				.GetMaxDocumentOrganizationCounterOnYearAsync(_uow, transferDate, seller, cancellationToken);
			var documentCounter = (lastDocument?.Counter ?? 0) + 1;

			var transferDocumentOrganization = new DocumentOrganizationCounter
			{
				Organization = seller,
				Counter = documentCounter,
				CounterDateYear = transferDate.Year,
				DocumentNumber = DocumentNumberBuilder.BuildDocumentNumber(seller, transferDate, documentCounter)
			};

			await _uow.SaveAsync(transferDocumentOrganization, cancellationToken: cancellationToken);
			return transferDocumentOrganization;
		}
	}
}
