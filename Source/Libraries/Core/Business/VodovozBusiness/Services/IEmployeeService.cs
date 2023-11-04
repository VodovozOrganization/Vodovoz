using QS.DomainModel.UoW;
using Vodovoz.Domain.Employees;
using Vodovoz.Parameters;

namespace Vodovoz.Services
{
	public interface IEmployeeService
	{
		Employee GetEmployeeForUser(IUnitOfWork uow, int userId);
		bool SendCounterpartyClassificationCalculationReportToEmail(
			IUnitOfWork unitOfWork,
			IEmailParametersProvider emailParametersProvider,
			string employeeName,
			string emailAddress,
			byte[] attachmentData);
	}
}
