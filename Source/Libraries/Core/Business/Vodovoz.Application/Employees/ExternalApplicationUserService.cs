using QS.DomainModel.UoW;
using System;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Domain.Employees;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Core.Domain.Results;
using Vodovoz.Domain.Employees;
using VodovozBusiness.Employees;

namespace Vodovoz.Application.Employees
{
	internal sealed class ExternalApplicationUserService : IExternalApplicationUserService
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IGenericRepository<ExternalApplicationUser> _externalApplicationUserRepository;
		private readonly IGenericRepository<EmployeeEntity> _employeeRepository;

		public ExternalApplicationUserService(
			IUnitOfWork unitOfWork,
			IGenericRepository<ExternalApplicationUser> externalApplicationUserRepository,
			IGenericRepository<EmployeeEntity> employeeRepository)
		{
			_unitOfWork = unitOfWork
				?? throw new ArgumentNullException(nameof(unitOfWork));
			_externalApplicationUserRepository = externalApplicationUserRepository
				?? throw new ArgumentNullException(nameof(externalApplicationUserRepository));
			_employeeRepository = employeeRepository
				?? throw new ArgumentNullException(nameof(employeeRepository));
		}

		public async Task<Result<EmployeeEntity>> GetExternalUserEmployee(
			string username,
			ExternalApplicationType externalApplicationType,
			CancellationToken cancellationToken)
		{
			if(_externalApplicationUserRepository
				.GetFirstOrDefault(
					_unitOfWork,
					x => x.Login == username
						&& x.ExternalApplicationType == externalApplicationType) is ExternalApplicationUser externalApplicationUser
				&& _employeeRepository.GetFirstOrDefault(
					_unitOfWork,
					x => x.Id == externalApplicationUser.Id) is EmployeeEntity employee)
			{
				return await Task.FromResult(employee);
			}
			else
			{
				return await Task.FromResult(VodovozBusiness.Errors.Employees.ExternalApplicationUser.NotFound);
			}
		}
	}
}
