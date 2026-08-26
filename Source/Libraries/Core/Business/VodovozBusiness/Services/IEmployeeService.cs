using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Domain.Employees;
using Vodovoz.Settings.Common;

namespace Vodovoz.Services
{
	public interface IEmployeeService
	{
		Employee GetEmployeeForUser(IUnitOfWork uow, int userId);
		Employee GetEmployeeForCurrentUser();
		Employee GetEmployee(int employeeId);
		Employee GetEmployee(IUnitOfWork uow, int employeeId);
		void SendCounterpartyClassificationCalculationReportToEmail(
			IUnitOfWork unitOfWork,
			IEmailSettings emailSettings,
			string employeeName,
			IEnumerable<string> emailAddresses,
			byte[] attachmentData);
		/// <summary>
		/// Получение идентификатора сотрудника и его внутреннего телефона
		/// </summary>
		/// <param name="uow">unit of work</param>
		/// <returns></returns>
		IEmployeeInnerPhone GetEmployeeInnerPhone();
	}
}
