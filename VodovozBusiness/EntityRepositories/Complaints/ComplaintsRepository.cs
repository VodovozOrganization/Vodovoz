using System;
using System.Collections.Generic;
using System.Linq;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;

namespace Vodovoz.EntityRepositories.Complaints
{
	public class ComplaintsRepository : IComplaintsRepository
	{
		public IList<ComplaintGuiltyNode> GetGuiltyAndCountForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null)
		{
			Complaint complaintAlias = null;
			Subdivision subdivisionAlias = null;
			Employee employeeAlias = null;
			ComplaintGuiltyItem guiltyItemAlias = null;
			ComplaintResult complaintResultAlias = null;
			QueryNode queryNodeAlias = null;

			var query = uow.Session.QueryOver(() => guiltyItemAlias)
						   .Left.JoinAlias(() => guiltyItemAlias.Complaint, () => complaintAlias)
						   .Left.JoinAlias(() => complaintAlias.ComplaintResult, () => complaintResultAlias)
						   .Left.JoinAlias(() => guiltyItemAlias.Subdivision, () => subdivisionAlias)
						   .Left.JoinAlias(() => guiltyItemAlias.Employee, () => employeeAlias)
						   ;

			if(start != null && end != null)
				query.Where(() => complaintAlias.CreationDate >= start)
					 .Where(() => complaintAlias.CreationDate <= end);
					
			var result = query.SelectList(list => list
										  .SelectGroup(c => c.Id)
										  .Select(() => complaintAlias.Status).WithAlias(() => queryNodeAlias.Status)
										  .Select(() => complaintResultAlias.Name).WithAlias(() => queryNodeAlias.ResultText)
										  .Select(
											  Projections.SqlFunction(
												  new SQLFunctionTemplate(
													  NHibernateUtil.String,
													  "GROUP_CONCAT(CASE ?1 WHEN 'Employee' THEN IFNULL(CONCAT('Сотр: ', GET_PERSON_NAME_WITH_INITIALS(?3,?4,?5)), 'Отдел ВВ') WHEN 'Subdivision' THEN IFNULL(CONCAT('Отд: ', ?2), 'Отдел ВВ') WHEN 'Client' THEN 'Клиент' WHEN 'None' THEN 'Нет (не жалоба)' ELSE ?1 END ORDER BY ?1 ASC SEPARATOR '\n')"
													 ),
												  NHibernateUtil.String,
												  Projections.Property(() => guiltyItemAlias.GuiltyType),
												  Projections.Property(() => subdivisionAlias.Name),
												  Projections.Property(() => employeeAlias.LastName),
												  Projections.Property(() => employeeAlias.Name),
												  Projections.Property(() => employeeAlias.Patronymic)
												 )
											 ).WithAlias(() => queryNodeAlias.GuiltyName)
										 )
							  .TransformUsing(Transformers.AliasToBean<QueryNode>()).List<QueryNode>();
			var groupedResult = result.GroupBy(p => p.GuiltyName, (guiltyName, guilties) => new ComplaintGuiltyNode {
				GuiltyName = guiltyName,
				Count = guilties.Count(),
				Guilties = guilties.ToList()
			}).ToList();
			foreach(var item in groupedResult) {
				item.CreateComplaintResultNodes();
			}
			return groupedResult;
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

	#region Nodes for GetGuiltyAndCountForDates()

	public class QueryNode
	{
		public ComplaintStatuses Status { get; set; }
		public string ResultText { get; set; }
		public string GuiltyName { get; set; }
	}

	public class ComplaintGuiltyNode
	{
		public int Count { get; set; }
		public string GuiltyName { get; set; }
		public IList<ComplaintResultNode> ComplaintResultNodes { get; set; }

		public IList<QueryNode> Guilties { get; set; }

		public void CreateComplaintResultNodes()
		{
			ComplaintResultNodes = new List<ComplaintResultNode>();

			var resultNodes = Guilties.GroupBy(p => new { p.Status, p.ResultText }, (grp, innerGuilties) => new ComplaintResultNode {
				Count = innerGuilties.Count(),
				Status = grp.Status,
				Text = grp.Status == ComplaintStatuses.Closed ? grp.ResultText : ComplaintStatuses.InProcess.GetEnumTitle(),
				ComplaintGuiltyNode = this,
			}).ToList();

			//Объединяю ноды со статусами "В работе" и "На проверке"
			if(resultNodes.Count(n => n.Status == ComplaintStatuses.InProcess || n.Status == ComplaintStatuses.Checking) > 1) {
				var nodesToUnion = resultNodes.Where(n => n.Status == ComplaintStatuses.InProcess || n.Status == ComplaintStatuses.Checking).ToList();
				nodesToUnion[0].Count = nodesToUnion.Sum(n => n.Count);
				foreach(var node in nodesToUnion.Skip(1)) {
					resultNodes.Remove(node);
				}
			}
			foreach(var node in resultNodes) {

				ComplaintResultNodes.Add(node);
			}
		}
	}

	public class ComplaintResultNode
	{
		public ComplaintGuiltyNode ComplaintGuiltyNode { get; set; }
		public string Text { get; set; }
		public int Count { get; set; }
		public ComplaintStatuses Status { get; set; }
	}

	#endregion
}