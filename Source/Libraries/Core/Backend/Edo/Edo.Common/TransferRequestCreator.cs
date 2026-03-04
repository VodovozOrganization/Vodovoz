using Core.Infrastructure;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Common
{
	public class TransferRequestCreator
	{
		private readonly IEdoRepository _edoRepository;

		public TransferRequestCreator(IEdoRepository edoRepository)
		{
			_edoRepository = edoRepository ?? throw new ArgumentNullException(nameof(edoRepository));
		}

		public async Task<TransferEdoRequestIteration> CreateTransferRequests(
			IUnitOfWork _uow,
			OrderEdoTask edoTask,
			EdoTaskItemTrueMarkStatusProvider taskItemStatusProvider,
			CancellationToken cancellationToken)
		{
			if(edoTask.TransferIterations.Any(x => x.Status == TransferEdoRequestIterationStatus.InProgress))
			{
				throw new EdoException($"Задача №{edoTask.Id} в данный момент уже содержит выполняющийся перенос. " +
					$"Повторный перенос кодов в данный момент невозможен.");
			}

			var itemStatuses = await taskItemStatusProvider.GetItemsStatusesAsync(cancellationToken);

			var edoOrganizations = await _edoRepository.GetEdoOrganizationsAsync(cancellationToken);
			var organizationTo = edoTask.FormalEdoRequest.Order.Contract.Organization;

			var transferRequests = new Dictionary<string, TransferEdoRequest>();
			foreach(var itemStatus in itemStatuses.Values)
			{
				if(cancellationToken.IsCancellationRequested)
				{
					return null;
				}

				if(itemStatus.ProductInstanceStatus == null)
				{
					throw new EdoException($"Строка №{itemStatus.EdoTaskItem.Id} в задаче " +
						$"№{itemStatus.EdoTaskItem.CustomerEdoTask.Id} не была проверена в честном знаке." +
						$"Эта проблема должна обрабатываться валидацией, необходимо проверить работу валидатора.");
				}

				if(itemStatus.ItemCodeType == EdoTaskItemCodeType.Source)
				{
					continue;
				}

				var edoOrganizationFrom = edoOrganizations.FirstOrDefault(x => x.INN == itemStatus.ProductInstanceStatus.OwnerInn);
				if(edoOrganizationFrom == null)
				{
					throw new EdoException($"ЭДО организация с ИНН {itemStatus.ProductInstanceStatus.OwnerInn} " +
						$"({itemStatus.ProductInstanceStatus.OwnerName}) не найдена." +
						$"Эта проблема должна обрабатываться валидацией, необходимо проверить работу валидатора.");
				}

				if(edoOrganizationFrom.Id == organizationTo.Id)
				{
					continue;
				}

				var transferRequest = transferRequests.GetOrAdd(itemStatus.ProductInstanceStatus.OwnerInn, 
					(inn) => new TransferEdoRequest
					{
						FromOrganizationId = edoOrganizationFrom.Id,
						ToOrganizationId = organizationTo.Id,
					}
				);

				if(!transferRequest.TransferedItems.Contains(itemStatus.EdoTaskItem))
				{
					transferRequest.TransferedItems.Add(itemStatus.EdoTaskItem);
				}
			}

			TransferInitiator initiator;
			switch(edoTask.TaskType)
			{
				case EdoTaskType.Document:
					initiator = TransferInitiator.Document;
					break;
				case EdoTaskType.Receipt:
					initiator = TransferInitiator.Receipt;
					break;
				case EdoTaskType.Tender:
					initiator = TransferInitiator.Tender;
					break;
				default:
					throw new NotSupportedException($"Тип задачи {edoTask.TaskType} не поддерживается.");
			}

			var transferIteration = new TransferEdoRequestIteration
			{
				OrderEdoTask = edoTask,
				Status = TransferEdoRequestIterationStatus.InProgress,
				Initiator = initiator,
			};

			foreach(var transferRequest in transferRequests.Values)
			{
				transferRequest.Iteration = transferIteration;
				transferIteration.TransferEdoRequests.Add(transferRequest);
			}

			await _uow.SaveAsync(transferIteration, cancellationToken: cancellationToken);

			return transferIteration;
		}
	}
}
