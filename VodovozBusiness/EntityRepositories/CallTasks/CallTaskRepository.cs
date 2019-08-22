using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.EntityRepositories.CallTasks
{
	public class CallTaskRepository : ICallTaskRepository
	{
		public IList<CallTask> GetTasksByAddress(IUnitOfWork UoW, DeliveryPoint deliveryPoint, int count = int.MaxValue)
		{
			CallTask callTaskAlias = null;
			var tasks = UoW.Session.QueryOver(() => callTaskAlias)
										  .Where(() => callTaskAlias.DeliveryPoint.Id == deliveryPoint.Id)
										  .Take(count)
										  .List();
			return tasks;
		}

		public IList<CallTask> GetTasksByClient(IUnitOfWork UoW, Counterparty counterparty, int count = int.MaxValue)
		{
			CallTask callTaskAlias = null;
			var tasks = UoW.Session.QueryOver(() => callTaskAlias)
										  .Where(() => callTaskAlias.DeliveryPoint.Counterparty.Id == counterparty.Id)
										  .Take(count)
										  .List();
			return tasks;
		}

		public IList<CallTask> GetTasksByPeriod(IUnitOfWork UoW, DateTime startDate, DateTime endDate)
		{
			CallTask callTaskAlias = null;
			var tasks = UoW.Session.QueryOver(() => callTaskAlias)
										  .Where(() => callTaskAlias.EndActivePeriod >= startDate && callTaskAlias.EndActivePeriod <= endDate)
										  .List();
			return tasks;
		}

		public string GetCommentsByDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint)
		{
			CallTask callTaskAlias = null;
			string comments = String.Empty;
			var tasks = UoW.Session.QueryOver(() => callTaskAlias)
				.Where(x => x.DeliveryPoint.Id == deliveryPoint.Id)
				.List();
			foreach(var task in tasks)
				comments += task.Comment;
			return comments;
		}

		public string GetCommentsByDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint, CallTask callTask)
		{
			CallTask callTaskAlias = null;
			string comments = String.Empty;
			var tasks = UoW.Session.QueryOver(() => callTaskAlias)
				.Where(x => x.DeliveryPoint.Id == deliveryPoint.Id)
				.And(x => x.Id != callTask.Id)
				.List();
			foreach(var task in tasks)
				comments += task.Comment;
			return comments;
		}
	}
}
