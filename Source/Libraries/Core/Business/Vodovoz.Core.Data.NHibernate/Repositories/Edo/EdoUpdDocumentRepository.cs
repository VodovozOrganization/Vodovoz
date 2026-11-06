using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Core.Data.Repositories;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Core.Domain.Edo.Upd;

namespace Vodovoz.Core.Data.NHibernate.Repositories.Edo
{
	/// <inheritdoc cref="IEdoUpdDocumentRepository"/>
	public class EdoUpdDocumentRepository : IEdoUpdDocumentRepository
	{
		/// <inheritdoc/>
		public IList<EdoUpdDocumentNode> GetItems(IUnitOfWork uow, EdoUpdDocumentFilter filter, int? offset, int? limit)
		{
			EdoTask taskAlias = null;
			EdoUpdDocument documentAlias = null;
			EdoUpdDocumentNode resultAlias = null;
			var query = CreateQuery(uow, filter)
				.OrderBy(() => documentAlias.Date).Desc
				.ThenBy(() => documentAlias.Id).Desc
				.ThenBy(() => taskAlias.Id).Desc
				.SelectList(select => select
					.Select(() => documentAlias.Id).WithAlias(() => resultAlias.Id)
					.Select(() => documentAlias.Number).WithAlias(() => resultAlias.Number)
					.Select(() => documentAlias.Date).WithAlias(() => resultAlias.Date)
					.Select(() => documentAlias.Sum).WithAlias(() => resultAlias.Sum)
					.Select(() => documentAlias.Seller.Name).WithAlias(() => resultAlias.Seller)
					.Select(() => documentAlias.Customer.Name).WithAlias(() => resultAlias.Customer)
					.Select(() => documentAlias.Customer.Inn).WithAlias(() => resultAlias.CustomerInn)
					.Select(Projections.Conditional(
						Restrictions.Where(() => taskAlias.GetType() == typeof(TransferEdoTask)),
						Projections.Constant(nameof(EdoTaskType.Transfer)),
						Projections.Constant(nameof(EdoTaskType.Document))))
					.WithAlias(() => resultAlias.TaskType)
					.Select(() => taskAlias.Id).WithAlias(() => resultAlias.TaskId))
				.TransformUsing(Transformers.AliasToBean<EdoUpdDocumentNode>());
			if(offset.HasValue)
			{
				query.Skip(offset.Value);
			}
			if(limit.HasValue)
			{
				query.Take(limit.Value);
			}
			return query.List<EdoUpdDocumentNode>();
		}

		/// <inheritdoc/>
		public int GetItemsCount(IUnitOfWork uow, EdoUpdDocumentFilter filter)
		{
			return CreateQuery(uow, filter).RowCount();
		}

		/// <inheritdoc/>
		public EdoUpdDocument GetDocument(IUnitOfWork uow, int documentId) => uow.GetById<EdoUpdDocument>(documentId);

		private static IQueryOver<EdoTask, EdoTask> CreateQuery(IUnitOfWork uow, EdoUpdDocumentFilter filter)
		{
			EdoTask taskAlias = null;
			EdoUpdDocument documentAlias = null;
			var query = uow.Session.QueryOver(() => taskAlias)
				.JoinEntityAlias(() => documentAlias,
					() => ((DocumentEdoTask)taskAlias).Upd.Id == documentAlias.Id, JoinType.InnerJoin)
				.Where(() => taskAlias.GetType() == typeof(DocumentEdoTask)
					|| taskAlias.GetType() == typeof(TransferEdoTask));
			if(filter.DateFrom.HasValue)
			{
				query.Where(() => documentAlias.Date >= filter.DateFrom.Value);
			}
			if(filter.DateToExclusive.HasValue)
			{
				query.Where(() => documentAlias.Date < filter.DateToExclusive.Value);
			}
			if(filter.TaskType.HasValue)
			{
				var taskClass = filter.TaskType == EdoTaskType.Transfer ? typeof(TransferEdoTask) : typeof(DocumentEdoTask);
				query.Where(() => taskAlias.GetType() == taskClass);
			}
			if(filter.CustomerInn != null)
			{
				query.Where(() => documentAlias.Customer.Inn == filter.CustomerInn);
			}
			return query;
		}
	}
}
