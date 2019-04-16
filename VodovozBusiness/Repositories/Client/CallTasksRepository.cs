using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;

namespace Vodovoz.Repositories.Client
{
	public static class CallTasksRepository
	{
		public static IList<CallTask> GetTasksByAddress(IUnitOfWork UoW, DeliveryPoint deliveryPoint , int count = int.MaxValue)
		{
			CallTask callTaskAlias = null;
			var tasks = UoW.Session.QueryOver<CallTask>(() => callTaskAlias)
										  .Where(() => callTaskAlias.DeliveryPoint.Id == deliveryPoint.Id)
										  .Take(count)
										  .List();
			return tasks;
		}

		public static IList<CallTask> GetTasksByClient(IUnitOfWork UoW, Counterparty counterparty , int count = int.MaxValue)
		{
			CallTask callTaskAlias = null;
			var tasks = UoW.Session.QueryOver<CallTask>(() => callTaskAlias)
										  .Where(() => callTaskAlias.DeliveryPoint.Counterparty.Id == counterparty.Id)
										  .Take(count)
										  .List();
			return tasks;
		}

		public static IList<CallTask> GetTasksByPeriod(IUnitOfWork UoW , DateTime startDate  , DateTime endDate )
		{
			CallTask callTaskAlias = null;
			var tasks = UoW.Session.QueryOver<CallTask>(() => callTaskAlias)
										  .Where(() => callTaskAlias.EndActivePeriod >= startDate && callTaskAlias.EndActivePeriod <= endDate)
										  .List();
			return tasks;
		}

		public static string GetCommentsByDeliveryPoint(IUnitOfWork UoW , DeliveryPoint deliveryPoint)
		{
			CallTask callTaskAlias = null;
			string comments = String.Empty;
			var tasks = UoW.Session.QueryOver<CallTask>(() => callTaskAlias)
				.Where(x => x.DeliveryPoint.Id == deliveryPoint.Id)
				.List();
			foreach(var task in tasks)
				comments += task.Comment;
			return comments;
		}

		public static string GetCommentsByDeliveryPoint(IUnitOfWork UoW, DeliveryPoint deliveryPoint ,CallTask callTask)
		{
			CallTask callTaskAlias = null;
			string comments = String.Empty;
			var tasks = UoW.Session.QueryOver<CallTask>(() => callTaskAlias)
				.Where(x => x.DeliveryPoint.Id == deliveryPoint.Id)
				.And(x => x.Id != callTask.Id)
				.List();
			foreach(var task in tasks)
				comments += task.Comment;
			return comments;
		}
	}
}
