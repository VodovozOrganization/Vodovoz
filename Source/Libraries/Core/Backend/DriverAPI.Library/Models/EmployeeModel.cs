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

		public void EnablePushNotifications(ExternalApplicationUser driverAppUser, string token)
		{
			driverAppUser.Token = token;
			_unitOfWork.Save(driverAppUser);
			_unitOfWork.Commit();
		}

		public void DisablePushNotifications(ExternalApplicationUser driverAppUser)
		{
			driverAppUser.Token = null;
			_unitOfWork.Save(driverAppUser);
			_unitOfWork.Commit();
		}

		public Employee GetByAPILogin(string login)
		{
			return _employeeRepository.GetEmployeeByAndroidLogin(_unitOfWork, login)
				?? throw new DataNotFoundException(nameof(login), $"Не найден сотрудник для логина { login }");
		}

		public IEnumerable<Employee> GetAllPushNotifiableEmployees()
		{
			return _employeeRepository.GetSubscribedToPushNotificationsDrivers(_unitOfWork);
		}
	}
}
