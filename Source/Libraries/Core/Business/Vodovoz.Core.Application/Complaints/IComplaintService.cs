using System;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Complaints;

namespace Vodovoz.Core.Application.Complaints
{
	public interface IComplaintService
	{
		bool CheckForDuplicateComplaint(IUnitOfWork uow, Complaint complaint, DateTime checkDuplicatesFromDate, DateTime checkDuplicatesToDate);
	}
}
