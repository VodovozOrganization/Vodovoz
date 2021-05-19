using QS.DomainModel.UoW;
using System;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Employees;

namespace DriverAPI.Library.DataAccess
{
	public class EmployeeData : IEmployeeData
	{
		private readonly IUnitOfWork unitOfWork;
		private readonly IEmployeeRepository employeeRepository;

		public EmployeeData(IUnitOfWork unitOfWork, IEmployeeRepository employeeRepository)
		{
			this.unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
		}

		public void EnablePushNotifications(Employee driver, string token)
		{
			driver.AndroidToken = token;
			unitOfWork.Save(driver);
			unitOfWork.Commit();
		}

		public void DisablePushNotifications(Employee driver)
		{
			driver.AndroidToken = null;
			unitOfWork.Save(driver);
			unitOfWork.Commit();
		}

		public Employee GetByAPILogin(string login)
		{
			return employeeRepository.GetDriverByAndroidLogin(unitOfWork, login)
				?? throw new DataNotFoundException(nameof(login), $"Не найден сотрудник для логина {login}");
		}
	}
}
