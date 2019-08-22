using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.CallTasks
{
	public interface ICallTaskRepository
	{
		string GetCommentsByDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint);
		string GetCommentsByDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint, CallTask callTask);
		IList<CallTask> GetTasksByAddress(IUnitOfWork UoW, DeliveryPoint deliveryPoint, int count = int.MaxValue);
		IList<CallTask> GetTasksByClient(IUnitOfWork UoW, Counterparty counterparty, int count = int.MaxValue);
		IList<CallTask> GetTasksByPeriod(IUnitOfWork UoW, DateTime startDate, DateTime endDate);
	}
}