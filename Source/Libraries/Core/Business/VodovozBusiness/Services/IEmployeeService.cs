using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using Vodovoz.Parameters;

namespace Vodovoz.Services
{
	public interface IEmployeeService
	{
		Employee GetEmployeeForUser(IUnitOfWork uow, int userId);
		Employee GetEmployeeForCurrentUser();
		Employee GetEmployeeForCurrentUser(IUnitOfWork uow);
		Employee GetEmployee(int employeeId);
		Employee GetEmployee(IUnitOfWork uow, int employeeId);
		void SendCounterpartyClassificationCalculationReportToEmail(
			IUnitOfWork unitOfWork,
			IEmailParametersProvider emailParametersProvider,
			string employeeName,
			IEnumerable<string> emailAddresses,
			byte[] attachmentData);
	}
}
