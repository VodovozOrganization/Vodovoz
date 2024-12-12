using DriverAPI.Library.Exceptions;
using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Employees;
using Vodovoz.EntityRepositories.Employees;

namespace DriverAPI.Library.V5.Services
{
	internal class EmployeeService : IEmployeeService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmployeeRepository _employeeRepository;
		private readonly IGenericRepository<ExternalApplicationUser> _externalApplicationUserRepository;

		public EmployeeService(
			IUnitOfWork unitOfWork,
			IEmployeeRepository employeeRepository,
			IGenericRepository<ExternalApplicationUser> externalApplicationUserRepository)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
			_externalApplicationUserRepository = externalApplicationUserRepository ?? throw new ArgumentNullException(nameof(externalApplicationUserRepository));
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
				?? throw new DataNotFoundException(nameof(login), $"Не найден сотрудник для логина {login}");
		}

		public IEnumerable<Employee> GetAllPushNotifiableEmployees()
		{
			return _employeeRepository.GetSubscribedToPushNotificationsDrivers(_unitOfWork);
		}

		public string GetDriverPushTokenById(int notifyableEmployeeId)
		{
			return _employeeRepository.GetDriverPushTokenById(_unitOfWork, notifyableEmployeeId);
		}

		public ExternalApplicationUser GetDriverExternalApplicationUserByFirebaseToken(string recipientToken)
		{
			return _externalApplicationUserRepository.Get(_unitOfWork, eau => eau.Token == recipientToken).FirstOrDefault();
		}
	}
}
