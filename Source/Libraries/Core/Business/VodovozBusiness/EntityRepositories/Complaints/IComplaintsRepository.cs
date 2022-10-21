using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Complaints;
using Vodovoz.EntityRepositories.Complaints.ComplaintResults;

namespace Vodovoz.EntityRepositories.Complaints
{
	public interface IComplaintsRepository
	{
		IList<object[]> GetGuiltyAndCountForDates(IUnitOfWork uow, DateTime? start = null, DateTime? end = null);
		int GetUnclosedComplaintsCount(IUnitOfWork uow, bool? withOverdue = null, DateTime? start = null, DateTime? end = null);
		IEnumerable<DriverComplaintReason> GetDriverComplaintReasons(IUnitOfWork unitOfWork);
        IEnumerable<DriverComplaintReason> GetDriverComplaintPopularReasons(IUnitOfWork unitOfWork);
		DriverComplaintReason GetDriverComplaintReasonById(IUnitOfWork unitOfWork, int driverComplaintReasonId);
        ComplaintSource GetComplaintSourceById(IUnitOfWork unitOfWork, int complaintSourceId);
    }
}
