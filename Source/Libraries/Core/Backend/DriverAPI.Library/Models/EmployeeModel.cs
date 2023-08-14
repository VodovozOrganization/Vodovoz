using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Employees;

namespace DriverAPI.Library.Models
{
	internal class EmployeeModel : IEmployeeModel
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmployeeRepository _employeeRepository;

		public EmployeeModel(IUnitOfWork unitOfWork, IEmployeeRepository employeeRepository)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
		}

		public void EnablePushNotifications(Employee driver, string token)
		{
			driver.AndroidToken = token;
			_unitOfWork.Save(driver);
			_unitOfWork.Commit();
		}

		public void DisablePushNotifications(Employee driver)
		{
			driver.AndroidToken = null;
			_unitOfWork.Save(driver);
			_unitOfWork.Commit();
		}

		public Employee GetByAPILogin(string login)
		{
			return _employeeRepository.GetDriverByAndroidLogin(_unitOfWork, login)
				?? throw new DataNotFoundException(nameof(login), $"Не найден сотрудник для логина { login }");
		}

		public IEnumerable<Employee> GetAllPushNotifiableEmployees()
		{
			return _employeeRepository.GetSubscribedToPushNotificationsDrivers(_unitOfWork);
		}
	}
}
