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

		public async Task CreateTransferRequests(
			IUnitOfWork _uow,
			OrderEdoTask edoTask,
			EdoTaskItemTrueMarkStatusProvider taskItemStatusProvider,
			CancellationToken cancellationToken)
		{
			if(edoTask.TransferEdoRequests.Any())
			{
				throw new EdoException($"Задача №{edoTask.Id} уже содержит заявки на перенос кодов. " +
					$"Повторный перенос кодов не предусмотрен.");
			}

			var itemStatuses = await taskItemStatusProvider.GetItemsStatusesAsync(cancellationToken);

			var edoOrganizations = await _edoRepository.GetEdoOrganizationsAsync(cancellationToken);
			var organizationTo = edoTask.OrderEdoRequest.Order.Contract.Organization;

			var transferRequests = new Dictionary<string, TransferEdoRequest>();
			foreach(var itemStatus in itemStatuses.Values)
			{
				if(cancellationToken.IsCancellationRequested)
				{
					return;
				}

				if(itemStatus.ProductInstanceStatus == null)
				{
					throw new EdoException($"Строка №{itemStatus.EdoTaskItem.Id} в задаче " +
						$"№{itemStatus.EdoTaskItem.CustomerEdoTask.Id} не была проверена в честном знаке." +
						$"Эта проблема должна обрабатываться валидацией, необходимо проверить работу валидатора.");
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
						OrderEdoTask = edoTask,
					}
				);

				if(!transferRequest.TransferedItems.Contains(itemStatus.EdoTaskItem))
				{
					transferRequest.TransferedItems.Add(itemStatus.EdoTaskItem);
				}
			}

			foreach(var transferRequest in transferRequests.Values)
			{
				await _uow.SaveAsync(transferRequest, cancellationToken: cancellationToken);
			}
		}
	}
}
