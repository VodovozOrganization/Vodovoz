﻿using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;

namespace Vodovoz.EntityRepositories.Complaints
{
	public class ComplaintsRepository : IComplaintsRepository
	{
		public IList<object[]> GetGuiltyAndCountForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null)
		{
			Complaint complaintAlias = null;
			Subdivision subdivisionAlias = null;
			Employee employeeAlias = null;
			ComplaintGuiltyItem guiltyItemAlias = null;

			var query = uow.Session.QueryOver(() => guiltyItemAlias)
						   .Left.JoinAlias(() => guiltyItemAlias.Complaint, () => complaintAlias)
						   .Left.JoinAlias(() => guiltyItemAlias.Subdivision, () => subdivisionAlias)
						   .Left.JoinAlias(() => guiltyItemAlias.Employee, () => employeeAlias)
						   .Where(() => complaintAlias.Status == ComplaintStatuses.Closed)
						   ;

			if(start != null && end != null)
				query.Where(() => complaintAlias.CreationDate >= start)
					 .Where(() => complaintAlias.CreationDate <= end);

			int i = 0;
			var result = query.SelectList(list => list
										  .SelectGroup(c => c.Id)
										  .Select(
											  Projections.SqlFunction(
												  new SQLFunctionTemplate(
													  NHibernateUtil.String,
													  "GROUP_CONCAT(CASE ?1 WHEN 'Employee' THEN IFNULL(CONCAT('Сотр: ', GET_PERSON_NAME_WITH_INITIALS(?3,?4,?5)), 'Отдел ВВ') WHEN 'Subdivision' THEN IFNULL(CONCAT('Отд: ', ?2), 'Отдел ВВ') WHEN 'Client' THEN 'Клиент' WHEN 'None' THEN 'Нет (не рекламация)' ELSE ?1 END ORDER BY ?1 ASC SEPARATOR '\n')"
													 ),
												  NHibernateUtil.String,
												  Projections.Property(() => guiltyItemAlias.GuiltyType),
												  Projections.Property(() => subdivisionAlias.Name),
												  Projections.Property(() => employeeAlias.LastName),
												  Projections.Property(() => employeeAlias.Name),
												  Projections.Property(() => employeeAlias.Patronymic)
												 )
											 )
										 )
							  .List<object[]>()
							  .GroupBy(x => x[1])
							  .Select(r => new[] { r.Key, r.Count(), i++ })
							  .ToList();

			return result;
		}

		public int GetUnclosedComplaintsCount(IUnitOfWork uow, bool? withOverdue = null, DateTime? start = null, DateTime? end = null)
		{
			var query = uow.Session.QueryOver<Complaint>()
						   .Where(c => c.Status != ComplaintStatuses.Closed)
						   ;

			if(start.HasValue && end.HasValue)
				query.Where(c => c.CreationDate >= start)
					 .Where(c => c.CreationDate <= end);

			if(withOverdue.HasValue && withOverdue.Value)
				query.Where(c => c.PlannedCompletionDate < DateTime.Today);

			if(withOverdue.HasValue && !withOverdue.Value)
				query.Where(c => c.PlannedCompletionDate >= DateTime.Today);

			return query.Select(Projections.Count<Complaint>(c => c.Id)).SingleOrDefault<int>();
		}

		public IList<object[]> GetComplaintsResults(IUnitOfWork uow, DateTime? start = null, DateTime? end = null)
		{
			ComplaintResult complaintResultAlias = null;

			var query = uow.Session.QueryOver<Complaint>()
						   .Left.JoinAlias(c => c.ComplaintResult, () => complaintResultAlias)
						   .Where(c => c.Status == ComplaintStatuses.Closed)
						   ;

			if(start != null && end != null)
				query.Where(c => c.CreationDate >= start)
					 .Where(c => c.CreationDate <= end);

			var result = query.SelectList(list => list
										  .SelectGroup(() => complaintResultAlias.Name)
										  .Select(
										  		Projections.SqlFunction(
													new SQLFunctionTemplate(NHibernateUtil.Int32, "COUNT(IFNULL(?1, 0))"),
													NHibernateUtil.Int32,
													Projections.Property(() => complaintResultAlias.Id)
												)
										  ))
							  .List<object[]>()
							  ;

			return result;
		}
	}
}