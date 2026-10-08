using QS.DomainModel.UoW;
using System;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Results;
using Vodovoz.EntityRepositories.Employees;

namespace DriverAPI.Library.Services
{
	/// <inheritdoc />
	public class DriverAuthenticationService : IDriverAuthenticationService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEmployeeRepository _employeeRepository;

		/// <summary>
		/// Создаёт сервис проверки доступа сотрудника.
		/// </summary>
		/// <param name="unitOfWork">Контекст работы с данными сотрудников.</param>
		/// <param name="employeeRepository">Репозиторий сотрудников.</param>
		public DriverAuthenticationService(IUnitOfWork unitOfWork, IEmployeeRepository employeeRepository)
		{
			_unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
			_employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
		}

		/// <inheritdoc />
		public Result ValidateEmployeeAccess(string login)
		{
			var employee = _employeeRepository.GetEmployeeByAndroidLogin(_unitOfWork, login, ExternalApplicationType.DriverApp);

			return employee?.Status == EmployeeStatus.IsFired
				? Result.Failure(Errors.Security.Authorization.EmployeeIsFired)
				: Result.Success();
		}
	}
}
