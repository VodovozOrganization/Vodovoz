using System;
using System.Collections.Generic;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.EntityRepositories.Complaints.ComplaintResults
{
	public interface IComplaintResultsRepository
	{
		IEnumerable<ComplaintResultOfCounterparty> GetActiveResultsOfCounterparty(IUnitOfWork uow);
		IEnumerable<ComplaintResultOfCounterparty> GetAllResultsOfCounterparty(IUnitOfWork uow);
		IEnumerable<ComplaintResultOfEmployees> GetActiveResultsOfEmployees(IUnitOfWork uow);
		IEnumerable<ComplaintResultOfEmployees> GetAllResultsOfEmployees(IUnitOfWork uow);
		IList<ClosedComplaintResultNode> GetComplaintsResultsOfCounterparty(IUnitOfWork uow, DateTime? start = null, DateTime? end = null);
		IList<ClosedComplaintResultNode> GetComplaintsResultsOfEmployees(IUnitOfWork uow, DateTime? start = null, DateTime? end = null);
	}
}
