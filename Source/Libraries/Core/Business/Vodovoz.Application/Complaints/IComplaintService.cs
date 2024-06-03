using QS.DomainModel.UoW;

namespace Vodovoz.Application.Complaints
{
	public interface IComplaintService
	{
		bool CheckForDuplicateComplaint(IUnitOfWork uow, int? orderId);
	}
}
