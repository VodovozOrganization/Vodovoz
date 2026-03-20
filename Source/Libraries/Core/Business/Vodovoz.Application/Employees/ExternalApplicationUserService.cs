using QS.DomainModel.UoW;
using System;
using System.Linq;
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

		public ExternalApplicationUserService(
			IUnitOfWork unitOfWork,
			IGenericRepository<ExternalApplicationUser> externalApplicationUserRepository)
		{
			_unitOfWork = unitOfWork
				?? throw new ArgumentNullException(nameof(unitOfWork));
			_externalApplicationUserRepository = externalApplicationUserRepository
				?? throw new ArgumentNullException(nameof(externalApplicationUserRepository));
		}

		public async Task<Result<Employee>> GetExternalUserEmployee(
			string username,
			ExternalApplicationType externalApplicationType,
			CancellationToken cancellationToken)
		{
			if(string.IsNullOrWhiteSpace(username))
			{
				throw new ArgumentException($"'{nameof(username)}' cannot be null or whitespace.", nameof(username));
			}

			if(_externalApplicationUserRepository
				.Get(
					_unitOfWork,
					x => x.Login == username
						&& x.ExternalApplicationType == externalApplicationType,
					1)
				.FirstOrDefault() is ExternalApplicationUser externalApplicationUser)
			{
				return await Task.FromResult(externalApplicationUser.Employee);
			}
			else
			{
				return await Task.FromResult(VodovozBusiness.Errors.Employees.ExternalApplicationUser.NotFound);
			}
		}
	}
}
