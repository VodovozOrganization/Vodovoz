using Microsoft.Extensions.Logging;
using QS.DomainModel.UoW;
using QS.Utilities.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.CachingRepositories.Common;
using Vodovoz.Core.Domain.Repositories;
using Vodovoz.Domain.Employees;

namespace VodovozBusiness.CachingRepositories.Employees
{
	internal sealed class EmployeeInMemoryNameWithInitialsCacheRepository :
		DomainEntityNodeInMemoryCacheRepositoryBase<Employee>, IEmployeeInMemoryNameWithInitialsCacheRepository
	{
		private readonly IGenericRepository<Employee> _employeeRepository;

		public EmployeeInMemoryNameWithInitialsCacheRepository(
			ILogger<EmployeeInMemoryNameWithInitialsCacheRepository> logger,
			IUnitOfWorkFactory unitOfWorkFactory,
			IGenericRepository<Employee> employeeRepository)
			: base(logger, unitOfWorkFactory)
		{
			_employeeRepository = employeeRepository
				?? throw new ArgumentNullException(nameof(employeeRepository));
		}

		protected override Employee GetEntityById(int id) =>
			_employeeRepository
				.Get(
					_unitOfWork,
					x => x.Id == id,
					limit: 1)
				.FirstOrDefault();

		protected override IDictionary<int, string> GetTitlesByIdsFromDatabase(ICollection<int> ids) =>
			_employeeRepository
				.GetValue(
					_unitOfWork,
					x => new {
						x.Id,
						Name = PersonHelper.PersonNameWithInitials(x.LastName, x.Name, x.Patronymic)
					},
					x => ids.Contains(x.Id))
				.ToDictionary(x => x.Id, x => x.Name);
	}
}
