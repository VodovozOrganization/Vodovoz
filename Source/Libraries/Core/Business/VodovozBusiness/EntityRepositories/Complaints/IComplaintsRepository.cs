using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient.Memcached;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Complaints;
using VodovozBusiness.Domain.Complaints;

namespace Vodovoz.EntityRepositories.Complaints
{
	public interface IComplaintsRepository
	{
		IList<object[]> GetGuiltyAndCountForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null);
		int GetUnclosedComplaintsCount(IUnitOfWork uow, bool? withOverdue = null, DateTime? start = null, DateTime? end = null);

		/// <summary>
		/// Возвращает список id незакрытых рекламаций, в которых подключена дискуссия для указанного
		/// отдела, но комментарии в ней отсутствуют
		/// </summary>
		/// <param name="uow">UnitOfWork</param>
		/// <param name="subdivisionId">id отдела для которого выполняется поиск рекламаций</param>
		/// <returns>Список id рекламаций</returns>
		IList<int> GetUnclosedWithNoCommentsComplaintIdsBySubdivision(IUnitOfWork uow, int subdivisionId);

		IEnumerable<DriverComplaintReason> GetDriverComplaintReasons(IUnitOfWork unitOfWork);
		IEnumerable<DriverComplaintReason> GetDriverComplaintPopularReasons(IUnitOfWork unitOfWork);
		DriverComplaintReason GetDriverComplaintReasonById(IUnitOfWork unitOfWork, int driverComplaintReasonId);
		ComplaintSource GetComplaintSourceById(IUnitOfWork unitOfWork, int complaintSourceId);
		(int, bool) GetComplaintIdByOrderRating(IUnitOfWork unitOfWork, int orderRatingId);
		(int, bool) GetTodayComplaintIdByOrder(IUnitOfWork unitOfWork, int orderId);
		IQueryable<OksDailyReportComplaintDataNode> GetClientComplaintsForPeriod(IUnitOfWork uow, DateTime startDate, DateTime endDate, int oksSubdivisionId);
	}

	public class OksDailyReportComplaintDataNode
	{
		public int Id { get; set; }
		public DateTime CreationDate { get; set; }
		public ComplaintWorkWithClientResult? WorkWithClientResult { get; set; }
		public ComplaintStatuses Status { get; set; }
		public ComplaintKind ComplaintKind { get; set; }
		public ComplaintObject ComplaintObject { get; set; }
		public ComplaintSource ComplaintSource { get; set; }
		public ComplaintDiscussionStatuses OksDiskussionStatuse { get; set; }
		public string ClientName {  get; set; }
		public string DeliveryPointAddress { get; set; }
	}
}
