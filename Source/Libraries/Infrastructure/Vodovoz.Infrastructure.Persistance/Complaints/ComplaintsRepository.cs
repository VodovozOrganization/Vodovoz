using DateTimeHelpers;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Complaints;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Complaints;
using Order = Vodovoz.Domain.Orders.Order;

namespace Vodovoz.Infrastructure.Persistance.Complaints
{
	internal sealed class ComplaintsRepository : IComplaintsRepository
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
												  Projections.Property(() => guiltyItemAlias.Responsible),
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

		public IList<int> GetUnclosedWithNoCommentsComplaintIdsBySubdivision(IUnitOfWork uow, int subdivisionId)
		{
			Complaint complaintAlias = null;
			ComplaintDiscussion complaintDiscussionAlias = null;
			ComplaintDiscussionComment complaintDiscussionCommentAlias = null;

			var complaintsIds = uow.Session.QueryOver(() => complaintDiscussionAlias)
				.Left.JoinAlias(() => complaintDiscussionAlias.Complaint, () => complaintAlias)
				.Left.JoinAlias(() => complaintDiscussionAlias.Comments, () => complaintDiscussionCommentAlias)
				.Where(() => complaintAlias.Status != ComplaintStatuses.Closed)
				.Where(() => complaintDiscussionAlias.Subdivision.Id == subdivisionId && complaintDiscussionAlias.Status != ComplaintDiscussionStatuses.Closed)
				.SelectList(list => list
					.Select(Projections.Group(() => complaintAlias.Id)))
				.Where(Restrictions.Eq(Projections.Count(() => complaintDiscussionCommentAlias.Id), 0))
				.List<int>();

			return complaintsIds;
		}

		public IEnumerable<DriverComplaintReason> GetDriverComplaintReasons(IUnitOfWork unitOfWork)
		{
			DriverComplaintReason driverComplaintReasonAlias = null;

			var query = unitOfWork.Session.QueryOver<DriverComplaintReason>();

			return query.List();
		}

		public IEnumerable<DriverComplaintReason> GetDriverComplaintPopularReasons(IUnitOfWork unitOfWork)
		{
			DriverComplaintReason driverComplaintReasonAlias = null;

			var query = unitOfWork.Session.QueryOver<DriverComplaintReason>()
						   .Where(c => c.IsPopular);

			return query.List();
		}

		public DriverComplaintReason GetDriverComplaintReasonById(IUnitOfWork unitOfWork, int driverComplaintReasonId)
		{
			return unitOfWork.GetById<DriverComplaintReason>(driverComplaintReasonId);
		}

		public ComplaintSource GetComplaintSourceById(IUnitOfWork unitOfWork, int complaintSourceId)
		{
			return unitOfWork.GetById<ComplaintSource>(complaintSourceId);
		}

		public (int, bool) GetComplaintIdByOrderRating(IUnitOfWork unitOfWork, int orderRatingId)
		{
			var query = from complaint in unitOfWork.Session.Query<Complaint>()
						where complaint.OrderRating.Id == orderRatingId
						select new ValueTuple<int, bool>(complaint.Id, true);

			return query.FirstOrDefault();
		}

		public (int, bool) GetTodayComplaintIdByOrder(IUnitOfWork unitOfWork, int orderId)
		{
			var query = from complaint in unitOfWork.Session.Query<Complaint>()
						join order in unitOfWork.Session.Query<Order>()
							on complaint.Order.Id equals orderId
						where complaint.CreationDate >= DateTime.Now.AddDays(-1)
						select new ValueTuple<int, bool>(complaint.Id, complaint.OrderRating != null);

			return query.FirstOrDefault();
		}

		public IQueryable<OksDailyReportComplaintDataNode> GetClientComplaintsForPeriod(
			IUnitOfWork uow,
			DateTime startDate,
			DateTime endDate)
		{
			var query =
				from complaint in uow.Session.Query<Complaint>()
				join c in uow.Session.Query<Counterparty>() on complaint.Counterparty.Id equals c.Id into counterpartes
				from counterparty in counterpartes.DefaultIfEmpty()
				join dp in uow.Session.Query<DeliveryPoint>() on complaint.DeliveryPoint.Id equals dp.Id into deliveryPoints
				from deliveryPoint in deliveryPoints.DefaultIfEmpty()
				join ck in uow.Session.Query<ComplaintKind>() on complaint.ComplaintKind.Id equals ck.Id into complaintKinds
				from complaintKind in complaintKinds.DefaultIfEmpty()
				join co in uow.Session.Query<ComplaintObject>() on complaintKind.ComplaintObject.Id equals co.Id into complaintObjects
				from complaintObject in complaintObjects.DefaultIfEmpty()
				where complaint.CreationDate >= startDate.Date && complaint.CreationDate <= endDate.LatestDayTime()
				&& complaint.ComplaintType == ComplaintType.Client
				orderby complaint.Id descending

				let resultComments =
				uow.Session.Query<ComplaintResultComment>()
				.Where(r => r.Complaint.Id == complaint.Id)
				.Select(r => new OksDailyReportComplaintResultCommentsData
				{
					ComplaintResultCommentId = r.Id,
					ComplaintResultComment = r.Comment
				})

				let discussionSubdivisions =
				uow.Session.Query<ComplaintDiscussion>()
				.Where(d => d.Complaint.Id == complaint.Id)
				.Select(d => new DiscussionSubdivisionData
				{
					DiscussionId = d.Id,
					ComplaintId = d.Complaint.Id,
					SubdivisionId = d.Subdivision.Id,
					DiscussionStatuse = d.Status
				})

				select new OksDailyReportComplaintDataNode
				{
					Id = complaint.Id,
					CreationDate = complaint.CreationDate,
					ComplaintText = complaint.ComplaintText,
					ComplaintResults = resultComments,
					WorkWithClientResult = complaint.WorkWithClientResult,
					Status = complaint.Status,
					ComplaintKind = complaint.ComplaintKind,
					ComplaintObject = complaint.ComplaintKind == null ? null : complaint.ComplaintKind.ComplaintObject,
					ComplaintSource = complaint.ComplaintSource,
					ClientName = counterparty == null ? string.Empty : counterparty.Name,
					DeliveryPointAddress = deliveryPoint == null ? string.Empty : deliveryPoint.CompiledAddress,
					DiscussionSubdivisions = discussionSubdivisions
				};

			return query;
		}
	}
}
