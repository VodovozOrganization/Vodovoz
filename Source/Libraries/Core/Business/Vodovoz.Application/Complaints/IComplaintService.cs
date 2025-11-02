using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.Application.Complaints
{
	public interface IComplaintService
	{
		bool CheckForDuplicateComplaint(IUnitOfWork uow, Complaint complaint, DateTime checkDuplicatesFromDate, DateTime checkDuplicatesToDate);
	}
}
