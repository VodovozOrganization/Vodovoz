using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using Vodovoz.Parameters;

namespace Vodovoz.Services
{
	public interface IEmployeeService
	{
		Employee GetEmployeeForUser(IUnitOfWork uow, int userId);
		void SendCounterpartyClassificationCalculationReportToEmail(
			IUnitOfWork unitOfWork,
			IEmailParametersProvider emailParametersProvider,
			string employeeName,
			IEnumerable<string> emailAddresses,
			byte[] attachmentData);
	}
}
