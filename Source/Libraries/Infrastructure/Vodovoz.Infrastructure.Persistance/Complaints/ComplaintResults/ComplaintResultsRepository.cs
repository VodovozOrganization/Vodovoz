using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Complaints;
using Vodovoz.Domain.Complaints;
using Vodovoz.EntityRepositories.Complaints.ComplaintResults;

namespace Vodovoz.Infrastructure.Persistance.Complaints.ComplaintResults
{
	internal sealed class ComplaintResultsRepository : IComplaintResultsRepository
	{
		public IEnumerable<ComplaintResultOfCounterparty> GetActiveResultsOfCounterparty(IUnitOfWork uow) =>
			uow.Session.QueryOver<ComplaintResultOfCounterparty>().Where(x => !x.IsArchive).List();

		public IEnumerable<ComplaintResultOfCounterparty> GetActiveResultsOfCounterpartyWithSelectedResult(IUnitOfWork uow, int resultId)
		{
			return uow.Session.QueryOver<ComplaintResultOfCounterparty>()
				.Where(x => !x.IsArchive || x.Id == resultId)
				.List();
		}

		public IEnumerable<ComplaintResultOfCounterparty> GetAllResultsOfCounterparty(IUnitOfWork uow) =>
			uow.GetAll<ComplaintResultOfCounterparty>();

		public IEnumerable<ComplaintResultOfEmployees> GetActiveResultsOfEmployees(IUnitOfWork uow) =>
			uow.Session.QueryOver<ComplaintResultOfEmployees>().Where(x => !x.IsArchive).List();

		public IEnumerable<ComplaintResultOfEmployees> GetActiveResultsOfEmployeesWithSelectedResult(IUnitOfWork uow, int resultId)
		{
			return uow.Session.QueryOver<ComplaintResultOfEmployees>()
				.Where(x => !x.IsArchive || x.Id == resultId)
				.List();
		}

		public IEnumerable<ComplaintResultOfEmployees> GetAllResultsOfEmployees(IUnitOfWork uow) =>
			uow.GetAll<ComplaintResultOfEmployees>();

		public IList<ClosedComplaintResultNode> GetComplaintsResultsOfCounterparty(IUnitOfWork uow, DateTime? start = null, DateTime? end = null)
		{
			ComplaintResultOfCounterparty resultOfCounterpartyAlias = null;
			ClosedComplaintResultNode resultAlias = null;

			var query = uow.Session.QueryOver<Complaint>()
				.Left.JoinAlias(c => c.ComplaintResultOfCounterparty, () => resultOfCounterpartyAlias)
				.Where(c => c.Status == ComplaintStatuses.Closed);

			AddDateRestriction(start, end, query);

			return query.SelectList(list => list
					.SelectGroup(() => resultOfCounterpartyAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.Int32, "COUNT(IFNULL(?1, 0))"),
						NHibernateUtil.Int32,
						Projections.Property(() => resultOfCounterpartyAlias.Id))).WithAlias(() => resultAlias.Count))
				.TransformUsing(Transformers.AliasToBean<ClosedComplaintResultNode>())
				.List<ClosedComplaintResultNode>();
		}

		public IList<ClosedComplaintResultNode> GetComplaintsResultsOfEmployees(IUnitOfWork uow, DateTime? start = null, DateTime? end = null)
		{
			ComplaintResultOfEmployees resultOfEmployeesAlias = null;
			ClosedComplaintResultNode resultAlias = null;

			var query = uow.Session.QueryOver<Complaint>()
				.Left.JoinAlias(c => c.ComplaintResultOfEmployees, () => resultOfEmployeesAlias)
				.Where(c => c.Status == ComplaintStatuses.Closed);

			AddDateRestriction(start, end, query);

			return query.SelectList(list => list
					.SelectGroup(() => resultOfEmployeesAlias.Name).WithAlias(() => resultAlias.Name)
					.Select(Projections.SqlFunction(
						new SQLFunctionTemplate(NHibernateUtil.Int32, "COUNT(IFNULL(?1, 0))"),
						NHibernateUtil.Int32,
						Projections.Property(() => resultOfEmployeesAlias.Id))).WithAlias(() => resultAlias.Count))
				.TransformUsing(Transformers.AliasToBean<ClosedComplaintResultNode>())
				.List<ClosedComplaintResultNode>();
		}

		private void AddDateRestriction(DateTime? start, DateTime? end, IQueryOver<Complaint, Complaint> query)
		{
			if(start.HasValue && end.HasValue)
			{
				query.Where(c => c.CreationDate >= start)
					.And(c => c.CreationDate <= end);
			}
		}
	}
}
